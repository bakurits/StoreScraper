using System.Threading;
using CheckoutBot.Captcha;
using HtmlAgilityPack;
using StoreScraper.Helpers;
using StoreScraper.Http.Factory;
using System.Threading.Tasks;

namespace CheckoutBot.TwoCaptcha 
{
    public class TwoCaptchaAPI : CaptchaAPIBase
    {
        public async override Task<string> GetCaptchaResponse(string siteKey, string url)
        {
            string requestUrl = "http://2captcha.com/in.php?key=" + _apiKey + "&method=userrecaptcha&googlekey=" + siteKey + "&pageurl=" + url;
           
            try
            {
                HtmlDocument doc = await ClientFactory.GeneralClient.GetDocTask(requestUrl, CancellationToken.None);
                var node = doc.DocumentNode;
                string response = node.InnerHtml;
                if (response.Length < 3)
                {
                    return "";
                }
                else
                {
                    if (response.Substring(0, 3) == "OK|")
                    {
                        string captchaID = response.Remove(0, 3);
                        string secondRequestUrl = "http://2captcha.com/res.php?key=" + _apiKey + "&action=get&id=" + captchaID;

                        for (int i = 0; i < 24; i++)
                        {
                            HtmlDocument answerDoc = await ClientFactory.GeneralClient.GetDocTask(secondRequestUrl, CancellationToken.None);
                            var answerNode = answerDoc.DocumentNode;
                             string answerResponse = answerNode.InnerHtml;

                            if (answerResponse.Length < 3)
                            {
                                return "";
                            }
                            else
                            {
                                if (answerResponse.Substring(0, 3) == "OK|")
                                {
                                    return answerResponse.Remove(0, 3);
                                }
                                else if (answerResponse != "CAPCHA_NOT_READY")
                                {
                                    return "";
                                }
                            }

                        Thread.Sleep(5000);
                        }

                        return "";
                    }
                    else
                    {
                        return "";
                    }
                }
                    
            }
            catch { }
            return "";
        }

        public TwoCaptchaAPI(string apiKey) : base(apiKey)
        {
        }
    }
}
