using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoreScraper.Models
{
    public class StringPair
    {
        public string Key { get; set; }
        public string Value { get; set; }

        public static implicit operator (string key, string value)(StringPair pair) => (pair.Key, pair.Value);

        public static implicit operator StringPair ((string key, string value) tuple) => new StringPair()
        {
            Key = tuple.key,
            Value = tuple.value
        };
    }
}
