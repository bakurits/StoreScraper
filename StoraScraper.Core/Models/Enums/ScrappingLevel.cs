using System;

namespace StoreScraper.Models.Enums
{
    [Flags]
    public enum ScrappingLevel
    {
        Name,
        Price,
        Url,
        Image,
        ReleaseTime,
        Keywords,
        PrimaryFields = Name | Price | Url | Image,
        NameAndUrl = Name | Url,
        Detailed = PrimaryFields | ReleaseTime | Keywords,
    }
}