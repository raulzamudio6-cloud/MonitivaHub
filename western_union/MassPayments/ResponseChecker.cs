using Newtonsoft.Json.Linq;
using System;
using NLog;
using System.IO;
using System.Collections.Generic;

namespace eu.advapay.core.hub.western_union
{
    internal sealed class ResponseChecker
    {
        public int errorCode;
        public string errorText;
        public string hook;
        public bool error;
        public int statusCode;
        public JObject data;
//        public JObject errorslist;
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();
        private Dictionary<string, string> errorDict;


        public ResponseChecker()
        {
            this.data = new JObject();
            this.errorDict = new Dictionary<string, string>();

        }

        public ResponseChecker(JObject data, Dictionary<string, string> errorDict)
        {
            this.data = data;
            this.errorDict = errorDict;
        }


        public JObject CheckResponse(JObject response)
        {
            log.Debug($"CheckResponse='" + response.ToString());
            errorText = "";
            errorCode = -1;
            hook = "";
            error = false;
            statusCode = response.SelectToken("statusCode") != null ? (int)response["statusCode"] : -1;
            int ii;

            log.Debug($"CheckResponse statusCode=" + statusCode.ToString() + " data=" + data.ToString());

            JObject result = new JObject();
            result.Add("module", "integrations");
            if (data.ContainsKey("id") && Int32.TryParse((string)data["id"], out ii)) result.Add("document", data["id"]);
            if (data.ContainsKey("externalReference")) result.Add("externalReference", data["externalReference"]);
            
            int re = int.Parse(response["re"].ToString());
            log.Debug($"re='" + re.ToString() + "'");
            switch (re)
            {
                case 0:
                    if (response.SelectToken("response.errorCode") != null)
                    {
                        ParseErrorText(response["response"]["errorCode"].ToString());
                        if(errorCode == 1000 || errorCode == 1001)
                        {
                            // todo postpone task
                        }
                    }
                    break;
                case -1:
                case -2:
                case -3:
                case -4:
                case -5:
                    hook = "apiError";
                    if (response.SelectToken("error") != null)
                    {
                        errorText = response["error"].ToString();
                    }
                    error = true;
                    break;
            }


            result.Add("hook", hook);

            if (errorText != "")
            {
/*
               // JObject myerror = (JObject)response["error"];
                ParseErrorText(response["error"].ToString());
                int start = errorText.IndexOf("{");
                int end = errorText.IndexOf("}");
                errorText = errorText.Substring(start,end-start+1);
                log.Debug($"errorText='" + errorText + "'");
                JObject myerror = JObject.Parse(errorText);

                string errorCodeStr = myerror["errorCode"].ToString();
                log.Debug($"CheckResponse errorCodeStr='" + errorCodeStr);

                if (!errorDict.TryGetValue(errorCodeStr, out errorText))
                {
                    log.Debug($"CheckResponse errorText='" + errorText);
                    if (myerror.ContainsKey("errorDescription"))
                      errorText = myerror["errorDescription"].ToString();
                    else
                      errorText = myerror["errorCode"].ToString();
                }
*/

                result.Add("error", errorText);
            }
            return result;
        }


        // set errorCode, errorText
        public void ParseErrorText(string errorMessage)
        {
            if (errorMessage == null) errorMessage = "";
            string description;

            if (!errorMessage.Contains(":"))
            {
                if (!int.TryParse(errorMessage, out errorCode))
                {
                    errorText = errorMessage;
                    return;
                }
                description = GetDescription(errorCode);
                if (description != "")
                {
                    errorText = description;
                    return;
                }
            }
            if (!int.TryParse(errorMessage.Substring(0, errorMessage.IndexOf(":")), out errorCode))
            {
                errorText = errorMessage;
                return;
            }
            description = GetDescription(errorCode);
            log.Debug($"ParseErrorText='" + errorMessage + " errorCode=" + errorCode.ToString() + " description=" + description);
            if (description != "")
            {
                string fields = errorMessage.Substring(errorMessage.IndexOf(":") + 1);
                errorText = $"{errorCode}. {description}: {fields}";
                log.Debug($"fields='" + fields + " errorText=" + errorText);
                return;
            }
            errorText = errorMessage;
            return;
        }

        private string GetDescription(int errorCode)
        {
            string description = "";
            switch (errorCode)
            {
                case 1000:
                    description = "Generic Mass Payments Web Service Error";
                    break;
                case 1001:
                    description = "Access denied";
                    break;
                case 1003:
                    description = "Required Field Validation Error";
                    break;
                case 1004:
                    description = "Conditional Field Validation Error";
                    break;
                case 1005:
                    description = "Input Field Value Validation Error";
                    break;
                case 1006:
                    description = "Unexpected Field Submitted";
                    break;
                case 1007:
                    description = "Defined Limit Exceeded";
                    break;
                case 1101:
                    description = "Resource Not Found";
                    break;
                case 1102:
                    description = "Quote Invalid";
                    break;
                case 1103:
                    description = "Quote Expired";
                    break;
                case 1104:
                    description = "Payment Update Not Supported";
                    break;
                case 1105:
                    description = "Payment Cannot be Cancelled";
                    break;
                case 1106:
                    description = "Re-submission of a Cancelled Payment is Not Supported";
                    break;
                case 1107:
                    description = "Customer Cannot be Found";
                    break;
                case 1112:
                    description = "Currency Not Supported";
                    break;
                case 1115:
                    description = "Unsupported Currency and Payment Method Combination";
                    break;
                case 1116:
                    description = "Payment Method Invalid or Not Supported";
                    break;
                case 1117:
                    description = "Remittance Type Invalid or Not Supported";
                    break;
                case 1118:
                    description = "Remittance Type Mismatch";
                    break;
                case 1119:
                    description = "Number of Orders Exceed Limit";
                    break;
                case 1120:
                    description = "Primary webhook cannot be deleted";
                    break;
                case 1202:
                    description = "Quote and Order Details Do Not Match";
                    break;
                case 1209:
                    description = "Total amount of the order exceeds the customers debit limit";
                    break;
                case 1210:
                    description = "Customer is not setup to use settlement method";
                    break;
                case 1213:
                    description = "It is not possible to carry a holding balance in this currency";
                    break;
                case 1218:
                    description = "An error occurred on WUBS side when generating payment instructions for bank";
                    break;
                case 1220:
                    description = "An error occurred when creating an order";
                    break;
                case 1222:
                    description = "Error in FAB Service";
                    break;
                case 1223:
                    description = "Invalid Address";
                    break;
            }

            return description;
        }
    }
}
