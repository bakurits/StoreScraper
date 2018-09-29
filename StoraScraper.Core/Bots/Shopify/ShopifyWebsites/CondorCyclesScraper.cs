namespace StoreScraper.Bots.Shopify.ShopifyWebsites
{
    public class CondorCyclesScraper : ShopifyScraper
    {
        public override string WebsiteName { get; set; } = "Condor Cycles";
        public override string WebsiteBaseUrl { get; set; } = "https://www.condorcycles.com/";
        public override bool Active { get; set; }
    }
}