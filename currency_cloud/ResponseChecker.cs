using Newtonsoft.Json.Linq;
using System;

namespace eu.advapay.core.hub.currency_cloud
{
    internal sealed class ResponseChecker
    {
        public int errorCode;
        public string errorText;
        public string hook;
        public bool error;
        public int statusCode;
        public JObject data;

        public ResponseChecker(JObject data)
        {
            this.data = data;
        }
        public JObject CheckResponse(JObject response)
        {
            return CheckResponse(response, null);
        }

        public JObject CheckResponse(JObject response,string errMsg)
        {
            errorText = "";
            errorCode = -1;
            hook = "";
            error = false;
            statusCode = response.SelectToken("statusCode") != null ? (int)response["statusCode"] : -1;

            int ii;

            JObject result = new JObject();
            result.Add("module", "integrations");
            if (data.ContainsKey("id") && Int32.TryParse((string)data["id"], out ii)) result.Add("document", data["id"]);
            if (data.ContainsKey("externalReference")) result.Add("externalReference", data["externalReference"]);
            
            int re = int.Parse(response["re"].ToString());
            switch (re)
            {
                case 0:
                    break;
                case -1:
                case -2:
                case -3:
                case -4:
                case -5:
                    hook = "apiError";
                    errorText = (errMsg==null ? "" : errMsg+"") + response["error"].ToString();
                    error = true;
                    break;
            }


            result.Add("hook", hook);

            if (errorText != "")
                result.Add("error", errorText);

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
            if (description != "")
            {
                string fields = errorMessage.Substring(errorMessage.IndexOf(":") + 1);
                errorText = $"{errorCode}. {description}: {fields}";
                return;
            }
            errorText = errorMessage;
            return;
        }

        private string GetDescription(int errorCode)
        {
            string description = "";

            return description;
        }
    }
}
