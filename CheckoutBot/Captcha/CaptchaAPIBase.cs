using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckoutBot.Captcha
{
    public abstract class CaptchaAPIBase
    {
        private string _apiKey;

        public CaptchaAPIBase(string apiKey)
        {
            _apiKey = apiKey;
        }

        public abstract bool GetCaptchaResponse(string siteKey, string url, out string result);
    }
}
