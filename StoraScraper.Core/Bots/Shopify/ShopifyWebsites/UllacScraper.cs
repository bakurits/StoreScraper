namespace StoreScraper.Bots.Shopify.ShopifyWebsites
{
    public class UllacScraper : ShopifyScraper
    {
        public override string WebsiteName { get; set; } = "Ullac Shop";
        public override string WebsiteBaseUrl { get; set; } = "https://ullac.com/";
        public override bool Active { get; set; }
    }
}