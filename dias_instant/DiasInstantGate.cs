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

namespace eu.advapay.core.hub.dias_instant
{
    internal sealed class DiasInstantGate
    {
        private readonly string _url;

        //private string authToken = "";
        private DateTime lastRequestTime = DateTime.MinValue;

        private static readonly ILogger log = LogManager.GetCurrentClassLogger();
        private static readonly ILogger archive = LogManager.GetLogger("archive");

        public DiasInstantGate(string url)
        {
            _url = url;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(certificateValidationCallback);
        }



        public static bool certificateValidationCallback(object sender, X509Certificate certificate,
                                    X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        public JObject SendRequest(string url, string requestMethod, string request)
        {
            return SendRequestInternal(url, requestMethod, request);
        }

        public JObject SendRequestInternal(string url, string requestMethod, string request)
        {
            string response;
            JObject result = new JObject();


            //NameValueCollection queryString = System.Web.HttpUtility.ParseQueryString(string.Empty);
            //if (request != null) queryString.Add(request);

            string requestStr = request; // queryString.ToString();

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
                rq.ContentType = "application/json";
                //rq.Headers.Add("X-Auth-Token", authToken);
                rq.Timeout = 60000;

                MappedDiagnosticsLogicalContext.Set("req_id", DateTime.Now.Ticks + "_" + Thread.CurrentThread.ManagedThreadId);

                var msg1 = new LogEventInfo(LogLevel.Info, "", "log");
                msg1.Properties.Add("sys", "di");
                msg1.Properties.Add("req",  "[{\"" + requestMethod + "\":\"" + _url + url + "\"}, {\"body\":\"" 
                     + requestStr + "\"}]");
                archive.Info(msg1);

                ReqRespLog.Save("di", true, "[{\"" + requestMethod + "\":\"" + _url + url + "\"}, {\"body\":\"" 
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
                msg.Properties.Add("sys", "di");
                msg.Properties.Add("resp", response);
                archive.Info(msg);

                ReqRespLog.Save("di", false, response);
                
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
