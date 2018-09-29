namespace StoreScraper.Bots.Shopify.ShopifyWebsites
{
    public class HobesScraper : ShopifyScraper
    {
        public override string WebsiteName { get; set; } = "Hobes";
        public override string WebsiteBaseUrl { get; set; } = "https://hobes.co/";
        public override bool Active { get; set; }
    }
}