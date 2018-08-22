using System.Threading.Tasks;
using Nuget_AntiCaptcha;
using Nuget_AntiCaptcha.Captchas;
using Nuget_AntiCaptcha.Responses;

namespace CheckoutBot.Captcha
{
    public class AntiCaptchaAPI : CaptchaAPIBase
    {
        public override async Task<string> GetCaptchaResponse(string siteKey, string url)
        {
            AntiCaptcha client = new AntiCaptcha(_apiKey);
            NoCaptchaTaskProxyless task = new NoCaptchaTaskProxyless
            {
                websiteKey = siteKey,
                websiteURL = url
            };
            TaskResponse response = await client.SubmitTask(task);

            while (true)
            {
                NoCaptchaSolution solution = await client.GetNoCaptchaSolution(response.taskId);
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
