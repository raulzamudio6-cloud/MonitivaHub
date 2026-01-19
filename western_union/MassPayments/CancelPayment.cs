using Newtonsoft.Json.Linq;

namespace eu.advapay.core.hub.western_union
{
    public sealed class CancelPayment
    {
        private readonly string _url;
        private readonly string _certificatePath;
        private readonly string _certificatePassword;

        public CancelPayment(string url, string certificatePath, string certificatePassword)
        {
            _url = url;
            _certificatePath = certificatePath;
            _certificatePassword = certificatePassword;
        }

        public JObject Cancel(string paymentId)
        {
            WesternUnionGate westernUnionGate = new WesternUnionGate(_url, _certificatePath, _certificatePassword);
            JObject response = westernUnionGate.SendRequest($"/payments/{paymentId}", "DELETE","");
            ResponseChecker responseChecker = new ResponseChecker();
            return responseChecker.CheckResponse(response);
        }
    }
}
