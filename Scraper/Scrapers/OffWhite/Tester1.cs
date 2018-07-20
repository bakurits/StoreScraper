using System.Net.Http;
using StoreScraper.Models;

/*
 * New tester class for making debugging OffWhiteScraper class
 * far more easier.
 */

namespace StoreScraper.Bots.OffWhite
{
    class Tester1
    {
        public ScraperBase offWhiteBot = AppSettings.Default.AvaibleBots[0];

        public void TestCheckOutForm()
        {
            CheckOutForm.Data d = new CheckOutForm.Data()
            {
                PMethod = "patch",
                AuthenticityToken = "",
                Email = "Dzagluka@gmail.com",
                StateLockVersion = "2",
                FirstName = "Dzliera",
                LastName = "Chkhikvadze",
                Address1 = "18 Boulden CIR STE 2",
                Address2 = "CAMARATC LLC C39598",
                City = "New Castle",
                CountryID = "49",
                BillingStateID = "103",
                ZipCode = "19720-3494",
                Phone = "3025445534",
                FiscalCode = "",
                BillingID = "845840",
                UseBilling = "1",
                ShippingStateID = "",
                ShippingID = "845841",
                TermsAndConditions = "yes",
                Commit = "Save and Continue"
            };
            CheckOutForm form = new CheckOutForm(d);
            HttpClient client = new HttpClient();
            HttpContent cont = new FormUrlEncodedContent(form.RequestData);
            var task = client.PostAsync(@"https://www.off---white.com/en/US/checkout/update/address", cont);
            task.Wait();

            var result = task.Result;
        }


        public const string url =
            "https://discordapp.com/api/webhooks/468240680414609429/kKJB9L4I8AfQWWDcqf0vpAj9OYDqxLAJ9gHl1b2B5xg8c5X2Ic4FpcSHAE8_0vKqZBoP";

    }
}
