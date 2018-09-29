namespace StoreScraper.Bots.Shopify.ShopifyWebsites
{
    public class DropDeadScraper : ShopifyScraper
    {
        public override string WebsiteName { get; set; } = "Dropdead";
        public override string WebsiteBaseUrl { get; set; } = "https://www.dropdead.co/";
        public override bool Active { get; set; }
    }
}