using System;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.IO;
using NLog;
using System.Collections.Generic;
using eu.advapay.core.hub.rr_log;

namespace eu.advapay.core.hub.openapi
{
    public class OpenAPI
    {
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();
        private static readonly ILogger archive = LogManager.GetLogger("archive");
        public readonly OpenAPIGate _oaGate;

        public OpenAPI(OpenAPIGate oaGate)
        {
            log.Debug("OpenAPI");

        }
        // int - edrror code
        // string - JSON array with instructions to integration hub
        // Stream - will be written to response attachments in DB   or  passed as body to http response
        public async Task<(int, JArray, Stream)> OAFunctionAsync(JObject taskParams, JObject jdata, string http_request)
        {

            JArray result = new JArray();
            int ii;


            MappedDiagnosticsLogicalContext.Set("req_id", null);
            MappedDiagnosticsLogicalContext.Set("req_type", null);
            if(jdata!= null && jdata.ContainsKey("id") && int.TryParse(jdata["id"].ToString(),out ii))
            {
                if (ii > 0) { MappedDiagnosticsLogicalContext.Set("doc", ii); }
                else { MappedDiagnosticsLogicalContext.Set("doc", null); }
            }
                

            string action = "";

            try
            {

                if (taskParams["action"] != null)
                    action = taskParams["action"].ToString();

                log.Debug($"action = " + action + " jdata="+jdata.ToString());
                if (action.Equals("http_request"))
                {
                    MappedDiagnosticsLogicalContext.Set("req_type", "http_request");
                    log.Trace("http_request: " + http_request);
//                    log.Debug("jdata: " + jdata.ToString());

                    if (jdata == null)
                    {
                        result.Add(new JObject()
                        {
                            {"module","http_response"},
                            {"HttpCode","400"},
                            {"X-Mab-Error","запрос не JSON"},
                            {"Content-Type","text/plain; charset= utf-8"}
                        });
                        return (4, result, new MemoryStream(Encoding.UTF8.GetBytes("request is not JSON")));
                    }

                    log.Debug($"before SelectToken");
                    if (jdata.SelectToken("resource.id") != null/* && int.TryParse(jdata["resource"]["id"].ToString(), out ii)*/)
                    {
                        if ( int.TryParse(jdata["resource"]["id"].ToString(), out ii))
                        log.Debug("[resource].[id] = " + ii.ToString());
                        MappedDiagnosticsLogicalContext.Set("doc", jdata["resource"]["id"]);
                    }
                    else if (jdata.SelectToken("resource.id") != null && int.TryParse(jdata["resource"][0]["id"].ToString(), out ii))
                    {
                        log.Debug("[resource][0][id] = " + ii.ToString());
                        MappedDiagnosticsLogicalContext.Set("doc", jdata["resource"][0]["id"]);
                    }

                    log.Debug("new LogEventInfo");
                    var msg1 = new LogEventInfo(LogLevel.Info, "", "log");
                    msg1.Properties.Add("sys", "oa");
                    msg1.Properties.Add("resp", http_request);
                    archive.Info(msg1);

                    log.Debug("ReqRespLog.Save");

                    ReqRespLog.Save("oa", false, http_request);


                    if (jdata.ContainsKey("eventType"))
                    {
                        string eventType = jdata.SelectToken("eventType")?.ToString();
                        log.Debug("eventType = " + eventType);
                        if ("payment.statusChanged".Equals(eventType) || "payment.notAccepted".Equals(eventType))
                        {
                            log.Debug("payment.statusChanged || payment.notAccepted jdata= " + jdata.ToString());
                            foreach (JObject res in WebHookPaymentStatusChanged(jdata))
                                result.Add(res);
                        }
                        else if ("order.statusChanged".Equals(eventType) || "payment.notAccepted".Equals(eventType))
                        {
                            log.Debug("order.statusChanged || payment.notAccepted jdata" + jdata.ToString());
                            foreach (JObject res in WebHookOrderStatusChanged(jdata))
                                result.Add(res);
                            log.Debug("order.statusChanged res= " + result.ToString());
                        }
                        //log.Debug("return = " + result.ToString());

                        return (0, result, null);
                    }

                    // Stream here is what returned as http response body
                    log.Debug("http_request return 4 = " + http_request);
                    return (4, result, new MemoryStream(Encoding.Unicode.GetBytes("req запрос:"+ http_request)));

                }
                else
                {
                    result.Add(new JObject()
                    {
                        {"module","integrations"},
                        {"hook","apiError"},
                        {"error", "unknown action: " + action},
                        {"document",jdata?["id"]}
                    });

                    return (3, result, null);
                }
            }
            catch (Exception e)
            {
                log.Error(e, "error");
                MemoryStream ms = null;
                if (action.Equals("http_request"))
                {
                    result.Add(new JObject()
                    {
                        {"module","http_response"},
                        {"HttpCode","500"},
                        {"X-Mab-Error","1"},
                        {"Content-Encoding","utf-8"},
                        {"Content-Type","text/plain; charset= utf-8"}
                    });
                    ms = new MemoryStream(Encoding.UTF8.GetBytes(Utils.ConvertExceptionToString(e)));
                }
                result.Add(new JObject()
                {
                    {"module","integrations"},
                    {"hook","apiError"},
                    {"error", Utils.ConvertExceptionToString(e)},
                    {"document",jdata?["id"]}
                });
                return (2, result, ms);
            }


        }

        public JObject[] WebHookPaymentStatusChanged(JObject jdata)
        {
            List<JObject> results = new List<JObject>();
            JArray payments;

            if (jdata.SelectToken("resource") is JArray)
            {
                payments = (JArray)jdata.SelectToken("resource");
                log.Debug($"WebHookPaymentStatusChanged1 ='" + payments.ToString() + "'");
            }
            else if (jdata.SelectToken("resource") != null)
            {
                payments = new JArray();
                payments.Add(jdata.SelectToken("resource"));
                log.Debug($"WebHookPaymentStatusChanged2 ='" + payments.ToString() + "'");
            }
            else {
                results.Add(new JObject()
                    {
                        {"module","http_response"},
                        {"HttpCode","400"},
                        {"X-Mab-Error","1"},
                        {"X-Mab-Error-Text","resource element not found"},
                        {"Content-Encoding","utf-8"},
                        {"Content-Type","text/plain; charset= utf-8"}
                    });
                log.Debug($"WebHookPaymentStatusChanged3 ='" + results.ToArray().ToString() + "'");
                return results.ToArray();
            }

            foreach (JObject paymentStatus in payments)
            {
                string externalReference = paymentStatus.SelectToken("id")?.ToString();
                string status = paymentStatus.SelectToken("status")?.ToString();
                string errorCode = paymentStatus.SelectToken("errorCode")?.ToString();
                string hook = ConvertPaymentStatus(status);
                log.Debug($"WebHookPaymentStatusChanged1 externalReference='" + externalReference + " status = " + status
                    + " errorCode=" + errorCode + " hook="+ hook);

                ResponseChecker responseChecker = new ResponseChecker(jdata);
                responseChecker.ParseErrorText(errorCode);


                JObject res = new JObject()
                        {
                            {"module","integrations"},
                            {"integration","openapi"},
                            {"hook",hook},
                            //{"document",externalReference},
                            {"externalReference",externalReference}
                        };

                if (!hook.Equals(""))
                {
                    if(!"".Equals(""+responseChecker.errorText)) 
                        res["error"] = responseChecker.errorText;
                }

                if ("".Equals(hook) && !"".Equals(responseChecker.errorText))
                {
                    hook = "apiError";
                    res["error"] = responseChecker.errorText;
                }

                if ("".Equals(hook))
                    hook = "updateLog";

                res["hook"] = hook;
                res["message"] = "Provider status: " + status;

                results.Add(res);
            }

            return results.ToArray();
        }

        public JObject[] WebHookOrderStatusChanged(JObject jdata)
        {
            JObject result = new JObject();

            string externalReference = jdata.SelectToken("resource.id")?.ToString();
            string status = jdata.SelectToken("resource.status")?.ToString();
            string errorCode = jdata.SelectToken("resource.errorCode")?.ToString();

            string hook = "";

            hook = ConvertOrderStatus(status);

            ResponseChecker responseChecker = new ResponseChecker(jdata);
            responseChecker.ParseErrorText(errorCode);


            if (!hook.Equals(""))
            {
                return new JObject[] {
                    new JObject()
                    {
                        {"module","integrations"},
                        {"integration","openapi"},
                        {"hook",hook},
                        {"error",responseChecker.errorText},
                        {"externalReference",externalReference}
                    }
                };
            }

            return new JObject[] { };
        }

        private string ConvertOrderStatus(string oaStatus)
        {
            string result = "";
            switch (oaStatus)
            {
                //Empty or not final status
                case "Committed":
                    result = "sentToBank";
                    break;
                case "Received":
                    result = "accepted";
                    break;
                case "Funded":
                    result = "settled";
                    break;
                case "SUCCESS":
                    result = "success";
                    break;
            }

            return result;
        }

        /*        public JObject[] PingWU(JObject payment)
                {
                    JObject result = new JObject();

                    DateTime reqStart = DateTime.Now;
                    JObject response = _westernUnionGate.SendRequest("/ping", "GET","");
                    DateTime respRead = DateTime.Now;

                    result.Add("hook", "ping");
                    result.Add("time", (respRead.Subtract(reqStart).TotalMilliseconds) + "ms");
                    result.Add("response", response.ToString());
                    return new JObject[] { result };
                }*/

        private string ConvertPaymentStatus(string oaStatus)
        {
            string result = "";
            switch (oaStatus)
            {
                //Empty or not final status
                case "OnHold":
                case "Created":
                case "NOC":
                case "NoticeOfChange":
                case "Prohibited":
                    break;
                // Error statuses
                case "NotAccepted":
                    result = "providerError";
                    break;
                //business statuses
                case "ReleasedPaymentRejected":
                case "Rejected":
                    result = "rejected";
                    break;
                case "Processing":
                    result = "accepted";
                    break;
                case "ReleasedPaymentCancelled":
                case "Cancelled":
                    result = "cancelled";
                    break;
                case "ReleasedBeneficiaryAccountCredited":
                case "ReleasedInProcess":
                case "ReleasedNoLongerTraceable":
                case "Released":
                    result = "settled";
                    break;
                case "Returned":
                    result = "returned";
                    break;
            }

            return result;
        }


        private decimal GetAmount(int Amount, string Currency)
        {
            int multi = int.Parse(Math.Pow(10, CurrencyMinorUnit.getInstance().GetMinorUnit(Currency)).ToString());
            return decimal.Divide(Amount, multi);
        }
        public JObject[] SetupWebhooks(JObject webhooks)
        {
            JObject result = new JObject();

            MappedDiagnosticsLogicalContext.Set("req_type", "WebhooksSetup");
            log.Debug($"WebhooksSetup {webhooks.ToString()}");

            Dictionary<string, bool> newHooks = new Dictionary<string, bool>();
            if (webhooks.SelectToken("webhooks") is JArray)
            {
                foreach(JObject hook in (JArray)webhooks.SelectToken("webhooks"))
                {
                    newHooks[(string)hook["url"]] = hook.SelectToken("isPrimary") != null ? (bool)hook["isPrimary"] : false;
                }
            }

            // request
            /*            JObject response = _oaGate.SendRequest($"/webhooks", "GET", "");
                        ResponseChecker responseChecker = new ResponseChecker(webhooks);
                        JObject check = responseChecker.CheckResponse(response);

                        // Parse Response
                        Dictionary<string, bool> existingHooks = new Dictionary<string, bool>();
                        if (responseChecker.statusCode == 404)
                        {
                            // no webhooks yet
                        }
                        else if (responseChecker.error) return new JObject[] { check };
                        else
                        {
                            if (response.SelectToken("response.webhooks") is JArray)
                            {
                                foreach (JObject hook in (JArray)response.SelectToken("response.webhooks"))
                                {
                                    string id = hook.SelectToken("id") != null ? (string)hook["id"] : "";
                                    // delete hook request
                                    response = _oa.SendRequest($"/webhooks/"+ id, "DELETE", "");
                                    check = responseChecker.CheckResponse(response);
                                    if (responseChecker.error) return new JObject[] { check };
                                }
                            }
                        }



                        foreach (KeyValuePair<string, bool> hook in newHooks)
                        {
                            // create new hook request
                            JObject wuHook = new JObject(){ {"uri", hook.Key } };
                            if (hook.Value) wuHook["isPrimary"] = true;
                            response = _oaGate.SendRequest($"/webhooks", "POST", wuHook.ToString());
                            check = responseChecker.CheckResponse(response);
                            if (responseChecker.error) return new JObject[] { check };
                        }

                        */
            JObject response2 = new JObject() { { "oa", "test" } };// _oaGate.SendRequest($"/webhooks", "GET", "");
            //log.Debug($"WebhooksSetup result={response2.ToString()}");

            
            return new JObject[] { response2 };
        }
    }
}
