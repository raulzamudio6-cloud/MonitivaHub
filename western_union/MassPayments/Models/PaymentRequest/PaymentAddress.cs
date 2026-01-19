using Newtonsoft.Json.Linq;

namespace eu.advapay.core.hub.western_union
{
    internal sealed class PaymentAddress
    {
        private const int MaxAddressLen = 35;

        public PaymentAddress(string address, string city, string stateOrProv, string zip, string country, int useSOP, bool use3line = true)
        {
            city = city.Replace(".", " ");
            city = city.Replace(",", " ");
            city = city.Trim();
            address = address.Replace(".", " ");
            address = address.Replace(",", " ");
            address = address.Trim();
            if (address.Length > MaxAddressLen)
            {
                Address1 = address.Substring(0, MaxAddressLen);
                if ((address.Length > MaxAddressLen*2) && use3line)
                {
                    Address2 = address.Substring(MaxAddressLen, MaxAddressLen);
                    if (address.Length > MaxAddressLen*3)
                        Address3 = address.Substring(MaxAddressLen*2, MaxAddressLen);
                    else
                        Address3 = address.Substring(MaxAddressLen*2, address.Length - MaxAddressLen*2);
                }
                else
                    Address2 = address.Substring(MaxAddressLen, address.Length - MaxAddressLen);
            }
            else
                Address1 = address;
            City = city;
            StateOrProv = stateOrProv;
            /*   if (country.Equals("MX") || country.Equals("CA") || country.Equals("US"))
                   StateOrProv = stateOrProv;
               else
                   StateOrProv = "";// stateOrProv;*/
            ZipOrPostal = zip;
            CountryCode = country;
            UseSOP = useSOP;
        }

        public JObject GetWUjson()
        {
            var jsonObject = new JObject();
            jsonObject.Add("line1", Address1);
            if (!string.IsNullOrEmpty(Address2))
                jsonObject.Add("line2", Address2);
            if (!string.IsNullOrEmpty(Address3))
                jsonObject.Add("line3", Address3);
            if (!string.IsNullOrEmpty(City))
                jsonObject.Add("city", City);

            if (!string.IsNullOrEmpty(StateOrProv))
                jsonObject.Add("stateOrProv", StateOrProv);
            else
                jsonObject.Add("stateOrProv", "ON");

            if (!string.IsNullOrEmpty(ZipOrPostal))
                jsonObject.Add("zipOrPostal", ZipOrPostal);
            if (!string.IsNullOrEmpty(CountryCode))
                jsonObject.Add("countryCode", CountryCode);
            return jsonObject;
        }

        private string Address1 { get; }

        private string Address2 { get; }

        private string Address3 { get; }

        private string City { get; }

        private string StateOrProv { get; }

        private string ZipOrPostal { get; }
        private int UseSOP { get; }

        public string CountryCode { get; }
    }
}
