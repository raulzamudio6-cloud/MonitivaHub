using eu.advapay.core.hub.rr_log;
using Newtonsoft.Json.Linq;
using NLog;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;

namespace eu.advapay.core.hub.currency_cloud
{
    internal sealed class CurrencyCloudGate
    {
        private readonly string _url;
        private readonly string _loginId;
        private readonly string _apiKey;

        private string authToken = "";
        private DateTime lastRequestTime = DateTime.MinValue;

        private static readonly ILogger log = LogManager.GetCurrentClassLogger();
        private static readonly ILogger archive = LogManager.GetLogger("archive");

        public CurrencyCloudGate(string url, string loginId, string apiKey)
        {
            _url = url;
            _loginId = loginId;
            _apiKey = apiKey;
        }

        public bool login()
        {
            NameValueCollection req = new NameValueCollection();
            req.Add("login_id", _loginId);
            req.Add("api_key" , _apiKey);
            JObject resp = SendRequestInternal("/v2/authenticate/api", "POST", req);
            if(resp.SelectToken("response")!=null) authToken = (string)resp["response"]["auth_token"];
            return (int)resp["re"] == 0;
        }
        public bool logout()
        {
            JObject resp = SendRequestInternal("/v2/authenticate/close_session", "POST", null);
            authToken = "";
            return (int)resp["re"] == 0;
        }

        public static bool certificateValidationCallback(object sender, X509Certificate certificate,
                                    X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        public JObject SendRequest(string url, string requestMethod, NameValueCollection request)
        {
            
            if (DateTime.Now - lastRequestTime > TimeSpan.FromMinutes(20))
            {
                if (!"".Equals(authToken)) logout();
            }
            if ("".Equals(authToken)) login();

            JObject resp = null;
            if (!"".Equals(authToken)) 
                resp = SendRequestInternal(url, requestMethod, request);

            if (resp != null  && resp.SelectToken("statusCode") != null)
            {
                if ((int)resp["statusCode"] == 401)
                {
                    login();
                    if (!"".Equals(authToken)) resp = SendRequestInternal(url, requestMethod, request);

                }
            }

            return resp;
        }

        public JObject SendRequestInternal(string url, string requestMethod, NameValueCollection request)
        {
            string response;
            JObject result = new JObject();

            NameValueCollection queryString = System.Web.HttpUtility.ParseQueryString(string.Empty);
            if (request != null) queryString.Add(request);

            string requestStr = queryString.ToString();

            if(requestMethod.Equals("GET"))
            {
                url = url + "?" + requestStr;
                requestStr = "";
            }
            HttpWebRequest rq = (HttpWebRequest)WebRequest.Create(_url + url);

            try
            {
                rq.Method = requestMethod;
                rq.Proxy = WebRequest.DefaultWebProxy;
                rq.ServicePoint.Expect100Continue = false;
                rq.Accept = "application/json";
                rq.ContentType = "application/x-www-form-urlencoded";
                rq.Headers.Add("X-Auth-Token", authToken);
                rq.Timeout = 60000;

                MappedDiagnosticsLogicalContext.Set("req_id", DateTime.Now.Ticks + "_" + Thread.CurrentThread.ManagedThreadId);

                var msg1 = new LogEventInfo(LogLevel.Info, "", "log");
                msg1.Properties.Add("sys", "cc");
                msg1.Properties.Add("req",  "[{\"" + requestMethod + "\":\"" + _url + url + "\"}, {\"x-auth\":\"" + authToken+ "\", \"body\":\"" 
                     + requestStr + "\"}]");
                archive.Info(msg1);

                ReqRespLog.Save("cc", true, "[{\"" + requestMethod + "\":\"" + _url + url + "\"}, {\"x-auth\":\"" + authToken + "\", \"body\":\"" 
                     + requestStr + "\"}]");


                if (!string.IsNullOrEmpty(requestStr))
                {

                    byte[] btSend = Encoding.UTF8.GetBytes(requestStr);
                    rq.ContentLength = btSend.Length;
                    using (Stream requestStream = rq.GetRequestStream())
                        requestStream.Write(btSend, 0, btSend.Length);
                }
                else
                {
                    rq.ContentLength = 0;
                }

                DateTime reqStart = DateTime.Now;
                HttpWebResponse resp = (HttpWebResponse)rq.GetResponse();
                
                DateTime respStart = DateTime.Now;
                using (StreamReader rd = new StreamReader(resp.GetResponseStream(), Encoding.UTF8))
                    response = rd.ReadToEnd();
                
                DateTime respRead = DateTime.Now;
                log.Trace(requestMethod + ":" + url + " request took " +(respRead.Subtract(reqStart).TotalMilliseconds)+"ms"+
                    "  (read in "+ (respRead.Subtract(respStart).TotalMilliseconds) + "ms");

                LogEventInfo msg = new LogEventInfo(LogLevel.Info, "", "log");
                msg.Properties.Add("sys", "cc");
                msg.Properties.Add("resp", response);
                archive.Info(msg);

                ReqRespLog.Save("cc", false, response);
                
                lastRequestTime = DateTime.Now;

                try
                {
                    if (response == "")
                        response = "{}";
                    JObject jsonResponse = JObject.Parse(response);
                    result.Add("re", 0);
                    result.Add("response", jsonResponse);
                }
                catch (Exception e)
                {
                    result.Add("re", -5);
                    result.Add("error", $"[NOTJSONRESPONSE] {e}");
                    result.Add("response", response);
                }
            }
            catch (TimeoutException e)
            {
                result.Add("re", -2);
                result.Add("error", $"[TIMEOUT] {e}");

                var msg = new LogEventInfo(LogLevel.Info, "", "log");
                msg.Properties.Add("sys", "cc");
                msg.Properties.Add("resp", e.Message);
                archive.Info(msg);

                ReqRespLog.Save("cc", false, e.Message);
            }
            catch (WebException e)
            {
                response = "";
                if (e.Response != null)
                    using (StreamReader rd = new StreamReader(e.Response.GetResponseStream(), Encoding.UTF8))
                        response = rd.ReadToEnd();

                result.Add("re", -3);
                result.Add("error", $"[WEBEXECPTION] {response} {e.Message}");
                if(e.Response!=null) result.Add("statusCode", (int)((HttpWebResponse)e.Response).StatusCode);

                var msg = new LogEventInfo(LogLevel.Info, "", "log");
                msg.Properties.Add("sys", "cc");
                msg.Properties.Add("resp", "[{\"exception\":\"" + e + "\"}, " + response + "]" );
                archive.Info(msg);

                ReqRespLog.Save("cc", false, "[{\"exception\":\"" + e + "\"}, " + response + "]");
            }
            catch (Exception e)
            {
                result.Add("re", -4);
                result.Add("error", $"[EXEPTION] {e}");

                var msg = new LogEventInfo(LogLevel.Info, "", "log");
                msg.Properties.Add("sys", "cc");
                msg.Properties.Add("resp", e.Message);
                archive.Info(msg);

                ReqRespLog.Save("cc", false, e.Message);
            }
            return result;
        }
    }

}
