using Newtonsoft.Json.Linq;
using System;

namespace eu.advapay.core.hub.western_union
{
    internal sealed class PaymentBeneficiary
    {
        public PaymentBeneficiary(JObject json)
        {
            Id = json["id"].ToString();
            VersionedOn = DateTime.UtcNow.AddMinutes(-2).ToString("yyyy-MM-ddTHH:mm:ssZ");
            if (json["Currency"].ToString() == "MXN")
                TaxId = Utils.jsonValue(json, "BenefTaxid", false);

            switch (json["BenefType"].ToString())
            {
                case "person":
                    Type = "individual";
                    FirstName = Utils.jsonValue(json,"BenefFirstName",true);
                    LastName = Utils.jsonValue(json, "BenefLastName", true);
                    break;
                case "company":
                    Type = "business";
                    BusinessName = Utils.jsonValue(json, "BenefCompanyName", true);
                    break;
            }

            Address = new PaymentAddress(
                Utils.jsonValue(json, "BenefAddress", false),
                Utils.jsonValue(json, "BenefCity", false),
                Utils.jsonValue(json, "BenefState", false),
                Utils.jsonValue(json, "BenefZip", false),
                Utils.jsonValue(json, "BenefCountry", true),
                0);
        }

        public JObject GetWUjson()
        {
            var jsonObject = new JObject();
            jsonObject.Add("id", Id);
            jsonObject.Add("versionedOn", VersionedOn);
            jsonObject.Add("type", Type);
            if (!string.IsNullOrEmpty(TaxId))
                jsonObject.Add("taxid", TaxId);

            switch (Type)
            {
                case "individual":
                    jsonObject.Add("firstName", FirstName);
                    jsonObject.Add("lastName", LastName);
                    break;
                case "business":
                    jsonObject.Add("businessName", BusinessName);
                    break;
            }

            jsonObject.Add("address", Address.GetWUjson());

            return jsonObject;
        }


        private string Id { get; }

        private string VersionedOn { get; }

        private string Type { get; }

        private string FirstName { get; }

        private string LastName { get; }

        private string BusinessName { get; }

        private string TaxId { get; }

        private PaymentAddress Address { get; }
    }
}
