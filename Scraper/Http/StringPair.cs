using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoreScraper.Http
{
    public class StringPair
    {
        public string Name { get; set; }
        public string Value { get; set; }

        public static implicit operator StringPair((string name, string value) pair)
        {
            return new StringPair()
            {
               Name = pair.name,
               Value = pair.value,
            };
        }

        public static implicit operator (string name, string value) (StringPair header)
        {
            return (header.Name, header.Value);
        }
    }
}
