using System.Threading.Tasks;

namespace CheckoutBot.Captcha
{
    public abstract class CaptchaAPIBase
    {
        protected string _apiKey;

        public CaptchaAPIBase(string apiKey)
        {
            _apiKey = apiKey;
        }

        public abstract Task<string> GetCaptchaResponse(string siteKey, string url);
    }
}
