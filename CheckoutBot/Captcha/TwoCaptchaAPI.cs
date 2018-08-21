using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CheckoutBot.Captcha;
using HtmlAgilityPack;
using StoreScraper.Helpers;
using StoreScraper.Http.Factory;

namespace CheckoutBot.TwoCaptcha 
{
    public class TwoCaptchaAPI : CaptchaAPIBase
    {
        public override bool GetCaptchaResponse(string siteKey, string url, out string result)
        {
            string requestUrl = "http://2captcha.com/in.php?key=" + _apiKey + "&method=userrecaptcha&googlekey=" + siteKey + "&pageurl=" + url;

            try
            {
                HtmlNode node = ClientFactory.GeneralClient.GetDoc(requestUrl, CancellationToken.None).DocumentNode;
                string response = node.InnerHtml;
                if (response.Length < 3)
                {
                    result = response;
                    return false;
                }
                else
                {
                    if (response.Substring(0, 3) == "OK|")
                    {
                        string captchaID = response.Remove(0, 3);
                        string secondRequestUrl = "http://2captcha.com/res.php?key=" + _apiKey + "&action=get&id=" + captchaID;

                        for (int i = 0; i < 24; i++)
                        {
                            HtmlNode answerNode = ClientFactory.GeneralClient.GetDoc(secondRequestUrl, CancellationToken.None).DocumentNode;
                            string answerResponse = answerNode.InnerHtml;

                            if (answerResponse.Length < 3)
                            {
                                result = answerResponse;
                                return false;
                            }
                            else
                            {
                                if (answerResponse.Substring(0, 3) == "OK|")
                                {
                                    result = answerResponse.Remove(0, 3);
                                    return true;
                                }
                                else if (answerResponse != "CAPCHA_NOT_READY")
                                {
                                    result = answerResponse;
                                    return false;
                                }
                            }

                        Thread.Sleep(5000);
                        }

                        result = "Timeout";
                        return false;
                    }
                    else
                    {
                        result = response;
                        return false;
                    }
                }
                    
            }
            catch { }
            result = "Unknown error";
            return false;
        }

        public TwoCaptchaAPI(string apiKey) : base(apiKey)
        {
        }
    }
}
