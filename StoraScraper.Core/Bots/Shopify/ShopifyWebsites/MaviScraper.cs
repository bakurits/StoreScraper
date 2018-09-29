namespace StoreScraper.Bots.Shopify.ShopifyWebsites
{
    public class MaviScraper : ShopifyScraper
    {
        public override string WebsiteName { get; set; } = "Mavi";
        public override string WebsiteBaseUrl { get; set; } = "https://us.mavi.com/";
        public override bool Active { get; set; }
    }
}