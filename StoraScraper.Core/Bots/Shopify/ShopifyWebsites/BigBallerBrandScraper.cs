namespace StoreScraper.Bots.Shopify.ShopifyWebsites
{
    public class BigBallerBrandScraper : ShopifyScraper
    {
        public override string WebsiteName { get; set; } = "Big Baller Brand";
        public override string WebsiteBaseUrl { get; set; } = "https://bigballerbrand.com/";
        public override bool Active { get; set; }
    }
}