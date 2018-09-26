using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Newtonsoft.Json;
using StoreScraper.Core;
using StoreScraper.Helpers;
using StoreScraper.Interfaces;

namespace StoreScraper.Models
{
    [JsonObject]
    public class WebHook
    {
        [JsonIgnore]
        private static readonly Uri _slackHookHost = new Uri("https://hooks.slack.com");

        [JsonIgnore]
        private static readonly Uri _discordHookHost = new Uri("https://discordapp.com");

        [JsonIgnore]
        private static readonly SlackPoster _slackPoster = new SlackPoster();

        [JsonIgnore]
        private static readonly DiscordPoster _discordPoster = new DiscordPoster();

        [Browsable(false)]
        [XmlIgnore]
        [JsonIgnore]
        public IWebHookPoster Poster { get; set; }

        private string _webHookUrl = "";
        public string WebHookUrl { get => _webHookUrl;
            set
            {
                Uri parsed = new Uri(value);    
                if (Uri.Compare(parsed, _slackHookHost, UriComponents.Host, UriFormat.Unescaped, StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    Poster = _slackPoster;
                    _webHookUrl = value;
                }
                else if(Uri.Compare(parsed, _discordHookHost, UriComponents.Host, UriFormat.Unescaped, StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    Poster = _discordPoster;
                    _webHookUrl = value;
                }
                else
                {
                    throw new NotSupportedException("This kinf of webhook is not supported");
                }
            }
        }


        public override string ToString()
        {
            return this._webHookUrl;
        }
    }
}
