using Newtonsoft.Json.Linq;

namespace eu.advapay.core.hub.western_union
{
    public sealed class CreateOrder
    {
        private const string MethodUrl = "/orders";
        private const string Method = "POST";
        private readonly string _url;
        private readonly string _certificatePath;
        private readonly string _certificatePassword;

        public CreateOrder(string url, string certificatePath, string certificatePassword)
        {
            _url = url;
            _certificatePath = certificatePath;
            _certificatePassword = certificatePassword;
        }

        public JObject Send(string request)
        {
            string url = $"{_url}{MethodUrl}";
            WesternUnionGate westernUnionGate = new WesternUnionGate(url, _certificatePath, _certificatePassword);
            JObject response = westernUnionGate.SendRequest(MethodUrl, Method,request);
            ResponseChecker responseChecker = new ResponseChecker();
            return responseChecker.CheckResponse(response);
        }
    }
}
