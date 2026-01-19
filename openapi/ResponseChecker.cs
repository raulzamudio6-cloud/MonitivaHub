using Newtonsoft.Json.Linq;
using System;
using NLog;
using System.IO;
using System.Collections.Generic;

namespace eu.advapay.core.hub.openapi
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


        public ResponseChecker()
        {
            this.data = new JObject();

        }

        public ResponseChecker(JObject data)
        {
            this.data = data;
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
                /*description = GetDescription(errorCode);
                if (description != "")
                {
                    errorText = description;
                    return;
                }*/
            }
            if (!int.TryParse(errorMessage.Substring(0, errorMessage.IndexOf(":")), out errorCode))
            {
                errorText = errorMessage;
                return;
            }
/*            description = GetDescription(errorCode);
            log.Debug($"ParseErrorText='" + errorMessage + " errorCode=" + errorCode.ToString() + " description=" + description);
            if (description != "")
            {
                string fields = errorMessage.Substring(errorMessage.IndexOf(":") + 1);
                errorText = $"{errorCode}. {description}: {fields}";
                log.Debug($"fields='" + fields + " errorText=" + errorText);
                return;
            }*/
            errorText = errorMessage;
            return;
        }

    }
}
