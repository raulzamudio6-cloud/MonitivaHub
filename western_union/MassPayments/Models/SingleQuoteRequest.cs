using Newtonsoft.Json.Linq;
using System;
using System.Globalization;

namespace eu.advapay.core.hub.western_union
{
    internal sealed class SingleQuoteRequest
    {
        public SingleQuoteRequest(JObject json, string customerID)
        {
            if (json == null)
                throw new ArgumentNullException(nameof(json));

            CustomerID = customerID;
            TradeCurrency = Utils.jsonValue(json, "buyCurrency", true);
            SettlementCurrency = Utils.jsonValue(json, "sellCurrency",true);
            IsSettlementAmount = false;
            
            Utils.jsonValue(json, "isSellAmount", true);
            IsSettlementAmount = (bool)json["isSellAmount"];

            if (!decimal.TryParse(Utils.jsonValue(json, "amount",true), out decimal amountCurrency))
            {
                amountCurrency = decimal.Parse(Utils.jsonValue(json, "amount",true), CultureInfo.InvariantCulture);
            }
            int multi;
            if (IsSettlementAmount)
                multi = int.Parse(Math.Pow(10, CurrencyMinorUnit.getInstance().GetMinorUnit(SettlementCurrency)).ToString());
            else
                multi = int.Parse(Math.Pow(10, CurrencyMinorUnit.getInstance().GetMinorUnit(TradeCurrency)).ToString());
            amountCurrency = decimal.Multiply(amountCurrency, multi);
            Amount = Convert.ToInt32(amountCurrency);
        }

        public string GetWUjson()
        {
            var jsonObject = new JObject();
            jsonObject.Add("customerId", CustomerID);
            var items = new JArray();
            items.Add(new JObject()
                    {
                        {"amount", Amount},
                        {"isAmountSettlement", $"{IsSettlementAmount.ToString().ToLowerInvariant()}" },
                        {"tradeCurrency", TradeCurrency},
                        {"settlementCurrency", SettlementCurrency}
                    });
            jsonObject.Add("itemsToQuote", items);
            return jsonObject.ToString();
        }

        private string CustomerID { get; }

        private int Amount { get; }

        private string TradeCurrency { get; }

        private string SettlementCurrency { get; }

        private bool IsSettlementAmount { get; }
    }
}
