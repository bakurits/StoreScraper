namespace StoreScraper.Bots.Shopify.ShopifyWebsites
{
    public class HickiesScraper : ShopifyScraper
    {
        public override string WebsiteName { get; set; } = "Hickies";
        public override string WebsiteBaseUrl { get; set; } = "https://www.hickies.com/";
        public override bool Active { get; set; }
    }
}