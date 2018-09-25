using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoreScraper.Exceptions
{
    public class ScrappingException : Exception
    {
        public ScrappingExcetionType Type { get; set; }

        public ScrappingException(ScrappingExcetionType type)
        {
            this.Type = type;
        }
    }

    [Flags]
    public enum ScrappingExcetionType
    {
        ProxyDetected,
        PriceParsing,
        NameParsing,
        UrlParsing,
        SizeListParsing,
        ImageUrlParsing,
        Unknown
    }
}
