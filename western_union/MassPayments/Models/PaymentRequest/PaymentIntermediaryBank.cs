using Newtonsoft.Json.Linq;

namespace eu.advapay.core.hub.western_union
{
    internal sealed class PaymentIntermediaryBank
    {
        public PaymentIntermediaryBank(JObject json)
        {
            BankName = Utils.jsonValue(json, "IntBankName", true);
            BankCode = Utils.jsonValue(json, "IntBankBIC", false);
            AccountNumber = Utils.jsonValue(json, "IntBankBIC", false);
            BankBranchCode = Utils.jsonValue(json, "IntBankBranchCode", false);
            Address = new PaymentAddress(
                Utils.jsonValue(json, "IntBankAddress", false),
                Utils.jsonValue(json, "IntBankCity", false),
                Utils.jsonValue(json, "IntBankState", false),
                Utils.jsonValue(json, "IntBankZip", false),
                Utils.jsonValue(json, "IntBankCountry", true),
                0);
        }

        public JObject GetWUjson()
        {
            var jsonObject = new JObject();
            jsonObject.Add("bankName", BankName);
            jsonObject.Add("bankCode", BankCode);
            jsonObject.Add("accountNumber", AccountNumber);
            jsonObject.Add("bankBranchCode", BankBranchCode);
            jsonObject.Add("address", Address.GetWUjson());
            return jsonObject;
        }

        private string BankName { get; }

        private string BankCode { get; }

        private string AccountNumber { get; }

        private string BankBranchCode { get; }

        private PaymentAddress Address { get; }
    }
}
