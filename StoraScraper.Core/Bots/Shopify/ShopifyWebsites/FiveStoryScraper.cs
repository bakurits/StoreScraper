namespace StoreScraper.Bots.Shopify.ShopifyWebsites
{
    public class FiveStoryScraper : ShopifyScraper
    {
        public override string WebsiteName { get; set; } = "FiveStory New York";
        public override string WebsiteBaseUrl { get; set; } = "https://fivestoryny.com/";
        public override bool Active { get; set; }
    }
}