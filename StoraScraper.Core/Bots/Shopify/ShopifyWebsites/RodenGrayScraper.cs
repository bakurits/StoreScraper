namespace StoreScraper.Bots.Shopify.ShopifyWebsites
{
    public class RodenGrayScraper : ShopifyScraper
    {
        public override string WebsiteName { get; set; } = "Roden Gray";
        public override string WebsiteBaseUrl { get; set; } = "https://rodengray.com/";
        public override bool Active { get; set; }
    }
}