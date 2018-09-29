namespace StoreScraper.Bots.Shopify.ShopifyWebsites
{
    public class HebeBoutiqueScraper : ShopifyScraper
    {
        public override string WebsiteName { get; set; } = "Hebe Boutique";
        public override string WebsiteBaseUrl { get; set; } = "https://hebeboutique.com/";
        public override bool Active { get; set; }
    }
}