using CheckoutBot.Captcha;
using Nuget_AntiCaptcha;
using Nuget_AntiCaptcha.Responses;
using Nuget_AntiCaptcha.Captchas;
using System.Threading.Tasks;

namespace CheckoutBot.Anti_Captcha
{
    public class AntiCaptchaAPI : CaptchaAPIBase
    {
        public async override Task<string> GetCaptchaResponse(string siteKey, string url)
        {
            AntiCaptcha Client = new AntiCaptcha(_apiKey);
            NoCaptchaTaskProxyless task = new NoCaptchaTaskProxyless
            {
                websiteKey = siteKey,
                websiteURL = url
            };
            TaskResponse response = await Client.SubmitTask(task);

            while (true)
            {
                NoCaptchaSolution solution = await Client.GetNoCaptchaSolution(response.taskId);
                if (solution.errorId != 0)
                {
                    return "";
                }
                if (solution.status == "ready")
                {
                    return solution.solution.gRecaptchaResponse;
                }
            }
        }

        public AntiCaptchaAPI(string apiKey) : base(apiKey)
        {

        }
    }
}
