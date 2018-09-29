namespace StoreScraper.Bots.Shopify.ShopifyWebsites
{
    public class AclerScraper : ShopifyScraper
    {
        public override string WebsiteName { get; set; } = "Acler";
        public override string WebsiteBaseUrl { get; set; } = "https://shopacler.com/";
        public override bool Active { get; set; }
    }
}