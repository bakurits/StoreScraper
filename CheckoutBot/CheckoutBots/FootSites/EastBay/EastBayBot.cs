using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CheckoutBot.Interfaces;
using CheckoutBot.Models;
using CheckoutBot.Models.Checkout;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StoreScraper.Helpers;
using StoreScraper.Http.Factory;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace CheckoutBot.CheckoutBots.FootSites.EastBay
{
    [JsonObject]
    public class EastBayBot : FootSitesBotBase, IProxyChecker
    {
        private const string ApiUrl = "http://pciis02.eastbay.com/api/v2/productlaunch/ReleaseCalendar/1";
        private const string CartUrl = "https://www.eastbay.com/shoppingcart";

        public EastBayBot() : base("EastBay", "https://www.eastbay.com", ApiUrl)
        {
        }


        public EastBayBot(string websiteName, string webSiteBaseUrl, string releasePageEndpoint) : base(websiteName,
            webSiteBaseUrl, releasePageEndpoint)
        {
        }

        [JsonIgnore]
        public int DelayInSecond { private get; set; } = 2;

        public override bool Login(string username, string password, CancellationToken token)
        {
            var webView = Browser.ActiveTab;
            webView.LoadUrlAndWait(WebsiteBaseUrl);
            Task.Delay(4000, token).Wait(token);
            webView.QueueScriptCall(GetScriptByXpath("//div[@id='header_account_button']/a/span") + ".click();").WaitOne();
            Task.Delay(4000, token).Wait(token);
            

            webView.QueueScriptCall($"{GetScriptByXpath("//input[@id='login_email']")}.value=\"{username}\"").WaitOne();
            webView.QueueScriptCall($"{GetScriptByXpath("//input[@id='login_password']")}.value=\"{password}\"").WaitOne();
            webView.QueueScriptCall($"{GetScriptByXpath("//input[@id='login_submit']")}.click()").WaitOne();
            Task.Delay(5000, token).Wait(token);
            
            string isWrong = (string) webView.EvalScript(@"document.getElementById(""login_password_error"").innerHTML");
            //Console.WriteLine(isWrong);
            return isWrong.Length == 0;
        }

        public override void GuestCheckOut(GuestCheckoutSettings settings, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This method adds product to cart
        /// </summary>
        /// <param name="settings"> Checkout settings </param>
        /// <param name="token"></param>
        protected override void AddToCart(AccountCheckoutSettings settings, CancellationToken token)
        {
            var script = AjaxGetRequest($@"'https://www.eastbay.com/pdp/gateway?requestKey=' +
                            requestKey +
                            '&action=add&qty={settings.BuyOptions.Quantity}&sku={settings.ProductToBuy.Sku}&size={
                    settings.BuyOptions.Size
                }&fulfillmentType=SHIP_TO_HOME&storeNumber=0&_=' +
                            date");
            Browser.ActiveTab.QueueScriptCall(script).WaitOne();
            Task.Delay(200, token).Wait(token);
        }


        protected override void FinalCheckout(AccountCheckoutSettings settings, CancellationToken token)
        {
            Browser.ActiveTab.QueueScriptCall($@"document.getElementById(""payMethodPaneStoredCCCVV"").value = ""{settings.UserCcv2}""").WaitOne();
            Debug.WriteLine(Browser.ActiveTab.LastJSException);
            Task.Delay(200, token).Wait(token);
            Browser.ActiveTab.QueueScriptCall(@"document.getElementById(""orderSubmit"").click()").WaitOne();
        }

        /// <summary>
        /// This method gets random item from releases page
        /// </summary>
        protected override FootsitesProduct GetArbitraryItem(CancellationToken token)
        {
            FootsitesProduct product = ScrapeReleasePage(token).Where(prod => (!prod.LaunchCountdownEnabled && prod.ReleaseTime < DateTime.UtcNow)).ToList()[0];
            GetProductSizes(product, token);
            return product;
        }
        
        /// <summary>
        /// This method adds Arbitrary item to cart
        /// </summary>
        /// <param name="product"> Item to add </param>
        /// <param name="token"></param>
        protected override void AddArbitraryItem(FootsitesProduct product , CancellationToken token)
        {
            AddToCart(new AccountCheckoutSettings()
            {
                ProductToBuy = product,
                BuyOptions = new ProductBuyOptions()
                {
                    Quantity = 1,
                    Size = product.Sizes[0]
                }
            }, token);
          
        }


        protected override void GoToCheckoutPage(CancellationToken token)
        {
            //Browser.ActiveTab.LoadUrl(CartUrl);
            Browser.ActiveTab.LoadUrlAndWait("https://www.eastbay.com/checkout/?uri=checkout");
            Task.Delay(4000, token).Wait(token);
            ClickContinueButton();
            Browser.ActiveTab.QueueScriptCall("document.getElementById(\"cart_checkout_button\").click();").WaitOne(); //load checkout page
            Debug.WriteLine(Browser.ActiveTab.LastJSException);
            Task.Delay(5000, token).Wait(token);

            
            Task.Delay(4000, token).Wait(token);
        }

        /// <summary>
        /// Navigates to cart page and removes arbitrary product from cart
        /// </summary>
        /// <param name="product"> product to remove </param>
        /// <param name="token"></param>
        protected override void RemoveArbitraryItem(FootsitesProduct product, CancellationToken token)
        {
            Browser.ActiveTab.LoadUrlAndWait(CartUrl);
            Task.Delay(4000, token).Wait(token);
            ClickContinueButton();
            Debug.WriteLine(GetScriptByXpath("//div[@id = 'cart_items']/ul/li[@data-sku = '" + product.Sku + "']/div/span/div/a[@data-btntype = 'remove']/span[2]") + ".click()");
            Browser.ActiveTab.QueueScriptCall(GetScriptByXpath("//div[@id = 'cart_items']/ul/li[@data-sku = '" + product.Sku + "']/div/span/div/a[@data-btntype = 'remove']/span[2]") + ".click()").WaitOne();
            /*driver.EvalScript($@"
                    var xhr = new XMLHttpRequest();
                    var date = Date.now();
                    var requestKey = window.frames.accountGateway._requestKey;
                    var itemId = {lineItemIdScript}
                    xhr.open('GET', 'https://www.eastbay.com/pdp/gateway?requestKey=' + requestKey + '&action=delete&lineItemId='+ itemId + '&_=' + date);
                    xhr.onload = function() {{
                        if (xhr.status === 200) {{
                            console.log(xhr.responseText);
                        }}
                        else {{
                            alert('Request failed.  Returned status of ' + xhr.status);
                        }}
                    }};
                    xhr.send();
            ");*/
        }


        /// <summary>
        /// Blocks current thread until product will be released
        /// </summary>
        /// <param name="model">unique model code of product to wait until release</param>
        /// <param name="token"></param>
        protected override void WaitBeforeRelease(string model, CancellationToken token)
        {
            bool released = false;

            do
            {
                var allReleases = ScrapeReleasePage(token);
                released = !allReleases.Find(p => p.Model == model)?.LaunchCountdownEnabled?? false;
                Task.Delay(25, token).Wait(token);
            } while (!released);
        }
        
        
        protected override void RemoveAllItems(CancellationToken token)
        {
            Browser.ActiveTab.LoadUrlAndWait(CartUrl);
            Task.Delay(4000, token).Wait(token);
            ClickContinueButton();
            Task.Delay(2000, token).Wait(token);
            Browser.ActiveTab.QueueScriptCall(
                @"var items = document.getElementById(""cart_items"").getElementsByTagName(""ul"")[0].getElementsByTagName(""li"");
                for (var i = 0; i < items.length; i++) {
                    var item = items[i];
                    item.querySelectorAll('[title=""Remove Item From Cart""]')[0].click();
                }").WaitOne();
            Debug.WriteLine(Browser.ActiveTab.LastJSException);
            Task.Delay(2000, token).Wait(token);
        }

        protected override void LogOut(CancellationToken token)
        {
            var webView = Browser.ActiveTab;
            webView.LoadUrlAndWait(WebsiteBaseUrl);
            Task.Delay(4000, token).Wait(token);
            webView.QueueScriptCall(GetScriptByXpath("//div[@id='header_account_button']/a/span") + ".click();").WaitOne();

            Task.Delay(DelayInSecond * 1000, token).Wait(token);
            
            webView.QueueScriptCall("document.getElementById(\"header_registered_logout_link\").click()").WaitOne();
            Task.Delay(DelayInSecond * 1000, token).Wait(token);
            
        }

        public void GetProductSizes(FootsitesProduct product, CancellationToken token)
        {
            List<string> infos = new List<string>();
            var client = ClientFactory.CreateHttpClient(autoCookies: true).AddHeaders(ClientFactory.FireFoxHeaders);
            var document = client.GetDoc(product.Url, token).DocumentNode;
            int ind = document.InnerHtml.IndexOf("var styles = ", StringComparison.Ordinal);
            var sizeData = Utils.GetFirstJson(document.InnerHtml.Substring(ind));
            var sizesForCurProd = (JArray)sizeData[product.Sku][7];

            foreach (var item in sizesForCurProd)
            {
                var t = (JArray) item;
                var s = (string)t[0];
                infos.Add(s.Trim());
            }

            product.Sizes = infos;
        }
        
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="proxyPool"></param>
        /// <param name="maxCount"></param>
        /// <returns></returns>
        public List<WebProxy> ChooseBestProxies(List<WebProxy> proxyPool, int maxCount)
        {
            List<Tuple<long, WebProxy>> lst = new List<Tuple<long, WebProxy>>();
            var lst1 = lst;
            Parallel.ForEach(proxyPool, proxy =>
            {
                using (HttpClient client = ClientFactory.CreateProxiedHttpClient(proxy, true).AddHeaders(ClientFactory.DefaultHeaders))
                {
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();
                    
                    try
                    {
                        var doc = client.GetDoc(WebsiteBaseUrl, CancellationToken.None);
                        stopwatch.Stop();
                        if (!IsFaulted(doc))
                        {
                            lock (lst1)
                            {
                                lst1.Add(new Tuple<long, WebProxy>(stopwatch.ElapsedMilliseconds, proxy));   
                            }     
                        }    
                    }
                    catch
                    {
                        // ignored
                    }
                }
            });

            lst = lst.OrderBy(item => item.Item1).ToList().GetRange(0, Math.Min( maxCount, lst.Count));
            List<WebProxy> ans = new List<WebProxy>();
            foreach (var item in lst)
            {
                ans.Add(item.Item2);
                Debug.WriteLine(item.Item1);
            }
            
            return ans;
        }

        private bool IsFaulted(HtmlDocument doc)
        {
            var ind = doc.DocumentNode.SelectSingleNode("//title").InnerHtml.IndexOf("Access Denied", StringComparison.Ordinal);
            return ind != -1;
        }
        
        /// <summary>
        /// Removes disturbing error messages from page
        /// </summary>
        private void ClickContinueButton()
        {
            Browser.ActiveTab.QueueScriptCall(@"var but = document.getElementById(""attention_continue"");
                                                if (but !== null) 
	                                            but.click()").WaitOne();
        }
    }
}