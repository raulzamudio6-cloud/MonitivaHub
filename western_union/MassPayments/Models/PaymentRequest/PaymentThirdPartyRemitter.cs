using Newtonsoft.Json.Linq;
using System;

namespace eu.advapay.core.hub.western_union
{
    internal sealed class PaymentThirdPartyRemitter
    {
        public PaymentThirdPartyRemitter(JObject json)
        {
            Id = json["id"].ToString();
            VersionedOn = DateTime.UtcNow.AddMinutes(-2).ToString("yyyy-MM-ddTHH:mm:ssZ");
            Type = Utils.jsonValue(json, "payer_type", true);
            businessName = Utils.jsonValue(json, "payer_bussines_name", false);
            state = Utils.jsonValue(json, "payer_state", false);
            //if (state.Equals(""))
            //    state = "ON";
            CurrencyCode = Utils.jsonValue(json, "Currency", true);
            SenderAccountIBAN = Utils.jsonValue(json, "SenderAccountIBAN", false);
            SenderAccountSWIFT = json.ContainsKey("SenderAccountSWIFT") ? (string)json["SenderAccountSWIFT"] : null;


            Address = new PaymentAddress(
                Utils.jsonValue(json, "payer_address", false),
                Utils.jsonValue(json, "payer_city", false),
                state,
                //Utils.jsonValue(json, "payer_state", false),
                Utils.jsonValue(json, "payer_postcode", false),
                Utils.jsonValue(json, "payer_country", false),
                0, false);
        }

        public JObject GetWUjson()
        {
            var jsonObject = new JObject();
            jsonObject.Add("id", Id);
            jsonObject.Add("versionedOn", VersionedOn);
            jsonObject.Add("type", Type);
            jsonObject.Add("businessName", businessName);
            jsonObject.Add("address", Address.GetWUjson());
            if (!"".Equals(SenderAccountIBAN))
            {
                jsonObject.Add("bankAccount", new JObject()
                {
                    { "accountNumber", SenderAccountIBAN},
//                    { "accountCurrency", CurrencyCode},
                    { "bankRoutingCode", SenderAccountIBAN.Substring(SenderAccountIBAN.Length>5 ?SenderAccountIBAN.Length-5:SenderAccountIBAN.Length)}
                });
                if (SenderAccountSWIFT != null)
                {
                    jsonObject["bankAccount"]["bankCode"] = SenderAccountSWIFT;
                    jsonObject.Add("bankAccountDetails", new JObject()
                    {
                        { "bankCode", SenderAccountSWIFT},
                    });
                }
            }
            return jsonObject;
        }

        private string Id { get; }

        private string VersionedOn { get; }

        private string Type { get; }

        private string businessName { get; }
        private string state { get; }

        private string SenderAccountIBAN { get; }
        private string SenderAccountSWIFT { get; }
        private string CurrencyCode { get; }

        private PaymentAddress Address { get; }
    }
}
