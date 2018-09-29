namespace StoreScraper.Bots.Shopify.ShopifyWebsites
{
    public class ThreeSixteenScraper : ShopifyScraper
    {
        public override string WebsiteName { get; set; } = "3Sixteen";
        public override string WebsiteBaseUrl { get; set; } = "https://www.3sixteen.com/";
        public override bool Active { get; set; }
    }
}