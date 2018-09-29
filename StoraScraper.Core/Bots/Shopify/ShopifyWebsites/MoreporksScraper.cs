namespace StoreScraper.Bots.Shopify.ShopifyWebsites
{
    public class MoreporksScraper : ShopifyScraper
    {
        public override string WebsiteName { get; set; } = "Moreporks";
        public override string WebsiteBaseUrl { get; set; } = "https://moreporks.com/";
        public override bool Active { get; set; }
    }
}