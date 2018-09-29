namespace StoreScraper.Bots.Shopify.ShopifyWebsites
{
    public class SvelteScraper : ShopifyScraper
    {
        public override string WebsiteName { get; set; } = "Svelte";
        public override string WebsiteBaseUrl { get; set; } = "https://svelte.co.uk/";
        public override bool Active { get; set; }
    }
}