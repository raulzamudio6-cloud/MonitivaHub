using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using NLog;

namespace eu.advapay.core.hub.western_union
{
    internal sealed class PaymentRequest
    {
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();
        private const int MaxDetailsLen = 140;

        public PaymentRequest(JObject json, string customerID)
        {
            if (json == null)
                throw new ArgumentNullException(nameof(json));

            CustomerID = customerID;
            CurrencyCode = Utils.jsonValue(json, "Currency", true);
            //log.Debug($"CurrencyCode='" + Utils.jsonValue(json, "Currency", true) + "'");
            if (!decimal.TryParse(Utils.jsonValue(json, "Amount", true), out decimal amountCurrency))
            {
                amountCurrency = decimal.Parse(Utils.jsonValue(json, "Amount", true), CultureInfo.InvariantCulture);
            }
            int multi = int.Parse(Math.Pow(10, CurrencyMinorUnit.getInstance().GetMinorUnit(CurrencyCode)).ToString());
            amountCurrency = decimal.Multiply(amountCurrency, multi);
            Amount = Convert.ToInt32(amountCurrency);

            BankAccount = new PaymentBankAccount(json);
            string bankCountryCode = BankAccount.Address.CountryCode;
            //PaymentMethod = "ACH";
            Boolean isACH = ACHCountryCurrency.getInstance().isACH(bankCountryCode, CurrencyCode);
            if (isACH)
                PaymentMethod = "ACH";
            else
                PaymentMethod = "wire";
            Id = Utils.jsonValue(json, "id", true);

            log.Debug($"bankCountryCode='" + bankCountryCode + ", CurrencyCode="+ CurrencyCode+ ", PaymentMethod="+ PaymentMethod+"'");
            Beneficiary = new PaymentBeneficiary(json);
            //log.Debug($"Beneficiary='" + Beneficiary.GetWUjson() + "'");


            if (!"".Equals(Utils.jsonValue(json, "IntBankBIC", false)))
                IntermediaryBank = new PaymentIntermediaryBank(json);

            string payer_bussines_name = Utils.jsonValue(json, "payer_bussines_name", false);
            //log.Debug($"payer_bussines_name='" + payer_bussines_name + "'");
            //Console.WriteLine("payer_bussines_name='" + payer_bussines_name+"'");
            if (!"".Equals(Utils.jsonValue(json, "payer_bussines_name", false)))
                ThirdPartyRemitter = new PaymentThirdPartyRemitter(json);
            //log.Debug($"ThirdPartyRemitter='" + ThirdPartyRemitter.GetWUjson() + "'");

            PurposeOfPayment = Utils.jsonValue(json, "PaymentDetails", false);

            if (!"".Equals(Utils.jsonValue(json, "purposeOfPaymentCode", false)))
                purposeOfPaymentCode = Utils.jsonValue(json, "purposeOfPaymentCode", false);
            else
                purposeOfPaymentCode = "";

            log.Debug($"PurposeOfPayment='" + PurposeOfPayment+"'" + ", purposeOfPaymentCode='" + purposeOfPaymentCode + "'");
            if (!"".Equals(Utils.jsonValue(json, "PaymentDetails", false)))
            {
                Ref = new JArray();
                IEnumerable<string> shortenedLines = SplitStringIntoLines(Utils.jsonValue(json, "PaymentDetails", false), MaxDetailsLen);
                foreach (string shortenedLine in shortenedLines)
                    Ref.Add(new JObject()
                            {
                                {"ref", shortenedLine}
                            });
            }

            IntegrationsAdditionalFields = Utils.jsonValue(json, "IntegrationsAdditionalFields", false);
            log.Debug($"IntegrationsAdditionalFields='" + IntegrationsAdditionalFields + "'");
        }

        public string GetWUjson()
        {
            var jsonObject = new JObject();
            jsonObject.Add("customerId", CustomerID);
            jsonObject.Add("paymentMethod", PaymentMethod);
            jsonObject.Add("id", Id);
            jsonObject.Add("partnerReference", Id);
            jsonObject.Add("amount", Amount);
            jsonObject.Add("currencyCode", CurrencyCode);
            jsonObject.Add("beneficiary", Beneficiary.GetWUjson());
            jsonObject.Add("bankAccount", BankAccount.GetWUjson());
            if (IntermediaryBank != null)
                jsonObject.Add("intermediaryBank", IntermediaryBank.GetWUjson());
            if (ThirdPartyRemitter != null)
            {
                jsonObject.Add("thirdPartyRemitter", ThirdPartyRemitter.GetWUjson());
            }

            jsonObject.Add("purposeOfPayment", PurposeOfPayment);
            jsonObject.Add("paymentReference", PurposeOfPayment);
            if (!"".Equals(purposeOfPaymentCode))
                jsonObject.Add("purposeOfPaymentCode", purposeOfPaymentCode);
            
            if (PaymentMethod.Equals("wire"))
              jsonObject.Add("chargeType", "OUR");

            JObject addFields = null;
            try
            {
                if (!String.IsNullOrEmpty(IntegrationsAdditionalFields))
                  addFields = JObject.Parse(IntegrationsAdditionalFields);
            }
            catch (Exception e)
            {
                log.Error(e, "error");
            }
            if (addFields != null)
            {
                foreach (JProperty prop in addFields.Properties())
                {
                    Utils.SetValueByPath(jsonObject, prop.Name, prop.Value.ToString());
                }
            }

            //if (Ref != null && Ref.Count > 0)
            //    jsonObject.Add("remittanceData", Ref);
            var payments = new JArray();
            payments.Add(jsonObject);
            var request = new JObject();
            request.Add("paymentToProcess", payments);


            return request.ToString();
        }

        private static IEnumerable<string> SplitStringIntoLines(string sourceString, int lineLength)
        {
            if (lineLength <= 0)
                throw new ArgumentException($"Value of argument {nameof(lineLength)} must be positive");

            if (sourceString.Length <= lineLength)
                return new[] { sourceString };

            var linesCount = (int)Math.Ceiling((double)sourceString.Length / lineLength);
            var lines = new string[linesCount];
            for (int lineIndex = 0; lineIndex < linesCount; lineIndex++)
            {
                int charsLeft = sourceString.Length - lineIndex * lineLength;
                int lengthToTake = Math.Min(lineLength, charsLeft);
                lines[lineIndex] = sourceString.Substring(lineIndex * lineLength, lengthToTake);
            }

            return lines;
        }

        private string CustomerID { get; }

        private string PaymentMethod { get; }

        private string Id { get; }

        private int Amount { get; }

        private string CurrencyCode { get; }

        private PaymentBeneficiary Beneficiary { get; }

        private PaymentBankAccount BankAccount { get; }

        private PaymentIntermediaryBank IntermediaryBank { get; }

        private PaymentThirdPartyRemitter ThirdPartyRemitter { get; }

        private JArray Ref { get; }

        private string PurposeOfPayment { get; }
        private string purposeOfPaymentCode { get; }
        private string IntegrationsAdditionalFields { get; }
    }
}
