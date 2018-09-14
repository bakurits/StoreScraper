using System;
using System.Threading;
using System.Threading.Tasks;
using CheckoutBot.Factory;
using CheckoutBot.Models.Checkout;
using OpenQA.Selenium.Support.UI;
using StoreScraper.Core;

namespace CheckoutBot.CheckoutBots.FootSites.EastBay
{
    public class EastBayBot : FootSitesBotBase
    {
        private const string ApiUrl = "http://pciis02.eastbay.com/api/v2/productlaunch/ReleaseCalendar/1";
        private static readonly object lock1 = new object();

        public EastBayBot() : base("EastBay", "https://www.eastbay.com", ApiUrl)
        {
        }


        public EastBayBot(string websiteName, string webSiteBaseUrl, string releasePageEndpoint) : base(websiteName,
            webSiteBaseUrl, releasePageEndpoint)
        {
        }

        public int DelayInSecond { private get; set; } = 2;

        public override void Login(string username, string password, CancellationToken token)
        {
            Driver.Url = WebsiteBaseUrl;
            Task.Delay(DelayInSecond * 1000, token).Wait(token);
            Driver.EvalScript(GetScriptByXpath("//div[@id='header_account_button']/a/span") + ".click();");
            Task.Delay(DelayInSecond * 1000, token).Wait(token);
            Driver.QueueScriptCall($"{GetScriptByXpath("//input[@id='login_email']")}.value=\"{username}\"");
            Driver.QueueScriptCall($"{GetScriptByXpath("//input[@id='login_password']")}.value=\"{password}\"");
            Driver.QueueScriptCall($"{GetScriptByXpath("//input[@id='login_submit']")}.click()");
            Task.Delay(DelayInSecond * 1000, token).Wait(token);
        }

        public override void GuestCheckOut(GuestCheckoutSettings settings, CancellationToken token)
        {
            var driver = DriverFactory.CreateFirefoxDriver();
            driver.Navigate().GoToUrl(WebsiteBaseUrl);
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(2));

            var cartContainer = GetClickableElementByXPath("//div[@id = 'header_cart_button']", wait, token);
            cartContainer.Click();

            var select = new SelectElement(GetVisibleElementByXPath("//select[@id = 'billCountry']", wait, token));
            try
            {
                select.SelectByText(settings.Shipping.Country.ToString());
            }
            catch
            {
                Logger.Instance.WriteErrorLog("This country isn't available");
            }


            throw new NotImplementedException();
        }

        public override void AccountCheckout(AccountCheckoutSettings settings, CancellationToken token)
        {
            Driver.Url = settings.ProductToBuy.Url;
            Task.Delay(DelayInSecond * 1000, token).Wait(token);
            Driver.EvalScript(GetScriptByXpath("//div[@id='header_account_button']/a/span") + ".click();");
            Task.Delay(DelayInSecond * 1000, token).Wait(token);
            Driver.QueueScriptCall($"{GetScriptByXpath("//input[@id='login_email']")}.value=\"{settings.UserLogin}\"");
            Driver.QueueScriptCall(
                $"{GetScriptByXpath("//input[@id='login_password']")}.value=\"{settings.UserPassword}\"");
            Driver.QueueScriptCall($"{GetScriptByXpath("//input[@id='login_submit']")}.click()");
            Task.Delay(DelayInSecond * 1000, token).Wait(token);
            string getReq = $@"
var xhr = new XMLHttpRequest();
var date = Date.now();
xhr.open('GET',
    'https://www.eastbay.com/pdp/gateway?requestKey=' +
    requestKey +
    '&action=add&qty=1&sku={settings.ProductToBuy.Id}&size={settings.BuyOptions.Size}&fulfillmentType=SHIP_TO_HOME&storeNumber=0&_=' +
    date);
xhr.onload = function() {{
    if (xhr.status === 200) {{
        console.log(xhr.responseText);
    }} else {{
        alert('Request failed.  Returned status of ' + xhr.status);
    }}
}};
xhr.send();";
            // Console.WriteLine(getReq);
            Driver.EvalScript(getReq);


        }
    }
}