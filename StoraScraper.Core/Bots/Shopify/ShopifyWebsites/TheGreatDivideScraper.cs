namespace StoreScraper.Bots.Shopify.ShopifyWebsites
{
    public class TheGreatDivideScraper : ShopifyScraper
    {
        public override string WebsiteName { get; set; } = "The Great Divide";
        public override string WebsiteBaseUrl { get; set; } = "https://thegreat-divide.com/";
        public override bool Active { get; set; }
    }
}