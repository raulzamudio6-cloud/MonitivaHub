using eu.advapay.core.hub.rr_log;
using Newtonsoft.Json.Linq;
using NLog;
using System;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;

namespace eu.advapay.core.hub.western_union
{
    public sealed class WesternUnionGate
    {
        private readonly string _url;
        private readonly string _certificatePath;
        private readonly string _certificatePassword;

        private X509Certificate2Collection certificates;

        private static readonly ILogger log = LogManager.GetCurrentClassLogger();
        private static readonly ILogger archive = LogManager.GetLogger("archive");


        public WesternUnionGate(string url, string certificatePath, string certificatePassword)
        {
            _url = url;
            _certificatePath = certificatePath;
            _certificatePassword = certificatePassword;

            certificates = new X509Certificate2Collection();
            if (!"".Equals(_certificatePassword))
            {
                certificates.Import(_certificatePath, _certificatePassword, X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet);
            }
            else
            {
                certificates.Import(_certificatePath);
            }
            if (certificates.Count == 0)
            {
                log.Error("Certificate not found");
                throw new Exception("Certificate not found");
            }
        }

        public static bool certificateValidationCallback(object sender, X509Certificate certificate,
                                    X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        public JObject SendRequest(string url, string requestMethod, string request)
        {
            //return new JObject() { { "re", -1 },
            //    {
            //        "response", JObject.Parse("{id:12345,status:\"NotAccepted\",errorCode:\"1003:thirdPartyRemitter.bankAccount.BankAccount\"}")
            //    }
            //};

            string response;
            JObject result = new JObject();
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(certificateValidationCallback);


            HttpWebRequest rq = (HttpWebRequest)WebRequest.Create(_url + url);

            try
            {
                rq.ClientCertificates = certificates;
                rq.Method = requestMethod;
                rq.Proxy = WebRequest.DefaultWebProxy;
                rq.ServicePoint.Expect100Continue = false;
                rq.Accept = "application/json";
                rq.ContentType = "application/json";
                rq.Timeout = 60000;

                MappedDiagnosticsLogicalContext.Set("req_id", DateTime.Now.Ticks + "_" + Thread.CurrentThread.ManagedThreadId);

                var msg1 = new LogEventInfo(LogLevel.Info, "", "log");
                msg1.Properties.Add("sys", "wu");
                msg1.Properties.Add("req", "[{\"" + requestMethod + "\":\"" + _url + url + "\"}, " + request + "]");
                archive.Info(msg1);

                ReqRespLog.Save("wu", true, "[{\"" + requestMethod + "\":\"" + _url + url + "\"}, " + request + "]");


                if (!string.IsNullOrEmpty(request))
                {

                    byte[] btSend = Encoding.UTF8.GetBytes(request);
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
                log.Debug(requestMethod + ":" + url + " request took " +(respRead.Subtract(reqStart).TotalMilliseconds)+"ms"+
                    "  (read in "+ (respRead.Subtract(respStart).TotalMilliseconds) + "ms");

                LogEventInfo msg = new LogEventInfo(LogLevel.Info, "", "log");
                msg.Properties.Add("sys", "wu");
                msg.Properties.Add("resp", response);
                archive.Info(msg);

                ReqRespLog.Save("wu", false, response);

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
                msg.Properties.Add("sys", "wu");
                msg.Properties.Add("resp", e.Message);
                archive.Info(msg);

                ReqRespLog.Save("wu", false, e.Message);
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
                msg.Properties.Add("sys", "wu");
                msg.Properties.Add("resp", "[{\"exception\":\"" + e + "\"}, " + response + "]" );
                archive.Info(msg);

                ReqRespLog.Save("wu", false, "[{\"exception\":\"" + e + "\"}, " + response + "]");
            }
            catch (Exception e)
            {
                result.Add("re", -4);
                result.Add("error", $"[EXEPTION] {e}");

                var msg = new LogEventInfo(LogLevel.Info, "", "log");
                msg.Properties.Add("sys", "wu");
                msg.Properties.Add("resp", e.Message);
                archive.Info(msg);

                ReqRespLog.Save("wu", false, e.Message);
            }
            log.Trace("resp===" + result.ToString());
            return result;
        }
    }
}
