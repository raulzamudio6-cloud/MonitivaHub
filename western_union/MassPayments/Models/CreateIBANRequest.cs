using Newtonsoft.Json.Linq;
using System;
using System.Globalization;

namespace eu.advapay.core.hub.western_union
{
    internal sealed class CreateIBANRequest
    {
        public CreateIBANRequest(JObject json)
        {
            if (json == null)
                throw new ArgumentNullException(nameof(json));

            iban = Utils.jsonValue(json, "iBanValue", true);
            countryCode = Utils.jsonValue(json, "countryCode", true);
        }

        public string GetWUjson()
        {
            var jsonObject = new JObject();
            jsonObject.Add("iBanValue", iban);
            jsonObject.Add("countryCode", countryCode);
            return jsonObject.ToString();
        }

        private string iban { get; }
        private string countryCode { get; }
    }
    internal sealed class CreateBICRequest
    {
        public CreateBICRequest(JObject json)
        {
            if (json == null)
                throw new ArgumentNullException(nameof(json));

            bic = Utils.jsonValue(json, "bankCode", true);
            countryCode = Utils.jsonValue(json, "countryCode", true);
        }

        public string GetWUjson()
        {
            var jsonObject = new JObject();
            jsonObject.Add("bankCode", bic);
            jsonObject.Add("countryCode", countryCode);
            return jsonObject.ToString();
        }

        private string bic { get; }
        private string countryCode { get; }
    }
}
