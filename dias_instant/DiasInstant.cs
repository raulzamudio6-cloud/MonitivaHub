using System;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.IO;
using NLog;
using System.Collections.Generic;
using eu.advapay.core.hub.rr_log;
using System.Collections.Specialized;
using StackExchange.Redis;
using System.Globalization;

namespace eu.advapay.core.hub.dias_instant
{
    public class DiasInstant
    {
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();
        private static readonly ILogger archive = LogManager.GetLogger("archive");
        private readonly DiasInstantGate _diasInstantGate;
        private String _diasEnv = "T";


        public DiasInstant(string url, string diasEnv)
        {
            if(diasEnv != null) _diasEnv = diasEnv;
            _diasInstantGate = new DiasInstantGate(url);
        }

        // int - edrror code
        // string - JSON array with instructions to integration hub
        // Stream - will be written to response attachments in DB   or  passed as body to http response
        public async Task<(int, JArray, Stream)> CLFunctionAsync(JObject taskParams, string body)
        {

            JArray result = new JArray();
            int ii;


            MappedDiagnosticsLogicalContext.Set("req_id", null);
            MappedDiagnosticsLogicalContext.Set("req_type", null);
            if (taskParams != null && taskParams.ContainsKey("id") && int.TryParse(taskParams["id"].ToString(), out ii)) 
            {
                if (ii > 0) { MappedDiagnosticsLogicalContext.Set("doc", ii); }
                else { MappedDiagnosticsLogicalContext.Set("doc", null); }
            }

            string action = "";

            try
            {

                if (taskParams["action"] != null)
                    action = taskParams["action"].ToString();

                if (action.Equals("sendToBank"))
                {
                    foreach (JObject res in SendToBank(taskParams,body))
                        result.Add(res);

                    return (0, result, new MemoryStream(Encoding.UTF8.GetBytes("")));
                }
                else
                {
                    result.Add(new JObject()
                    {
                        {"module","integrations"},
                        {"hook","apiError"},
                        {"error", "unknown action: " + action},
                        {"document",taskParams?["id"]}
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
                    {"document",taskParams?["id"]}
                });
                return (2, result, ms);
            }


        }
        public JObject[] SendToBank(JObject payment, string body)
        {
            List<JObject> results = new List<JObject>();
            JObject result = new JObject();
            result.Add("integration", "dias-instant");

            MappedDiagnosticsLogicalContext.Set("req_type", "SendToBank");
            log.Debug($"SendToBank id = " + Utils.jsonValue(payment, "id", true));


            string doc_id = Utils.jsonValue(payment, "id", true);


            // request
            JObject req = new JObject();
            req["MsgDtTm"] = Utils.jsonValue(payment, "MsgDtTm", true);
            req["MsgID"] = Utils.jsonValue(payment, "MsgID", true);
            req["Env"] = _diasEnv;
            req["SndgInst"] = Utils.jsonValue(payment, "SndgInst", true);
            req["PayloadType"] = Utils.jsonValue(payment, "PayloadType", true);
            req["Payload"] = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(body));

            JObject req0 = new JObject();
            req0["PutMessageRequest"] = req;

            JObject resp = _diasInstantGate.SendRequest("", "POST", req0.ToString());

            if ((int)resp["re"] == 0)
            {
                string msgSts = resp.SelectToken("response.PutMessageResponse.MsgSts")?.ToString();
                if ("RJCT".Equals(msgSts))
                {
                    result.Add("module", "integrations");
                    result.Add("hook", "apiError");
                    result.Add("document", doc_id);
                    result.Add("error", resp.SelectToken("response.PutMessageResponse")?.ToString()); 
                }
                else if ("ACCP".Equals(msgSts))
                {
                    string payLoad = resp.SelectToken("response.PutMessageResponse.Payload")?.ToString();
                    if (payLoad != null)
                    {
                        payLoad = Encoding.UTF8.GetString(System.Convert.FromBase64String(payLoad));
                        resp["response"]["PutMessageResponse"]["Payload"] = payLoad;
                    }

                    result.Add("module", "integrations");
                    result.Add("hook", "parseDiasInstant");
                    result.Add("document", doc_id);
                    result.Add("result", resp.SelectToken("response")?.ToString()); 
                }
            }
            else
            {
                result.Add("module", "integrations");
                result.Add("hook", "apiError");
                result.Add("document", doc_id);
                result.Add("error", resp.ToString());

            }


            results.Add(result);

            return results.ToArray();
        }


    }
}
