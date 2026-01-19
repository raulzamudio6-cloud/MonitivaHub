using Newtonsoft.Json.Linq;
using System;
using System.Globalization;

namespace eu.advapay.core.hub.western_union
{
    internal sealed class CreateOrderRequest
    {
        public CreateOrderRequest(JObject json, string customerID)
        {
            if (json == null)
                throw new ArgumentNullException(nameof(json));

            CustomerId = customerID;
            QuoteId = Utils.jsonValue(json,"quoteId",true);
            TradeCurrency = Utils.jsonValue(json, "buyCurrency", true);
            SettlementCurrency = Utils.jsonValue(json, "sellCurrency", true);
            IsSettlementAmount = false;
            Utils.jsonValue(json, "isSellAmount", true);
            IsSettlementAmount = (bool)json["isSellAmount"];

            if (!decimal.TryParse(Utils.jsonValue(json, "amount", true), out decimal amountCurrency))
            {
                amountCurrency = decimal.Parse(Utils.jsonValue(json, "amount", true), CultureInfo.InvariantCulture);
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
            jsonObject.Add("customerId", CustomerId);
            jsonObject.Add("quoteId", QuoteId);
            var items = new JArray();
            items.Add(new JObject()
                    {
                        {"amount", Amount},
                        {"tradeCurrency", TradeCurrency},
                        {"isAmountSettlement", $"{IsSettlementAmount.ToString().ToLowerInvariant()}" }
                    });
            var orders = new JArray();
            orders.Add(new JObject()
                    {
                        {"settlementCurrency", SettlementCurrency},
//                        {"settlementMethod", "wire"},
                        {"settlementMethod", "undefined"},
                        {"itemsToBook", items}
                    });
            jsonObject.Add("ordersToBook", orders);
            return jsonObject.ToString();
        }

        private string CustomerId { get; }

        private string QuoteId { get; }

        private int Amount { get; }

        private string TradeCurrency { get; }

        private string SettlementCurrency { get; }

        private bool IsSettlementAmount { get; }
    }
}
