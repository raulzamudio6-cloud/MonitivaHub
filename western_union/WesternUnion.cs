using System;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.IO;
using NLog;
using System.Collections.Generic;
using eu.advapay.core.hub.rr_log;

namespace eu.advapay.core.hub.western_union
{
    public class WesternUnion
    {
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();
        private static readonly ILogger archive = LogManager.GetLogger("archive");
        public readonly WesternUnionGate _westernUnionGate;
        public readonly string _url;
        public readonly string _certPath;
        public readonly string _password;
        public readonly string _customerID;
        public readonly string _errorPath;
        public readonly Dictionary<string, string> _errorDict;
        //private readonly JObject errorslist;

        public WesternUnion(WesternUnionGate westernUnionGate, string customerID, string errorPath)
        {
            _customerID = customerID;
            _westernUnionGate = westernUnionGate;
            _errorPath = errorPath;
            _errorDict = new Dictionary<string, string>();
            JArray jArr = new JArray();
            if (File.Exists(_errorPath))
                //                errorslist = JObject.Parse(File.ReadAllText(_errorPath));
                jArr = JArray.Parse(File.ReadAllText(_errorPath));
           // _errorDict = JArray.Parse(File.ReadAllText(_errorPath)).ToObject<Dictionary<string, string>>();
            
            else
                throw new FileNotFoundException($"No file with errors data can be found: {_errorPath}");

            //log.Debug($"jArr count ='" + jArr.Count.ToString() + "'");
            
            if (jArr.Count > 0)
            {
                for(int i = 0; i < jArr.Count; i++)   
                    {
                    _errorDict.Add(jArr[i]["ErrorCode"].ToString(), jArr[i]["ErrorDescription"].ToString());
                }
             //   log.Debug($"_errorDict.Count='" + _errorDict.Count.ToString() + "'");
            }

        }

        // int - edrror code
        // string - JSON array with instructions to integration hub
        // Stream - will be written to response attachments in DB   or  passed as body to http response
        public async Task<(int, JArray, Stream)> WUFunctionAsync(JObject taskParams, JObject jdata, string http_request)
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
                    log.Debug($"before MappedDiagnosticsLogicalContext");
                    MappedDiagnosticsLogicalContext.Set("req_type", "http_request");
                    log.Debug($"after MappedDiagnosticsLogicalContext");
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
                    msg1.Properties.Add("sys", "wu");
                    msg1.Properties.Add("resp", http_request);
                    archive.Info(msg1);

                    log.Debug("ReqRespLog.Save");

                    ReqRespLog.Save("wu", false, http_request);


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
                else if ("rateQuote".Equals(action))
                {
                    JObject quote = RateQuote(jdata);

                    if(quote != null)
                    {
                        quote["module"] = "rate";
                        result.Add(quote);
                    }

                    return (0, result, quote != null ? new MemoryStream(Encoding.UTF8.GetBytes(quote.ToString())) : null);
                }
                else if (action.Equals("ping"))
                {
                    foreach (JObject res in PingWU(jdata))
                        result.Add(res);

                    return (0, result, new MemoryStream(Encoding.UTF8.GetBytes(result.ToString())));
                }
                else if (action.Equals("sendToBank"))
                {
                    //foreach (JObject res in CheckIBAN(jdata))
                      //  result.Add(res);

                    //return (0, result, new MemoryStream(Encoding.Unicode.GetBytes("")));
                    foreach (JObject res in SendToBank(jdata))
                        result.Add(res);

                    return (0, result, new MemoryStream(Encoding.UTF8.GetBytes("")));
                }
                else if (action.Equals("getStatus"))
                {
                    foreach (JObject res in GetStatus(jdata))
                        result.Add(res);

                    return (0, result, new MemoryStream(Encoding.UTF8.GetBytes("")));
                }
                else if (action.Equals("getOrderStatus"))
                {
                    foreach (JObject res in GetOrderStatus(jdata))
                        result.Add(res);

                    return (0, result, new MemoryStream(Encoding.UTF8.GetBytes("")));
                }
                else if (action.Equals("getBalance"))
                {
                    foreach (JObject res in GetBalanace(jdata))
                        result.Add(res);

                    return (0, result, new MemoryStream(Encoding.UTF8.GetBytes("")));
                }
                else if (action.Equals("orderAccount"))
                {
                    foreach (JObject res in OrderAccount(jdata))
                        result.Add(res);

                    return (0, result, new MemoryStream(Encoding.UTF8.GetBytes("")));
                }
                else if (action.Equals("getQuote"))
                {
                    foreach (JObject res in GetQuote(jdata))
                        result.Add(res);

                    return (0, result, new MemoryStream(Encoding.UTF8.GetBytes(result[0].ToString())));
                }
                else if (action.Equals("orderByQuote"))
                {
                    foreach (JObject res in OrderByQuote(jdata))
                        result.Add(res);

                    return (0, result, new MemoryStream(Encoding.UTF8.GetBytes("")));
                }
                else if (action.Equals("cancelPayment"))
                {
                    foreach (JObject res in CancelPayment(jdata))
                        result.Add(res);

                    return (0, result, new MemoryStream(Encoding.Unicode.GetBytes("")));
                }
                else if (action.Equals("setupWebhooks"))
                {
                    foreach (JObject res in SetupWebhooks(jdata))
                        result.Add(res);

                    return (0, result, new MemoryStream(Encoding.Unicode.GetBytes("")));
                }
                else if (action.Equals("checkIBAN"))
                {
                    foreach (JObject res in CheckIBAN(jdata))
                        result.Add(res);

                    log.Debug($"webhook checkIBAN " + result.ToString());
                    return (0, result, new MemoryStream(Encoding.Unicode.GetBytes("")));
                }
                else if (action.Equals("checkBIC"))
                {
                    foreach (JObject res in CheckBIC(jdata))
                        result.Add(res);
                    log.Debug($"checkBIC Result1 = " + Encoding.UTF8.GetBytes(result[0].ToString()));
                    log.Debug($"checkBIC Result2 = " + result);
                    //                    return (0, result, new MemoryStream(Encoding.UTF8.GetBytes(result[0].ToString())));

                    return (0, result, new MemoryStream(Encoding.Unicode.GetBytes("")));
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

                ResponseChecker responseChecker = new ResponseChecker(jdata, _errorDict);
                responseChecker.ParseErrorText(errorCode);


                JObject res = new JObject()
                        {
                            {"module","integrations"},
                            {"integration","western-union"},
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

            ResponseChecker responseChecker = new ResponseChecker(jdata, _errorDict);
            responseChecker.ParseErrorText(errorCode);


            if (!hook.Equals(""))
            {
                return new JObject[] {
                    new JObject()
                    {
                        {"module","integrations"},
                        {"integration","western-union"},
                        {"hook",hook},
                        {"error",responseChecker.errorText},
                        {"externalReference",externalReference}
                    }
                };
            }

            return new JObject[] { };
        }

        public JObject RateQuote(JObject jdata)
        {
            JObject[] quoteResult = GetQuote(jdata);

            JObject res = null;
            if (quoteResult.Length > 0) {
                res = quoteResult[0];
                res["original-request"] = jdata;
            }
            return res;

        }

        public JObject[] PingWU(JObject payment)
        {
            JObject result = new JObject();

            DateTime reqStart = DateTime.Now;
            JObject response = _westernUnionGate.SendRequest("/ping", "GET","");
            DateTime respRead = DateTime.Now;

            result.Add("hook", "ping");
            result.Add("time", (respRead.Subtract(reqStart).TotalMilliseconds) + "ms");
            result.Add("response", response.ToString());
            return new JObject[] { result };
        }

        public JObject[] SendToBank(JObject payment)
        {
            string needUpdateBank = Utils.jsonValue(payment, "needUpdateBank", false);
            string bankBranchCode = Utils.jsonValue(payment, "BenefBankBranchCode", false);
            string isIBAN = Utils.jsonValue(payment, "isIBAN", false);
            log.Debug($"bankBranchCode = " + payment["BenefBankBranchCode"]+",isIBAN="+isIBAN);
            //            if (!needUpdateBank.Equals("1"))
            if (bankBranchCode.Equals(""))
            {
                if (isIBAN.Equals("1"))
                {
                    JObject[] checkIbanResults = CheckIBAN(payment);
                    JObject res = checkIbanResults.Length > 0 ? checkIbanResults[0] : new JObject();
                    log.Debug($"checkIbanResults = " + res.ToString());

                    if (res.ContainsKey("hook") && "checkiban".Equals(res["hook"].ToString()))
                    {
                        log.Debug($"Res bankBranchCode2 = " + res["bankBranchCode"]);
                        payment.Remove("BenefBankState");
                        payment.Remove("BenefBankZip");
                        payment.Remove("BenefBankAddress");
                        payment.Remove("BenefBankCity");
                        payment.Remove("BenefBankName");
                        payment.Remove("BenefBankBranchCode");

                        payment.Add("BenefBankBranchCode", res["bankBranchCode"]);
                        payment.Add("BenefBankState", res["countryProvinceState"]);
                        payment.Add("BenefBankZip", res["zipCode"]);
                        payment.Add("BenefBankAddress", res["streetAddress1"]);
                        payment.Add("BenefBankCity", res["city"]);
                        payment.Add("BenefBankName", res["bankName"]);
                    }
                }
                else
                {
                    JObject[] checkIbanResults = CheckBIC(payment);
                    JObject res = checkIbanResults.Length > 0 ? checkIbanResults[0] : new JObject();
                    log.Debug($"checkBICResults = " + res.ToString());

                    if (res.ContainsKey("hook") && "checkiban".Equals(res["hook"].ToString()))
                    {
                        log.Debug($"Res bankBranchCode2 = " + res["bankBranchCode"]);
                        payment.Remove("BenefBankState");
                        payment.Remove("BenefBankZip");
                        payment.Remove("BenefBankAddress");
                        payment.Remove("BenefBankCity");
                        payment.Remove("BenefBankName");
                        payment.Remove("BenefBankBranchCode");

                        payment.Add("BenefBankBranchCode", res["bankBranchCode"]);
                        payment.Add("BenefBankState", res["countryProvinceState"]);
                        payment.Add("BenefBankZip", res["zipCode"]);
                        payment.Add("BenefBankAddress", res["streetAddress1"]);
                        payment.Add("BenefBankCity", res["city"]);
                        payment.Add("BenefBankName", res["bankName"]);
                    }
                }
            }

            JObject result = new JObject();
            result.Add("integration", "western-union");

            MappedDiagnosticsLogicalContext.Set("req_type", "SendToBank");
            log.Debug($"SendToBank id = " + Utils.jsonValue(payment,"id",true));

            // request
            PaymentRequest request = new PaymentRequest(payment, _customerID);

            JObject response = _westernUnionGate.SendRequest("/payments", "PUT", request.GetWUjson());
            ResponseChecker responseChecker = new ResponseChecker(payment, _errorDict);
            JObject check = responseChecker.CheckResponse(response);
            if (responseChecker.error) return new JObject[] { check };

            // Parse Response
            int receivedPayments = 0;
            if (response.SelectToken("response.receivedPayments") != null)
                receivedPayments = (int)response["response"]["receivedPayments"];

            if (receivedPayments == 1)
            {
                result.Add("module", "integrations");
                result.Add("hook","sentToBank");
                result.Add("document", Utils.jsonValue(payment, "id", false));
                result.Add("externalReference", Utils.jsonValue(payment, "id", false)); // todo replace with real reference
            }
            return new JObject[] { result };
        }


        public JObject[] GetStatus(JObject payment)
        {

            JObject result = new JObject();
            result.Add("module", "integrations");
            result.Add("integration", "western-union");

            if ("".Equals(Utils.jsonValue(payment, "externalReference", true)))
            {
                result["hook"] = "apiError";
                result["error"] = "externalReference not provided or empty";
                return new JObject[] { result };
            };

            // Check input, Log
            MappedDiagnosticsLogicalContext.Set("req_type", "GetStatus");
            if (payment.SelectToken("id") == null)
            {
                result["hook"] = "apiError";
                result["error"] = "payment id not provided";
                return new JObject[] { result };
            }
            

            int paymentId = (int)payment["id"];
            result.Add("document", paymentId);

            string externalReference = (string)payment["externalReference"];
            result.Add("externalReference", externalReference);

            log.Debug($"GetStatus id = {paymentId}");


            // request
            JObject response = _westernUnionGate.SendRequest($"/payments/{externalReference}", "GET","");
            ResponseChecker responseChecker = new ResponseChecker(payment, _errorDict);
            JObject check = responseChecker.CheckResponse(response);
            if (responseChecker.error) return new JObject[] { check };


            // Parse Response
            string status = "";
            if (response.SelectToken("response.status") != null)
                status = (string)response["response"]["status"];

            string hook = ConvertPaymentStatus(status);

            if (!hook.Equals(""))
            {
                if (!"".Equals("" + responseChecker.errorText))
                    result["error"] = responseChecker.errorText;
            }

            if ("".Equals(hook)  && !"".Equals(responseChecker.errorText))
            {
                hook = "apiError";
                result["error"] = responseChecker.errorText;
            }

            if ("".Equals(hook) )
                hook = "updateLog";

            result["hook"] = hook;
            result["message"] = "Provider status: " + status;

            return new JObject[] { result };
        }

        private string ConvertPaymentStatus(string wuStatus)
        {
            string result = "";
            switch (wuStatus)
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

        public JObject[] CancelPayment(JObject payment)
        {
            MappedDiagnosticsLogicalContext.Set("req_type", "CancelPayment");
            Utils.jsonValue(payment, "id", true);
            log.Debug($"CancelPayment id = {payment["id"]}");

            CancelPayment cancelPayment = new CancelPayment(_url, _certPath, _password);
            JObject response = cancelPayment.Cancel(payment["id"].ToString());
            response.Add("document", payment["id"]);
            response.Add("method", "CancelPayment");
            response.Remove("response");
            return new JObject[] { response };
        }

        public JObject[] CheckIBAN(JObject checkIban)
        {
            JObject result = new JObject();
            result.Add("module", "integrations");
            result.Add("method", "CheckIBAN");

            MappedDiagnosticsLogicalContext.Set("req_type", "CheckIBAN");
            //Utils.jsonValue(check, "iBanValue", true);
            log.Debug($"CheckIBAN iBanValue = {checkIban["iBanValue"]}");
            //log.Debug($"CheckIBAN countryCode = {checkIban["countryCode"]}");

            // request
            CreateIBANRequest ibanRequest = new CreateIBANRequest(checkIban);
            JObject response = _westernUnionGate.SendRequest($"/banks/searchbyiban", "POST", ibanRequest.GetWUjson());
            ResponseChecker responseChecker = new ResponseChecker(checkIban, _errorDict);
            JObject check = responseChecker.CheckResponse(response);
            log.Debug($"responseChecker.error = "+ responseChecker.error.ToString());
            if (responseChecker.error) return new JObject[] { check };

            // Parse Response
            if (response.SelectToken("response.responseStatus") != null)
            {
                result.Add("responseStatus", response["response"]["responseStatus"]);
                log.Debug($"checkIBAN responseStatus = " + response.SelectToken("response.responseStatus"));
                result.Add("bankBranchCode", response["response"]["banks"][0]["BankDirectory"]["bankBranchCode"]);
                result.Add("bankName", response["response"]["banks"][0]["BankDirectory"]["bankName"]);
                result.Add("countryProvinceState", response["response"]["banks"][0]["BankDirectory"]["countryProvinceState"]);
                result.Add("zipCode", response["response"]["banks"][0]["BankDirectory"]["zipCode"]);
                result.Add("city", response["response"]["banks"][0]["BankDirectory"]["city"]);
                result.Add("streetAddress1", response["response"]["banks"][0]["BankDirectory"]["streetAddress1"]);
                //log.Debug($"bankBranchCode = " + response["response"]["banks"][0]["BankDirectory"]["bankBranchCode"]);
            }
            string status = response.SelectToken("response.responseStatus") != null ?
                (string)response["response"]["responseStatus"] : "";

            result.Add("hook", "checkiban");
            result.Add("document", checkIban["id"]);

            log.Debug($"checkIBAN finish " + result.ToString());
            return new JObject[] { result };
        }

        public JObject[] CheckBIC(JObject checkBic)
        {
            JObject result = new JObject();
            result.Add("module", "integrations");
            result.Add("method", "CheckBIC");

            MappedDiagnosticsLogicalContext.Set("req_type", "CheckBIC");
            //Utils.jsonValue(check, "iBanValue", true);
            log.Debug($"CheckBIC bankCode = {checkBic["bankCode"]}, countryCode = { checkBic["countryCode"]}");
            //log.Debug($"CheckBIC countryCode = {checkBic["countryCode"]}");

            // request
            CreateBICRequest bicRequest = new CreateBICRequest(checkBic);
            JObject response = _westernUnionGate.SendRequest($"/banks/searchbydetail", "POST", bicRequest.GetWUjson());
            ResponseChecker responseChecker = new ResponseChecker(checkBic, _errorDict);
            JObject check = responseChecker.CheckResponse(response);
            if (responseChecker.error) return new JObject[] { check };

             // Parse Response
            if (response.SelectToken("response.responseStatus") != null)
            {
                result.Add("responseStatus", response["response"]["responseStatus"]);
                log.Debug($"checkBIC responseStatus = " + response.SelectToken("response.responseStatus"));
                result.Add("bankBranchCode", response["response"]["banks"][0]["BankDirectory"]["bankBranchCode"]);
                result.Add("bankName", response["response"]["banks"][0]["BankDirectory"]["bankName"]);
                result.Add("countryProvinceState", response["response"]["banks"][0]["BankDirectory"]["countryProvinceState"]);
                result.Add("zipCode", response["response"]["banks"][0]["BankDirectory"]["zipCode"]);
                result.Add("city", response["response"]["banks"][0]["BankDirectory"]["city"]);
                result.Add("streetAddress1", response["response"]["banks"][0]["BankDirectory"]["streetAddress1"]);
                log.Debug($"CheckBIC bankBranchCode = " + response["response"]["banks"][0]["BankDirectory"]["bankBranchCode"]);
            }
            string status = response.SelectToken("response.responseStatus") != null ?
                (string)response["response"]["responseStatus"] : "";

            result.Add("hook", "checkiban");
            result.Add("document", checkBic["id"]);

            return new JObject[] { result };
        }
        public JObject[] GetBalanace(JObject account)
        {
            JObject result = new JObject();

            MappedDiagnosticsLogicalContext.Set("req_type", "GetBalanace");
            Utils.jsonValue(account, "id", true);
            log.Debug($"GetBalanace id = {account["id"]}");
            string currencyCode = Utils.jsonValue(account, "Currency",true);

            // request
            JObject response = _westernUnionGate.SendRequest($"/holdingBalance/{_customerID}/{currencyCode}", "GET","");

            ResponseChecker responseChecker = new ResponseChecker(account, _errorDict);
            JObject check = responseChecker.CheckResponse(response);
            if (responseChecker.error) return new JObject[] { check };


            // Parse Response


            response.Add("document", account["id"]);

            response.Add("hook", "getBalanace");

            if (response.SelectToken("response.holdingBalances[0].currency") != null)
                result.Add("currency", response["response"]["holdingBalances"][0]["currency"]);
            if (response.SelectToken("response.holdingBalances[0].available") != null)
                result.Add("available", GetAmount(int.Parse(response["response"]["holdingBalances"][0]["available"].ToString()), currencyCode));
            if (response.SelectToken("response.holdingBalances[0].booked") != null)
                result.Add("booked",    GetAmount(int.Parse(response["response"]["holdingBalances"][0]["booked"].ToString()), currencyCode));


            return new JObject[] { result };
        }

        public JObject[] OrderAccount(JObject payment)
        {
            Utils.jsonValue(payment, "id", true);
            log.Debug($"OrderAccount id = {payment["id"]}");

            MappedDiagnosticsLogicalContext.Set("req_type", "SingleQuote");
            SingleQuoteRequest quoteRequest = new SingleQuoteRequest(payment, _customerID);
            SingleQuote quote = new SingleQuote(_url, _certPath, _password);
            JObject response = quote.CreateQuote(quoteRequest.GetWUjson());

            response.Remove("response");
            if (response["hook"].ToString() == "createQuote")
            {
                string quoteId = response["quoteId"].ToString();
                payment.Add("quoteId", quoteId);

                MappedDiagnosticsLogicalContext.Set("req_type", "CreateOrder");
                CreateOrderRequest orderRequest = new CreateOrderRequest(payment, _customerID);
                CreateOrder createOrder = new CreateOrder(_url, _certPath, _password);
                response = createOrder.Send(orderRequest.GetWUjson());
                response.Add("quoteId", quoteId);
                response.Add("document", payment["id"]);
                response.Add("externalReference", response["orderId"]);
                response.Remove("response");
            }
            response.Add("method", "OrderAccount");
            return new JObject[] { response };
        }

        public JObject[] GetQuote(JObject jdata)
        {
            JObject result = new JObject
            {
                { "integration", "western-union" }
            };

            MappedDiagnosticsLogicalContext.Set("req_type", "GetQuote");
            string id = Utils.jsonValue(jdata, "id", false);
            log.Debug($"GetQuote id = {id}");


            // request
            SingleQuoteRequest quoteRequest = new SingleQuoteRequest(jdata, _customerID);
            JObject response = _westernUnionGate.SendRequest($"/quotes", "POST", quoteRequest.GetWUjson());
            ResponseChecker responseChecker = new ResponseChecker(jdata, _errorDict);
            JObject check = responseChecker.CheckResponse(response);
            if (responseChecker.error) return new JObject[] { check };


            // Parse Response
            log.Debug($"quotes='" + response.ToString() + "'");

            if (response.SelectToken("response.id") != null)
            {
                result.Add("quoteId", response["response"]["id"]);
                result.Add("expirationIntervalInSec", response["response"]["expirationIntervalInSec"]);
                result.Add("createdOn", response["response"]["createdOn"]);
                result.Add("buyAmount", response["response"]["quotedItems"][0]["tradeAmount"]);
                result.Add("sellAmount", response["response"]["quotedItems"][0]["settlementAmount"]);
                result.Add("sellCurrency", response["response"]["quotedItems"][0]["settlementCurrency"]);
                result.Add("buyCurrency", response["response"]["quotedItems"][0]["tradeCurrency"]);
                // wu rate is settlment/trade, we want rate = buy/sell
                bool isDirect = !(bool)response["response"]["quotedItems"][0]["isDirectRate"];
                result.Add("isDirectRate", isDirect);
                result.Add("rate", response["response"]["quotedItems"][0]["rate"]);
                result.Add("rateInverted", response["response"]["quotedItems"][0]["rateInverted"]);
            }


            try
            {
                result["buyAmount"] = GetAmount((int)result["buyAmount"], (string)result["buyCurrency"]);
                result["sellAmount"] = GetAmount((int)result["sellAmount"], (string)result["sellCurrency"]);
            }
            catch (Exception e){
                log.Error(e, "Can't convert WU amounts: "+ (string)result["buyAmount"] +"/"+ (string)result["sellAmount"]);
            }

            return new JObject[] { result };
        }

        public JObject[] OrderByQuote(JObject quote)
        {
            log.Debug($"OrderByQuote quote = " + quote.ToString());
            JObject result = new JObject();
            result.Add("module", "integrations");

            MappedDiagnosticsLogicalContext.Set("req_type", "OrderByQuote");
            Utils.jsonValue(quote, "id", true);
            log.Debug($"OrderByQuote id = {quote["id"]}");

            // request
            CreateOrderRequest orderRequest = new CreateOrderRequest(quote, _customerID);
            JObject response = _westernUnionGate.SendRequest($"/orders", "POST", orderRequest.GetWUjson());
            log.Debug($"OrderByQuote SendRequest");
            ResponseChecker responseChecker = new ResponseChecker(quote, _errorDict);
            JObject check = responseChecker.CheckResponse(response);
 //           log.Debug($"Exchange check='" + check.ToString() + "'");
            if (responseChecker.error) return new JObject[] { check };


            // Parse Response
            if (response.SelectToken("response.orders[0].id") != null)
                result.Add("externalReference", response["response"]["orders"][0]["id"]);

            string status = response.SelectToken("response.orders[0].status") != null ?
                (string)response["response"]["orders"][0]["status"] : "";

            result.Add("hook", ConvertOrderStatus(status));
            log.Debug($"Exchange status='" + status + "'");


            result.Add("document", quote["id"]);
            result.Add("status", status);

            log.Debug($"OrderByQuote Return");
            return new JObject[] { result };
        }

        public JObject[] GetOrderStatus(JObject order)
        {
            JObject result = new JObject
            {
                { "module", "integrations" },
                { "integration", "western-union" }
            };

            MappedDiagnosticsLogicalContext.Set("req_type", "GetOrderStatus");
            Utils.jsonValue(order, "id", true);
            log.Debug($"GetOrderStatus id = {order["id"]}");

            Utils.jsonValue(order, "externalReference", true);
            string externalReference = (string)order["externalReference"];
            result.Add("externalReference", externalReference);

            // request
            JObject response = _westernUnionGate.SendRequest($"/orders/{externalReference}", "GET", "");
            ResponseChecker responseChecker = new ResponseChecker(order, _errorDict);
            JObject check = responseChecker.CheckResponse(response);
            if (responseChecker.error) return new JObject[] { check };


            // Parse Response
            string status = response.SelectToken("response.status") != null ?
                (string)response["response"]["status"] : "";

            result.Add("hook", ConvertOrderStatus(status));

            return new JObject[] { result };
        }

        private string ConvertOrderStatus(string wuStatus)
        {
            string result = "";
            switch (wuStatus)
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
            JObject response = _westernUnionGate.SendRequest($"/webhooks", "GET", "");
            ResponseChecker responseChecker = new ResponseChecker(webhooks, _errorDict);
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
                        response = _westernUnionGate.SendRequest($"/webhooks/"+ id, "DELETE", "");
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
                response = _westernUnionGate.SendRequest($"/webhooks", "POST", wuHook.ToString());
                check = responseChecker.CheckResponse(response);
                if (responseChecker.error) return new JObject[] { check };
            }


            JObject response2 = _westernUnionGate.SendRequest($"/webhooks", "GET", "");
            log.Debug($"WebhooksSetup result={response2.ToString()}");


            return new JObject[] { response2 };
        }
    }
}
