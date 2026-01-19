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

namespace eu.advapay.core.hub.currency_cloud
{
    public class CurrencyCloud
    {
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();
        private static readonly ILogger archive = LogManager.GetLogger("archive");
        private readonly CurrencyCloudGate _currencyCloudGate;
        private readonly IDatabase _redisDb;
        public bool dontCreateUpdateAccount;
        public bool hideContacts;
        public bool makeTransfersToHouseAccount;
        public bool deductTransferFeeToHouseAccount;

        private Dictionary<string, string> houseAccId = new Dictionary<string, string>();

        public CurrencyCloud(string url, string loginId, string apiKey, string redisHost, string redisPost)
        {
            _currencyCloudGate = new CurrencyCloudGate(url, loginId, apiKey);
            log.Info("Connecting to Redis at "+redisHost+":"+redisPost);
            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(redisHost + ":" + redisPost);
            _redisDb = redis.GetDatabase();
            log.Info("Redis connected");
        }

        // int - edrror code
        // string - JSON array with instructions to integration hub
        // Stream - will be written to response attachments in DB   or  passed as body to http response
        public async Task<(int, JArray, Stream)> CLFunctionAsync(JObject taskParams, JObject jdata, string http_request)
        {

            JArray result = new JArray();
            int ii;


            MappedDiagnosticsLogicalContext.Set("req_id", null);
            MappedDiagnosticsLogicalContext.Set("req_type", null);
            if (jdata != null && jdata.ContainsKey("id") && int.TryParse(jdata["id"].ToString(), out ii)) 
            {
                if (ii > 0) { MappedDiagnosticsLogicalContext.Set("doc", ii); }
                else { MappedDiagnosticsLogicalContext.Set("doc", null); }
            }

            string action = "";

            try
            {

                if (taskParams["action"] != null)
                    action = taskParams["action"].ToString();

                if (action.Equals("http_request"))
                {
                    MappedDiagnosticsLogicalContext.Set("req_type", "http_request");
                    log.Trace("http_request: " + http_request);

                    if (jdata == null)
                    {
                        result.Add(new JObject()
                        {
                            {"module","http_response"},
                            {"HttpCode","400"},
                            {"X-Mab-Error","request is not JSON"},
                            {"Content-Type","text/plain; charset= utf-8"}
                        });
                        return (4, result, new MemoryStream(Encoding.UTF8.GetBytes("request is not JSON")));
                    }

                    var msg1 = new LogEventInfo(LogLevel.Info, "", "log");
                    msg1.Properties.Add("sys", "cc");
                    msg1.Properties.Add("resp", http_request);
                    archive.Info(msg1);

                    ReqRespLog.Save("cc", false, http_request);



                    string msgType  =   (string)jdata.SelectToken("header.message_type");



                    if ("payment".Equals(msgType))
                    {
                        foreach (JObject res in WebHookPaymentStatusChanged(jdata))
                            result.Add(res);

                        return (0, result, null);
                    }
                    else if ("cash_manager_transaction".Equals(msgType))
                    {
                        foreach (JObject res in IncomingFundsProcess(jdata))
                            result.Add(res);

                        return (0, result, null);
                    }
                    else if ("conversion".Equals(msgType))
                    {
                        foreach (JObject res in WebHookOrderStatusChanged(jdata))
                            result.Add(res);

                        return (0, result, null);
                    }
                    else if ("transfer".Equals(msgType))
                    {
                        foreach (JObject res in TransferStatusChanged(jdata))
                            result.Add(res);

                        return (0, result, null);
                    }

                    // Stream here is what returned as http response body
                    return (4, result, new MemoryStream(Encoding.Unicode.GetBytes(http_request)));

                }
                else if ("cacheAllRates".Equals(action))
                {
                    foreach (JObject res in CacheAllRates(jdata))
                        result.Add(res);

                    return (0, result, null);
                }
                else if ("rateQuote".Equals(action))
                {
			        log.Debug($"RateQuote start: " + jdata.ToString());
                    JObject quote = RateQuote(jdata);

			        log.Debug($"RateQuote result: " + quote.ToString());

                    if(quote != null)
                    {
                        quote["module"] = "rate";
                        result.Add(quote);
                    }

                    return (0, result, quote != null ? new MemoryStream(Encoding.UTF8.GetBytes(quote.ToString())) : null);
                }
                else if (action.Equals("ping"))
                {
                    result.Add(_currencyCloudGate.SendRequest("/v2/accounts/current", "GET", null));

                    return (0, result, new MemoryStream(Encoding.UTF8.GetBytes(result.ToString())));
                }
                else if (action.Equals("sendToBank"))
                {
                    foreach (JObject res in SendToBank(jdata))
                        result.Add(res);

                    return (0, result, new MemoryStream(Encoding.UTF8.GetBytes("")));
                }
                else if (action.Equals("createUpdateAccount"))
                {
                    foreach (JObject res in CreateUpdateAccount(jdata))
                        result.Add(res);

                    return (0, result, new MemoryStream(Encoding.UTF8.GetBytes("")));
                }
                else if (action.Equals("accountIBANs"))
                {
                    foreach (JObject res in GetAccountIBANs(jdata))
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
                else if (action.Equals("subAccountTransfer"))
                {
                    foreach (JObject res in TransferFromSubAcc(jdata))
                        result.Add(res);

                    return (0, result, new MemoryStream(Encoding.UTF8.GetBytes("")));
                }
                
                else if (action.Equals("setupWebhooks"))
                {
                    foreach (JObject res in SetupWebhooks(jdata))
                        result.Add(res);

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

        public JObject[] IncomingFundsProcess(JObject jdata)
        {
            List<JObject> results = new List<JObject>();

            string error = "";

            string transId = (string)jdata.SelectToken("body.id");
            string entityId   = (string)jdata.SelectToken("body.related_entity_id");
            string entityType = (string)jdata.SelectToken("body.related_entity_type");
            string entityStatus =   (string)jdata.SelectToken("body.status");
            string accountId = "" + (string)jdata.SelectToken("body.account_id");
            string currency  = "" + (string)jdata.SelectToken("body.currency");
            float  amount    =       (float)jdata.SelectToken("body.amount");
            string createdAt = "" + (string)jdata.SelectToken("body.created_at");
            string settlesAt = "" + (string)jdata.SelectToken("body.settles_at");

            if (!"completed".Equals(entityStatus)) return results.ToArray();
            if (!"inbound_funds".Equals(entityType)) return results.ToArray();

            ResponseChecker responseChecker = new ResponseChecker(jdata);
            NameValueCollection req = new NameValueCollection();

            JObject check;
            string contactID = "";
            req.Add("account_id", accountId);
            JObject findContact = _currencyCloudGate.SendRequest("/v2/contacts/find", "POST", req);
            check = responseChecker.CheckResponse(findContact);
            if (responseChecker.error) { log.Error(responseChecker.errorText); return new JObject[] { check }; }

            contactID = (string)findContact.SelectToken("response.contacts[0].id");

            req.Clear();
            //req.Add("related_entity_id", entityId.ToLower());
            req.Add("on_behalf_of", contactID);
            JObject findTrans = _currencyCloudGate.SendRequest($"/v2/transactions/{transId}", "GET", req);
            check = responseChecker.CheckResponse(findTrans,"Can't find incoming transaction");
            if (responseChecker.error) { log.Error(responseChecker.errorText); return new JObject[] { check }; }

            JObject res = new JObject()
            {
                {"module","integrations"},
                {"integration","currency-cloud"},
                {"hook","incoming_payment"},
                {"currency",currency},
                {"amount",amount},
                {"entityId",entityId},
                {"accountExtRef", accountId},
                {"createdAt",createdAt},
                {"settlesAt",settlesAt},
            };


            JObject tran = (JObject)findTrans.SelectToken("response");
            if(tran!=null)
            { 
            //JArray trans = (JArray)findTrans.SelectToken("response.transactions");
            //if(trans!=null) 
            //{
            //    if (trans.Count == 0)
            //    {
            //        error = "transactions not found for incoming entity with id=" + entityId + " contactID=" + contactID;
            //        log.Error(error);
            //    }
            //    else if (trans.Count > 1)
            //    {
            //        error = "More than one transaction for incoming entity with id=" + entityId + " contactID=" + contactID;
            //        log.Error(error);
            //    }
            //    else
            //    {
            //        tran = (JObject)trans[0];
                    if ( Math.Abs((float)tran["amount"] - amount) < 0.00001 && currency.Equals((string)tran["currency"]) 
                        && "completed".Equals((string)tran["status"]) )
                    {
                        JObject transDetails = _currencyCloudGate.SendRequest("/v2/transactions/sender/"+(string)tran["related_entity_id"], "GET", null);
                        check = responseChecker.CheckResponse(transDetails);
                        if (responseChecker.error) { 
                            log.Error(responseChecker.errorText); 
                        }
                        else
                        {
                            res["externalReference"] = (string)tran["id"];

                            string sender = "" + (string)transDetails.SelectToken("response.sender");
                            string[] senderData = sender.Split(';', 6);
                            if (senderData.Length > 0) res["senderName"] = senderData[0];
                            if (senderData.Length > 1) res["senderAddress"] = senderData[1];
                            if (senderData.Length > 2) res["senderCountry"] = senderData[2];
                            if (senderData.Length > 3) res["senderAccount"] = senderData[3];
                            if (senderData.Length > 4) res["senderBic"] = senderData[4];
                            if (senderData.Length > 5) res["senderRouting"] = senderData[5];

                            res["accountIban"] = (string)transDetails.SelectToken("response.receiving_account_iban");
                            res["accountNumber"] = (string)transDetails.SelectToken("response.receiving_account_number");
                            res["paymentDetails"] = (string)transDetails.SelectToken("response.additional_information");
                        
                        }

                        //string house_acc_id = getHouseAcc(currency);
                        //req.Clear();
                        //req.Add("source_account_id", accountId);
                        //req.Add("destination_account_id", house_acc_id);
                        //req.Add("currency", currency);
                        //req.Add("amount", ""+amount);
                        //req.Add("reason", "balance sub-acc to house");
                        //JObject transferToHouse = _currencyCloudGate.SendRequest("/v2/transfers/create", "POST", req);
                        //responseChecker.CheckResponse(transferToHouse, "Can't find incoming transaction");
                        //if (responseChecker.error) { log.Error(responseChecker.errorText); }
                    }
                    else
                    {
                        error = "Incoming payment details does not match";
                    }

            //    }
            }
            else
            {
                error = "Can't find incoming transaction: element not found";
                log.Error(error);
            }

            if (!"".Equals(""+error))
            {
                res["hook"] = "incoming_payment_error";
                res["error"] = error;
            }

            results.Add(res);

            return results.ToArray();
        }

        public JObject[] TransferFromSubAcc(JObject jdata)
        {
            List<JObject> results = new List<JObject>();

            ResponseChecker responseChecker = new ResponseChecker(jdata);
            NameValueCollection req = new NameValueCollection();

            string doc_id = Utils.jsonValue(jdata, "id", true);
            log.Debug($"TransferFromSubAcc id = " + doc_id);
            log.Debug($"TransferFromSubAcc id2 = " + doc_id);

            string house_acc_id = getHouseAcc(Utils.jsonValue(jdata, "currency", true));
            string details = Utils.jsonValue(jdata, "details", false);
            if("".Equals(""+ details)) details = "balancing incoming payment "+ Utils.jsonValue(jdata, "id", true);

            req.Add("source_account_id", Utils.jsonValue(jdata, "fromExternalReference", true));
            req.Add("destination_account_id", house_acc_id);
            req.Add("currency", Utils.jsonValue(jdata, "currency", true));
            req.Add("amount", Utils.jsonValue(jdata, "amount", true));
            req.Add("reason", details);
            req.Add("unique_request_id", Utils.jsonValue(jdata, "id", true));
            JObject transferToHouse = _currencyCloudGate.SendRequest("/v2/transfers/create", "POST", req);
            JObject check = responseChecker.CheckResponse(transferToHouse, "transfer from sub-account");
            if (responseChecker.error) { log.Error(responseChecker.errorText); return new JObject[] { check }; }

            JObject result = new JObject
            {
            };

            string paymentId;

            if ((int)transferToHouse["re"] == 0)
            {
                paymentId = (string)transferToHouse["response"]["id"];
                log.Trace("Create transfer id:" + paymentId);
                result.Add("module", "integrations");
                result.Add("hook", "sentToBank");
                result.Add("document", doc_id);
                result.Add("externalReference", paymentId); // todo replace with real reference

            }
            else
            {
                result.Add("module", "integrations");
                result.Add("hook", "apiError");
                result.Add("document", doc_id);
                result.Add("error", transferToHouse.ToString()); // todo use ResponseChecker
            }
        

            results.Add(result);

            return results.ToArray();
        }

        public JObject[] WebHookPaymentStatusChanged(JObject jdata)
        {
            List<JObject> results = new List<JObject>();

            {
                string externalReference = (string)jdata.SelectToken("body.id");
                string status = (string)jdata.SelectToken("body.status");
                string error = (string)jdata.SelectToken("body.failure_reason");
                string hook = ConvertPaymentStatus(status);


                JObject res = new JObject()
                        {
                            {"module","integrations"},
                            {"integration","currency-cloud"},
                            {"hook",hook},
                            {"externalReference",externalReference}
                        };

                if (!"".Equals(""+error)) res["error"] = error;

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
            List<JObject> results = new List<JObject>();

            JObject result = new JObject();

            string externalReference = (string)jdata.SelectToken("body.id");
            string status = (string)jdata.SelectToken("body.status");
            string error = (string)jdata.SelectToken("body.failure_reason");
            string hook = ConvertOrderStatus(status);

            ResponseChecker responseChecker = new ResponseChecker(jdata);
            JObject res = new JObject()
                        {
                            {"module","integrations"},
                            {"integration","currency-cloud"},
                            {"hook",hook},
                            {"externalReference",externalReference}
                        };

            if (!"".Equals("" + error)) res["error"] = error;

            if ("".Equals(hook))
                hook = "updateLog";

            res["hook"] = hook;
            res["message"] = "Provider status: " + status;

            results.Add(res);

            return results.ToArray();
        }

        public JObject[] TransferStatusChanged(JObject jdata)
        {
            List<JObject> results = new List<JObject>();

            JObject result = new JObject();

            string externalReference = (string)jdata.SelectToken("body.id");
            string status = (string)jdata.SelectToken("body.status");
            string error  = (string)jdata.SelectToken("body.failure_reason"); 
            string hook   = ConvertTransferStatus(status);

            ResponseChecker responseChecker = new ResponseChecker(jdata);
            JObject res = new JObject()
                        {
                            {"module","integrations"},
                            {"integration","currency-cloud"},
                            {"hook",hook},
                            {"externalReference",externalReference}
                        };

            if (!"".Equals("" + error)) res["error"] = error;

            if ("".Equals(hook))
                hook = "updateLog";

            res["hook"] = hook;
            res["message"] = "Provider status: " + status;

            results.Add(res);

            return results.ToArray();
        }

        public JObject RateQuote(JObject jdata)
        {
            MappedDiagnosticsLogicalContext.Set("req_type", "RateQuote");

            JObject result = new JObject();
            result["original-request"] = jdata;

            string buyCurrency = Utils.jsonValue(jdata, "buyCurrency", true);
            string sellCurrency = Utils.jsonValue(jdata, "sellCurrency", true);
            
            Utils.jsonValue(jdata, "isSellAmount", true);
            bool isSellAmount = (bool)jdata["isSellAmount"];

            if (!decimal.TryParse(Utils.jsonValue(jdata, "amount", true), out decimal amount))
            {
                amount = decimal.Parse(Utils.jsonValue(jdata, "amount", true), CultureInfo.InvariantCulture);
            }

            string currPair = sellCurrency + buyCurrency;
            string currPairRev = buyCurrency + sellCurrency;

            string rateDataStr = _redisDb.HashGet("currencyCloudRates", currPair);
            string rateDataStrRev = _redisDb.HashGet("currencyCloudRates", currPairRev);

            if (rateDataStr == null && rateDataStrRev == null)
            {
                // no record in cache, request rate
                CacheAllRates(new JObject() { { "currencyPairs", currPair + "," + currPairRev } });

                rateDataStr = _redisDb.HashGet("currencyCloudRates", currPair);
                rateDataStrRev = _redisDb.HashGet("currencyCloudRates", currPairRev);
                
                if (rateDataStr == null && rateDataStrRev == null)
                {

                    result["hook"] = "error";
                    result["error"] = "try later";
                    return result;
                }
            }

            if (rateDataStr == null) rateDataStr = "{}";
            if (rateDataStrRev == null) rateDataStrRev = "{}";
            JObject rateData = JObject.Parse(rateDataStr);
            JObject rateDataRev = JObject.Parse(rateDataStrRev);

            decimal bid = rateData.ContainsKey("bid") ? (decimal)rateData["bid"] : 0;
            decimal ask = rateData.ContainsKey("ask") ? (decimal)rateData["ask"] : 0;
            string date = rateData.ContainsKey("date") ? (string)rateData["date"] : null;
            decimal bidRev = rateDataRev.ContainsKey("bid") ? (decimal)rateDataRev["bid"] : 0;
            decimal askRev = rateDataRev.ContainsKey("ask") ? (decimal)rateDataRev["ask"] : 0;
            string dateRev = rateDataRev.ContainsKey("date") ? (string)rateDataRev["date"] : null;

            if (date == null && dateRev == null)
            {
                // empty record in cache - rate not available in CurrencyCloud
                result["hook"] = "error";
                result["error"] = "not_available";
                return result;
            }


            int roundDigits = 2;

            decimal ALMOST_ZERO = (decimal)0.000000001;
            bool useDirectRate = bid > 0 && (bid > 1 || ask < ALMOST_ZERO || bidRev < ALMOST_ZERO || askRev < ALMOST_ZERO);
            
            decimal rate;

            DateTime rateCreated = DateTime.Parse(useDirectRate ? date : dateRev, DateTimeFormatInfo.InvariantInfo);
            DateTime rateExpires = rateCreated.AddSeconds(30);
            if (rateExpires < DateTime.Now)
            {
                result["hook"] = "error";
                result["error"] = "expired";
                return result;
            }


            if (isSellAmount)
            {
                if ("JPY".Equals(buyCurrency.ToUpper())) roundDigits = 0;
                if (useDirectRate) rate = bid;
                else rate = 1 / bidRev;
                result["sellAmount"] = amount;
                result["buyAmount"] = Math.Round(amount * rate, roundDigits);
            }
            else
            {
                if ("JPY".Equals(sellCurrency.ToUpper())) roundDigits = 0;
                if (useDirectRate) rate = bid;
                else rate = 1 / bidRev;
                result["sellAmount"] = Math.Round(amount / rate, roundDigits);
                result["buyAmount"] = amount;
            }

            result.Add("quoteId", "n/a");
            result.Add("expirationIntervalInSec", 120);
            result.Add("createdOn", rateCreated.ToString("s", DateTimeFormatInfo.InvariantInfo) + "Z");
            result.Add("sellCurrency", sellCurrency);
            result.Add("buyCurrency", buyCurrency);
            result.Add("isDirectRate", true);
            result.Add("rate", rate);
            //result.Add("rateInverted", false);

            return result;

        }

        public JObject[] CreateUpdateAccount(JObject acc)
        {
            MappedDiagnosticsLogicalContext.Set("req_type", "CreateUpdateAccount");

            List<JObject> results = new List<JObject>();
            JObject result = null;

            ResponseChecker responseChecker = new ResponseChecker(acc);

            bool accOk = true;
            bool contactOk = true;

            string acc_id = Utils.jsonValue(acc, "id", true);

            string externalReference = ""+(string)acc["externalReference"];

            if("".Equals(externalReference))
            {
                if (dontCreateUpdateAccount) throw new Exception("Creating accounts is not allowed");
                NameValueCollection req = new NameValueCollection();
                req.Add("account_name", (string)acc["OwnerName"]);
                req.Add("your_reference", (string)acc["Designation"]);
                req.Add("legal_entity_type", (string)acc["OwnerType"]);
                req.Add("street", (string)acc["OwnerStreet"]);
                req.Add("city", (string)acc["OwnerCity"]);
                req.Add("country", (string)acc["OwnerCountry"]);

                JObject accResp = _currencyCloudGate.SendRequest("/v2/accounts/create", "POST", req);
                JObject check = responseChecker.CheckResponse(accResp, "create account");
                if (responseChecker.error) { return new JObject[] { check }; }

                accOk = !responseChecker.error;

                externalReference = (string)accResp["response"]["id"];

                result = new JObject();

                result.Add("module", "integrations");
                result.Add("integration", "currency-cloud");
                result.Add("hook", "createdAccount");
                result.Add("document", acc_id);
                result.Add("externalReference", externalReference);

                results.Add(result);
            }
            else
            {
                NameValueCollection req = new NameValueCollection();
                if ((int)acc["statusChanged"] == 1) req.Add("status", (string)acc["Status"]);
                if ((int)acc["nameChanged"] == 1)   req.Add("account_name", (string)acc["OwnerName"]);

                if ((int)acc["nameChanged"] == 1)   req.Add("legal_entity_type", (string)acc["OwnerType"]);

                if ((int)acc["addressChanged"] == 1) req.Add("street", (string)acc["OwnerStreet"]);
                if ((int)acc["addressChanged"] == 1) req.Add("city", (string)acc["OwnerCity"]);
                if ((int)acc["addressChanged"] == 1) req.Add("country", (string)acc["OwnerCountry"]);

                if (req.Count > 0 && !dontCreateUpdateAccount)
                {
                    JObject accResp = _currencyCloudGate.SendRequest("/v2/accounts/" + externalReference, "POST", req);
                    JObject check = responseChecker.CheckResponse(accResp, "update account");
                    if (responseChecker.error) { return new JObject[] { check }; }

                    result = new JObject();

                    result.Add("module", "integrations");
                    result.Add("integration", "currency-cloud");
                    result.Add("hook", "updatedAccount");
                    result.Add("document", acc_id);
                    result.Add("externalReference", externalReference);

                    results.Add(result);
                }
            }

            if (accOk)
            {
                string contactExternalReference = "" + (string)acc["ownerExternalReference"];

                if ("".Equals(contactExternalReference))
                {
                    string login_id = hideContacts ? $"cc_{acc_id}@example.com" : acc_id + "." + (string)acc["OwnerEmail"];
                    NameValueCollection req = new NameValueCollection();
                    req.Add("account_id", externalReference);
                    req.Add("login_id", login_id);
                    req.Add("first_name", hideContacts ? acc_id : (string)acc["OwnerFirstName"]);
                    req.Add("last_name", hideContacts ? "API" : (string)acc["OwnerLastName"]);
                    req.Add("date_of_birth", hideContacts ? "1970-01-01" : (string)acc["OwnerDateOfBirth"]);

                    req.Add("email_address", hideContacts ? $"cc_{acc_id}@example.com" : (string)acc["OwnerEmail"]);
                    req.Add("phone_number", hideContacts ? "000" : (string)acc["OwnerPhone"]);

                    req.Add("status", "enabled");

                    JObject contactResp = _currencyCloudGate.SendRequest("/v2/contacts/create", "POST", req);
                    JObject contactCheck = responseChecker.CheckResponse(contactResp, "create contact");
                    if (responseChecker.error) { log.Error(responseChecker.errorText); }

                    contactOk = !responseChecker.error;

                    bool returnError = !contactOk;
                    if (contactOk)
                    {
                        contactExternalReference = (string)contactResp.SelectToken("response.id");

                        result = new JObject();
                        result.Add("module", "integrations");
                        result.Add("integration", "currency-cloud");
                        result.Add("hook", "createdContact");
                        result.Add("document", Utils.jsonValue(acc, "ownerId", false));
                        result.Add("accountDocument", acc_id);
                        result.Add("externalReference", contactExternalReference);
                        result.Add("accountExternalReference", externalReference);

                        results.Add(result);
                    }
                    else
                    {
                        req.Clear();
                        req.Add("account_id", externalReference);
                        JObject findContact = _currencyCloudGate.SendRequest("/v2/contacts/find", "POST", req);
                        ResponseChecker responseCheckerFind = new ResponseChecker(acc);
                        responseCheckerFind.CheckResponse(findContact,"find contact");
                        if (responseCheckerFind.error) { log.Error(responseCheckerFind.errorText); }

                        if ((int)findContact["re"] == 0)
                        {
                            string foundContactId = (string)findContact.SelectToken("response.contacts[0].id");
                            if(!"".Equals(""+foundContactId))
                            {
                                contactExternalReference = foundContactId;
                                
                                result = new JObject();

                                result.Add("module", "integrations");
                                result.Add("integration", "currency-cloud");
                                result.Add("hook", "createdContact");
                                result.Add("document", Utils.jsonValue(acc, "ownerId", false));
                                result.Add("accountDocument", acc_id);
                                result.Add("externalReference", contactExternalReference);
                                result.Add("accountExternalReference", externalReference);

                                results.Add(result);
                                returnError = false;
                            }
                        }
                    }

                    if (returnError) return new JObject[] { contactCheck }; 
                    
                }
                else
                {
                    NameValueCollection req = new NameValueCollection();
                    if ((int)acc["nameChanged"] == 1) req.Add("first_name", hideContacts ? acc_id : (string)acc["OwnerFirstName"]);
                    if ((int)acc["nameChanged"] == 1) req.Add("last_name", hideContacts ? "API" : (string)acc["OwnerLastName"]);
                    if ((int)acc["nameChanged"] == 1) req.Add("date_of_birth", hideContacts ? "1970-01-01" : (string)acc["OwnerDateOfBirth"]);

                    if ((int)acc["contactsChanged"] == 1) req.Add("email_address", hideContacts ? $"cc_{acc_id}@example.com" : (string)acc["OwnerEmail"]);
                    if ((int)acc["contactsChanged"] == 1) req.Add("phone_number", hideContacts ? "000" : (string)acc["OwnerPhone"]);

                    req.Add("status", "enabled");

                    if (req.Count > 0)
                    {
                        JObject contactResp = _currencyCloudGate.SendRequest("/v2/contacts/" + contactExternalReference, "POST", req);
                        responseChecker.CheckResponse(contactResp, "update contact");
                        if (responseChecker.error) { log.Error(responseChecker.errorText); }

                        result = new JObject();

                        result.Add("module", "integrations");
                        result.Add("integration", "currency-cloud");
                        result.Add("hook", "updatedContact");
                        result.Add("document", Utils.jsonValue(acc, "ownerId", false));
                        result.Add("accountDocument", acc_id);
                        result.Add("externalReference", contactExternalReference);
                        result.Add("accountExternalReference", externalReference);

                        results.Add(result);
                    }
                }
            }

            return results.ToArray();
        }

        public List<JObject> GetAccountIBANs(JObject acc)
        {
            List<JObject> results = new List<JObject>();
            JObject result = null;

            JArray ibans = new JArray();

            NameValueCollection req = new NameValueCollection();
            req.Add("account_id", Utils.jsonValue(acc, "externalReference", true));
            req.Add("currency", "EUR");

            JObject findAccResp = _currencyCloudGate.SendRequest("/v2/funding_accounts/find", "GET", req);

            if ((int)findAccResp["re"] == 0)
            {
                JArray accs = (JArray)findAccResp["response"]["funding_accounts"];

                foreach (JObject foundAcc in accs)
                {
                    if("iban".Equals((string)foundAcc["account_number_type"]))
                    {
                        JObject iban = new JObject();
                        iban["iban"] = foundAcc["account_number"];

                        if ("priority".Equals((string)foundAcc["payment_type"]))
                        {
                            iban["type"] = "SWIFT";
                        } 
                        else
                        {
                            iban["type"] = "";
                        }
                        ibans.Add(iban);
                    }
                }
            }


            req.Clear();
            req.Add("account_id", Utils.jsonValue(acc, "externalReference", true));
            req.Add("currency", "GBP");

            findAccResp = _currencyCloudGate.SendRequest("/v2/funding_accounts/find", "GET", req);

            if ((int)findAccResp["re"] == 0)
            {
                JArray accs = (JArray)findAccResp["response"]["funding_accounts"];

                foreach (JObject foundAcc in accs)
                {
                    if ("account_number".Equals((string)foundAcc["account_number_type"]) &&
                        "GB".Equals((string)foundAcc["bank_country"]))
                    {
                        JObject iban = new JObject();
                        iban["account_number"] = foundAcc["account_number"];
                        iban["routing_code"] = foundAcc["routing_code"];
                        iban["type"] = "FPS";
                        ibans.Add(iban);
                    }
                }
            }

            if (ibans.Count > 0)
            {
                result = new JObject();
                result.Add("module", "integrations");
                result.Add("hook", "accountIBANs");
                result.Add("document", Utils.jsonValue(acc, "id", false));
                result.Add("externalReference", (string)acc["externalReference"]);
                result.Add("IBANS", ibans.ToString(Newtonsoft.Json.Formatting.None));

                results.Add(result);
            }

            return results;
        }
        public JObject[] SendToBank(JObject payment)
        {
            List<JObject> results = new List<JObject>();
            JObject result = new JObject();
            result.Add("integration", "currency-cloud");

            MappedDiagnosticsLogicalContext.Set("req_type", "SendToBank");
            log.Debug($"SendToBank id = " + Utils.jsonValue(payment, "id", true));

            string on_behalf_of = null;
            if (!"".Equals(Utils.jsonValue(payment, "SenderAccountContactReference", false)))
                on_behalf_of = (string)payment["SenderAccountContactReference"];

            string doc_id = Utils.jsonValue(payment, "id", false);
            string eventId = Utils.jsonValue(payment, "eventId", false);


            // request

            NameValueCollection req = new NameValueCollection();
            JObject check;


            req.Add("currency", (string)payment["Currency"]);
            if ("FPS".Equals((string)payment["PaymentType"]))
                req.Add("account_number", (string)payment["BenefAccount"]);
            else
                req.Add("iban", (string)payment["BenefAccount"]);
            //req.Add("bic_swift", (string)payment["BenefBankBIC"]);
            //req.Add("account_number", (string)payment["BenefAccount"]);
            if (on_behalf_of != null)
                req.Add("on_behalf_of", on_behalf_of);

            string cc_payment_type;
            if ("SWIFT".Equals((string)payment["PaymentType"]))
                { cc_payment_type = "priority"; }
            else
                { cc_payment_type = "regular"; }


            if ("FPS".Equals((string)payment["PaymentType"]))
            {
                req.Add("routing_code_type[0]", "sort_code");
                req.Add("routing_code_value[0]", Utils.jsonValue(payment, "BenefBankRoutingCode", true));
            }

            JObject findBenef = _currencyCloudGate.SendRequest("/v2/beneficiaries/find", "POST", req);

            string benefId = "";
            string name = "";
            string benefType = (string)payment["BenefType"];
            if ((int)findBenef["re"] == 0)
            {
                JArray benefs = (JArray)findBenef["response"]["beneficiaries"];
                benefId = "";

                foreach (JObject benef in benefs)
                {
                    if (benef.ContainsKey("payment_types") && (benef["payment_types"] is JArray))
                    {
                        foreach(string pay_type in (JArray)benef["payment_types"])
                        {
                            if(cc_payment_type.Equals(pay_type))
                            {
                                benefId = (string)benef["id"];
                            }
                        }
                    }
                }
                log.Trace($"found benef id={benefId} for payment_type={cc_payment_type}");

                if ("".Equals(benefId))
                {
                    //todo create benef
                    NameValueCollection req1 = new NameValueCollection();
                    if ("person".Equals(benefType)) 
                        benefType = "individual";

                    if ((string)payment["BenefFirstName"] != "")
                        name = (string)payment["BenefFirstName"];
                    if ((string)payment["BenefLastName"] != "")
                        name = name + " " + (string)payment["BenefLastName"];

                    req1.Add("name", name);
                    if ("individual".Equals(benefType))
                    {
                        req1.Add("beneficiary_entity_type", "individual");
                        req1.Add("beneficiary_first_name", (string)payment["BenefFirstName"]);
                        req1.Add("beneficiary_last_name", (string)payment["BenefLastName"]);
                    }
                    else if ("company".Equals(benefType))
                    {
                        req1.Add("beneficiary_entity_type", "company");
                        req1.Add("beneficiary_company_name", name);
                    }
                    
                    req1.Add("payment_types[]", cc_payment_type); 

                    req1.Add("bank_account_holder_name", name);
                    req1.Add("currency", (string)payment["Currency"]);
                    req1.Add("bank_country", (string)payment["BenefBankCountry"]);
                    if ("FPS".Equals((string)payment["PaymentType"]))
                        req1.Add("account_number", (string)payment["BenefAccount"]);
                    else
					{	
						Utils.AddRequestParam(req1, "bic_swift", payment, "BenefBankBIC", false, false);
                        req1.Add("iban", (string)payment["BenefAccount"]);
					}
                    Utils.AddRequestParam(req1, "beneficiary_country", payment, "BenefCountry", false, false);
                    Utils.AddRequestParam(req1, "beneficiary_city", payment, "BenefCity", false, false);
                    Utils.AddRequestParam(req1, "beneficiary_address", payment, "BenefAddress", false, false);

                    if ("FPS".Equals((string)payment["PaymentType"]))
                    {
                        req1.Add("routing_code_type_1", "sort_code");
                        Utils.AddRequestParam(req1, "routing_code_value_1", payment, "BenefBankRoutingCode", false, false);
                    }

                    if (on_behalf_of != null)
                        req1.Add("on_behalf_of", on_behalf_of);

                    JObject createBenef = _currencyCloudGate.SendRequest("/v2/beneficiaries/create", "POST", req1);
                    if ((int)createBenef["re"] == 0)
                    {
                        benefId = (string)createBenef["response"]["id"];
                        log.Trace("Create benef id:" + benefId);
                    }
                    else
                    {
                        result.Add("module", "integrations");
                        result.Add("hook", "apiError");
                        result.Add("document", doc_id);
                        result.Add("error", createBenef.ToString()); // todo use ResponseChecker

                    }
                }
            }
            else
            {
                result.Add("module", "integrations");
                result.Add("hook", "apiError");
                result.Add("document", doc_id);
                result.Add("error", findBenef.ToString()); // todo use ResponseChecker

            }


            if (!"".Equals(benefId)) {
                ResponseChecker responseChecker = new ResponseChecker(payment);
                string house_acc_id = getHouseAcc(Utils.jsonValue(payment, "Currency", true));

                if (makeTransfersToHouseAccount)
                {
                    req.Clear();
                    req.Add("source_account_id", house_acc_id);
                    req.Add("destination_account_id", Utils.jsonValue(payment, "SenderAccountReference", true));
                    req.Add("currency", Utils.jsonValue(payment, "Currency", true));
                    req.Add("amount", Utils.jsonValue(payment, "Amount", true));
                    req.Add("reason", "balancing outgoing payment " + doc_id);
                    if (!"".Equals(eventId)) req.Add("unique_request_id", "bal_out_" + eventId);

                    JObject transferToHouse = _currencyCloudGate.SendRequest("/v2/transfers/create", "POST", req);
                    check = responseChecker.CheckResponse(transferToHouse, "transfer to sub-account");
                    if (responseChecker.error) { log.Error(responseChecker.errorText); return new JObject[] { check }; }
                }

                NameValueCollection req2 = new NameValueCollection();
                if (on_behalf_of != null)
                    req2.Add("on_behalf_of", on_behalf_of);
                
                req2.Add("currency", (string)payment["Currency"]);
                req2.Add("beneficiary_id", benefId);
                req2.Add("amount", (string)payment["Amount"]);
                req2.Add("reason", (string)payment["PaymentDetails"]);
                req2.Add("reference", (string)payment["Reference"]);
                if (!"".Equals(Utils.jsonValue(payment, "Reference", false)))
                {
                    req2.Add("unique_request_id", Utils.jsonValue(payment, "Reference", false));
                }

                req2.Add("payment_type", cc_payment_type);

                /*
                NameValueCollection req3 = new NameValueCollection();
                Utils.AddRequestParam(req3, "payer_country", payment, "PayerCountry", true, false);
                req3.Add("payer_entity_type", ((int)payment["isCompany"] == 0 ? "individual" : "company"));
                req3.Add("payment_type", cc_payment_type);
                req3.Add("currency", (string)payment["Currency"]);
                
                JObject payerDetails = _currencyCloudGate.SendRequest("/v2/reference/payer_required_details", "GET", req3);
                log.Trace($"payerDetails:{payerDetails}");
                HashSet<string> required_fields = new HashSet<string>();
                if ((int)payerDetails["re"] == 0)
                {
                    JArray fields = (JArray)payerDetails.SelectToken("response.details[0].required_fields");
                    if (fields!=null)
                    {
                        foreach(JObject field in fields)
                        {
                            if(field.SelectToken("name")!=null) required_fields.Add((string)field["name"]);
                        }
                    }
                }
                log.Trace($"required_fields:{required_fields}");

                if ((int)payment["isCompany"] == 0)
                {
                    req2.Add("payer_entity_type", "individual");
                    if(required_fields.Contains("payer_first_name")) req2.Add("payer_first_name", (string)payment["PayerFirstName"]);
                    if (required_fields.Contains("payer_last_name")) req2.Add("payer_last_name", (string)payment["PayerLastName"]);
                    if (required_fields.Contains("payer_date_of_birth")) req2.Add("payer_date_of_birth", (string)payment["PayerDateOfBirth"]);
                }
                else
                {
                    req2.Add("payer_entity_type", "company");
                    if (required_fields.Contains("payer_company_name")) req2.Add("payer_company_name", (string)payment["PayerCompanyName"]);
                    if (required_fields.Contains("payer_identification_type")) req2.Add("payer_identification_type", "incorporation_number");
                    if (required_fields.Contains("payer_identification_type")) Utils.AddRequestParam(req2, "payer_identification_value", payment, "PayerRegistrationNumber", true, false);
                }

                if (required_fields.Contains("payer_country"))  Utils.AddRequestParam(req2, "payer_country", payment, "PayerCountry", true, false);
                if (required_fields.Contains("payer_city"))     Utils.AddRequestParam(req2, "payer_city",    payment, "PayerCity", true, false);
                if (required_fields.Contains("payer_address"))  Utils.AddRequestParam(req2, "payer_address", payment, "PayerAddress", true, false);
                if (required_fields.Contains("payer_postcode")) Utils.AddRequestParam(req2, "payer_postcode",payment, "PayerPostcode", true, false);
                
                */

                JObject createPayment = _currencyCloudGate.SendRequest("/v2/payments/create", "POST", req2);
                string paymentId = "";

                if ((int)createPayment["re"] == 0)
                {
                    paymentId = (string)createPayment["response"]["id"];
                    log.Trace("Create Payment id:" + paymentId);
                    result.Add("module", "integrations");
                    result.Add("hook", "sentToBank");
                    result.Add("document", doc_id);
                    result.Add("externalReference", paymentId); // todo replace with real reference

                    if(deductTransferFeeToHouseAccount && !"".Equals(Utils.jsonValue(payment, "FeeAmount", false)))
                    {
                        req.Clear();
                        req.Add("source_account_id", Utils.jsonValue(payment, "SenderAccountReference", true));
                        req.Add("destination_account_id", house_acc_id);
                        req.Add("currency", Utils.jsonValue(payment, "Currency", true));
                        req.Add("amount", Utils.jsonValue(payment, "FeeAmount", true));
                        req.Add("reason", "payment fee for " + doc_id);
                        if (!"".Equals(eventId)) req.Add("unique_request_id", "payment_fee_" + doc_id);

                        JObject transferToSubAcc = _currencyCloudGate.SendRequest("/v2/transfers/create", "POST", req);
                        check = responseChecker.CheckResponse(transferToSubAcc, "rollback transfer to sub-account");
                        if (responseChecker.error) { log.Error(responseChecker.errorText); results.Add(check); }
                    }
                }
                else
                {
                    result.Add("module", "integrations");
                    result.Add("hook", "apiError");
                    result.Add("document", doc_id);
                    result.Add("error", createPayment.ToString()); // todo use ResponseChecker

                    if (makeTransfersToHouseAccount)
                    {
                        req.Clear();
                        req.Add("source_account_id", Utils.jsonValue(payment, "SenderAccountReference", true));
                        req.Add("destination_account_id", house_acc_id);
                        req.Add("currency", Utils.jsonValue(payment, "Currency", true));
                        req.Add("amount", Utils.jsonValue(payment, "Amount", true));
                        req.Add("reason", "rollback balancing outgoing payment " + doc_id);
                        if (!"".Equals(eventId)) req.Add("unique_request_id", "rb_bal_out_" + eventId);

                        JObject transferToSubAcc = _currencyCloudGate.SendRequest("/v2/transfers/create", "POST", req);
                        check = responseChecker.CheckResponse(transferToSubAcc, "rollback transfer to sub-account");
                        if (responseChecker.error) { log.Error(responseChecker.errorText); return new JObject[] { check }; }
                    }

                }
            }
            results.Add(result);

            return results.ToArray();
        }


        public JObject[] GetStatus(JObject payment)
        {
            Utils.jsonValue(payment, "externalReference", true);

            JObject result = new JObject();
            result.Add("module", "integrations");
            result.Add("integration", "currency-cloud");


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
            JObject response = _currencyCloudGate.SendRequest($"/v2/payments/{externalReference}", "GET",null);
            ResponseChecker responseChecker = new ResponseChecker(payment);
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
                case "ready_to_send":
                    break;
                // Error statuses
                case "failed":
                    result = "providerError";
                    break;
                //business statuses
                case "released":
                    result = "accepted";
                    break;
                case "completed":
                    result = "settled";
                    break;
            }

            return result;
        }

        private string ConvertTransferStatus(string wuStatus)
        {
            string result = "";
            switch (wuStatus)
            {
                //Empty or not final status
                case "ready_to_send":
                    break;
                // Error statuses
                case "failed":
                    result = "providerError";
                    break;
                //business statuses
                case "released":
                    result = "accepted";
                    break;
                case "completed":
                    result = "settled";
                    break;
            }

            return result;
        }

        public JObject[] GetBalanace(JObject account)
        {
            JObject result = new JObject();

            MappedDiagnosticsLogicalContext.Set("req_type", "GetBalanace");
            Utils.jsonValue(account, "id", true);
            log.Debug($"GetBalanace id = {account["id"]}");
            string currencyCode = Utils.jsonValue(account, "Currency",true);

            // request
            JObject response = _currencyCloudGate.SendRequest($"/holdingBalance/{currencyCode}", "GET", null);

            ResponseChecker responseChecker = new ResponseChecker(account);
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


        public JObject[] GetQuote(JObject request)
        {
            JObject result = new JObject
            {
                { "integration", "currency-cloud" }
            };

            return new JObject[] { RateQuote(request) };

            MappedDiagnosticsLogicalContext.Set("req_type", "GetQuote");
            string id = Utils.jsonValue(request, "id", false);
            log.Debug($"GetQuote id = {id}");


            // request
            JObject response = _currencyCloudGate.SendRequest($"/quotes", "POST", null);
            ResponseChecker responseChecker = new ResponseChecker(request);
            JObject check = responseChecker.CheckResponse(response);
            if (responseChecker.error) return new JObject[] { check };


            // Parse Response

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

        public JObject[] OrderByQuote(JObject order)
        {
            JObject result = new JObject();
            result.Add("module", "integrations");

            MappedDiagnosticsLogicalContext.Set("req_type", "OrderByQuote");
            Utils.jsonValue(order, "id", true);
            log.Debug($"OrderByQuote id = {order["id"]}");


            // request
            ResponseChecker responseChecker = new ResponseChecker(order);
            NameValueCollection req = new NameValueCollection();

            req.Add("buy_currency",  Utils.jsonValue(order, "buyCurrency", true));
            req.Add("sell_currency", Utils.jsonValue(order, "sellCurrency", true));

            Utils.jsonValue(order, "isSellAmount", true);
            bool isSellAmount = (bool)order["isSellAmount"];

            req.Add("fixed_side", isSellAmount ? "sell" : "buy");

            req.Add("amount", Utils.jsonValue(order, "amount", true));
            req.Add("term_agreement", "true");
            //req.Add("conversion_date", "2022-03-24");
            req.Add("conversion_date_preference", "earliest");

            string contactRef = Utils.jsonValue(order, "AccountContactReference", false); 
            if (!"".Equals(contactRef)) req.Add("on_behalf_of", contactRef);

            JObject createConversion = _currencyCloudGate.SendRequest("/v2/conversions/create", "POST", req);
            JObject check = responseChecker.CheckResponse(createConversion, "conversions create");
            if (responseChecker.error) { return new JObject[] { check }; }

            string conversionId = createConversion.SelectToken("response.id")?.ToString();
            string status = createConversion.SelectToken("response.status")?.ToString();
            log.Trace("Create conversion id:" + conversionId);

            result.Add("hook", "sentToBank");
            result.Add("document", Utils.jsonValue(order, "id", false));
            result.Add("sellCurrency", createConversion.SelectToken("response.sell_currency")?.ToString());
            result.Add("sellAmount", createConversion.SelectToken("response.client_sell_amount"));
            result.Add("buyCurrency", createConversion.SelectToken("response.buy_currency")?.ToString());
            result.Add("buyAmount", createConversion.SelectToken("response.client_buy_amount"));
            result.Add("rate", createConversion.SelectToken("response.client_rate"));
            result.Add("conversionDate", ((DateTime)createConversion.SelectToken("response.conversion_date")).ToUniversalTime());
            result.Add("settlementDate", ((DateTime)createConversion.SelectToken("response.settlement_date")).ToUniversalTime());
            result.Add("externalReference", conversionId);
            result.Add("externalStatus", status);

            return new JObject[] { result };
        }

        public JObject[] GetOrderStatus(JObject conversion)
        {
            JObject result = new JObject
            {
                { "module", "integrations" },
                { "integration", "currency-cloud" }
            };

            MappedDiagnosticsLogicalContext.Set("req_type", "GetOrderStatus");
            Utils.jsonValue(conversion, "id", true);
            log.Debug($"GetOrderStatus id = {conversion["id"]}");

            Utils.jsonValue(conversion, "externalReference", true);
            string externalReference = (string)conversion["externalReference"];
            result.Add("externalReference", externalReference);

            // request
            JObject response = _currencyCloudGate.SendRequest($"/v2/conversions/{externalReference}", "GET", null);

            if ((int)response["re"] == 0)
            {
                // Parse Response
                string status = response.SelectToken("response.status")?.ToString();

                result.Add("hook", ConvertOrderStatus(status));
            }

            return new JObject[] { result };
        }

        private string ConvertOrderStatus(string wuStatus)
        {
            string result = "";
            switch (wuStatus)
            {
                //Empty or not final status
                case "awaiting_funds":
                    result = "sentToBank";
                    break;
                case "funds_arrived":
                    result = "accepted";
                    break;
                case "trade_settled":
                    result = "settled";
                    break;
                case "closed":
                    result = "cancelled";
                    break;
            }

            return result;
        }

        private decimal GetAmount(int Amount, string Currency)
        {
            int multi = 1;
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
            JObject response = _currencyCloudGate.SendRequest($"/webhooks", "GET", null);
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
                        response = _currencyCloudGate.SendRequest($"/webhooks/"+ id, "DELETE", null);
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
                response = _currencyCloudGate.SendRequest($"/webhooks", "POST", null);
                check = responseChecker.CheckResponse(response);
                if (responseChecker.error) return new JObject[] { check };
            }


            JObject response2 = _currencyCloudGate.SendRequest($"/webhooks", "GET", null);
            log.Debug($"WebhooksSetup result={response2.ToString()}");


            return new JObject[] { response2 };
        }

        public JObject[] CacheAllRates(JObject request)
        {
            MappedDiagnosticsLogicalContext.Set("req_type", "CacheAllRates");

            DateTime nowUtc = DateTime.Now.ToUniversalTime();
            if (nowUtc.DayOfWeek.Equals(DayOfWeek.Saturday)) return new JObject[0];
            if (nowUtc.DayOfWeek.Equals(DayOfWeek.Sunday)) return new JObject[0];

            List<JObject> results = new List<JObject>();

            RedisValue[] keys = _redisDb.HashKeys("currencyCloudRates");
            string currPairs = string.Join(',', Array.FindAll(keys, k=>k.ToString().Length==6));
            
            if(!"".Equals(""+ (string)request["currencyPairs"]))
                currPairs += ","+ (string)request["currencyPairs"];

            NameValueCollection req = new NameValueCollection();
            req["currency_pair"] = currPairs;

            log.Info("refresh currpairs: "+currPairs);

            JObject ratesRes = _currencyCloudGate.SendRequest("/v2/rates/find", "GET", req);
            if ((int)ratesRes["re"] == 0)
            {
                JObject rates = (JObject)ratesRes.SelectToken("response.rates");

                DateTime now = DateTime.Now;
                foreach (JProperty property in rates.Properties())
                {
                    JObject quote = new JObject()
                    {
                        {"date",now.ToString("s",DateTimeFormatInfo.InvariantInfo) + "Z"},
                        {"bid",((float)property.Value[0]).ToString("0.00000000")},
                        {"ask",((float)property.Value[1]).ToString("0.00000000")},
                    };

                    _redisDb.HashSet("currencyCloudRates", property.Name, quote.ToString());
                }

                JArray unavailable = (JArray)ratesRes.SelectToken("response.unavailable");
                foreach (Object currPair in unavailable)
                {
                    _redisDb.HashSet("currencyCloudRates", currPair.ToString(), "{}");
                }
                _redisDb.HashSet("currencyCloudRates", "error", "");
            }
            else
            {
                _redisDb.HashSet("currencyCloudRates", "error", (string)ratesRes["error"]);
            }

            return results.ToArray();
        }

        private string getHouseAcc(string currency)
        {
            if (houseAccId.ContainsKey(currency)) return houseAccId[currency];
            NameValueCollection req = new NameValueCollection();
            req.Add("currency", currency);
            JObject houseAccReq = _currencyCloudGate.SendRequest("/v2/funding_accounts/find", "GET", req);
            if ((int)houseAccReq["re"] == 0)
            {
                houseAccId[currency] = (string)houseAccReq.SelectToken("response.funding_accounts[0].account_id");
            }
            else
            {
                log.Error("can't get house account");
                return null;
            }

            return houseAccId[currency];
        }
    }
}
