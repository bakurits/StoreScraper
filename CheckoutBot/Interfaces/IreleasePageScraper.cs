﻿using System.Collections.Generic;
using System.Threading;
using StoreScraper.Models;

namespace CheckoutBot.Interfaces
{
    public interface IreleasePageScraper
    {
        List<Product> ScrapeReleasePage(CancellationToken token);
    }
}