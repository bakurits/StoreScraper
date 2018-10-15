namespace StoreScraper.Models
{
    public class StringPair
    {
        public string Key { get; set; }
        public string Value { get; set; }

        public static implicit operator (string key, string value)(StringPair pair)
        {
            return (pair.Key, pair.Value);
        }

        

        public static implicit operator StringPair ((string key, string value) tuple) => new StringPair()
        {
            Key = tuple.key,
            Value = tuple.value
        };

        public override string ToString()
        {
            return this.Key + " " + this.Value;
        }
    }
}
