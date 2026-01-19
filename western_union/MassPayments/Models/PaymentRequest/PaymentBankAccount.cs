using Newtonsoft.Json.Linq;
using System;

namespace eu.advapay.core.hub.western_union
{
    internal sealed class PaymentBankAccount
    {
        public PaymentBankAccount(JObject json)
        {
            Id = json["id"].ToString();
            VersionedOn = DateTime.UtcNow.AddMinutes(-2).ToString("yyyy-MM-ddTHH:mm:ssZ");
            BankName = Utils.jsonValue(json, "BenefBankName", true);
            BankCode = Utils.jsonValue(json, "BenefBankBIC", true);
            BankBranchCode = Utils.jsonValue(json, "BenefBankBranchCode", false);
            BankAccount = Utils.jsonValue(json, "BenefAccount", true);
            Address = new PaymentAddress(
                Utils.jsonValue(json, "BenefBankAddress", false),
                Utils.jsonValue(json, "BenefBankCity", false),
                Utils.jsonValue(json, "BenefBankState", false),
                Utils.jsonValue(json, "BenefBankZip", false),
                Utils.jsonValue(json, "BenefBankCountry", true),
                0);
        }

        public JObject GetWUjson()
        {
            var jsonObject = new JObject();
            jsonObject.Add("id", Id);
            jsonObject.Add("versionedOn", VersionedOn);
            jsonObject.Add("accountNumber", BankAccount);
            jsonObject.Add("bankName", BankName);
            jsonObject.Add("bankCode", BankCode);
            jsonObject.Add("bankBranchCode", BankBranchCode);
            jsonObject.Add("address", Address.GetWUjson());
            return jsonObject;
        }

        private string Id { get; }

        private string VersionedOn { get; }

        private string BankName { get; }

        private string BankAccount { get; }

        private string BankCode { get; }

        private string BankBranchCode { get; }

        public PaymentAddress Address { get; }
        //string BenefBankCountry { get; }
    }
}
