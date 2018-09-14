﻿using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CheckoutBot.Factory;
using CheckoutBot.Models.Checkout;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using StoreScraper.Http.Factory;
using static SeleniumExtras.WaitHelpers.ExpectedConditions;

namespace CheckoutBot.CheckoutBots.FootSites.ChampsSports
{
    public class ChampsSportsBot : FootSitesBotBase
    {
        private const string ApiUrl = "";
        public int DelayInSecond { private get; set; } = 2;

        public ChampsSportsBot() : base("ChampsSports", "https://www.champssports.com/", ApiUrl)
        {
        }


        public override void AccountCheckout(AccountCheckoutSettings settings, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public override void GuestCheckOut(GuestCheckoutSettings settings, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public override void Login(string username, string password, CancellationToken token)
        {
            Driver.Url = WebsiteBaseUrl;
            Task.Delay(DelayInSecond * 1000, token).Wait(token);
            Driver.EvalScript(GetScriptByXpath("//div[@id='header_login']/a") + ".click();");
            Driver.QueueScriptCall($"{GetScriptByXpath("//input[@id='login_email']")}.value=\"{username}\"");
            Driver.QueueScriptCall($"{GetScriptByXpath("//input[@id='login_password']")}.value=\"{password}\"");
            Driver.QueueScriptCall($"{GetScriptByXpath("//div[@id='header_login']/a//input[@id='login_submit']")}.click()");

            throw new NotImplementedException();
        }

        private IWebElement GetVisibleElementByXPath(WebDriverWait wait, string xPath, CancellationToken token)
        {
            var element = wait.Until(ElementIsVisible(By.XPath(xPath)));
            token.ThrowIfCancellationRequested();
            return element;
        }

        private IWebElement GetClickableElementByXPath(WebDriverWait wait, string xPath, CancellationToken token)
        {
            var element = wait.Until(ElementToBeClickable(By.XPath(xPath)));
            token.ThrowIfCancellationRequested();
            return element;
        }
    }
}
