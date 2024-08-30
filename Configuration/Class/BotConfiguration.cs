using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoCompleteBot.Configuration.Class
{
    public class BotConfiguration
    {
        public string Language {set; get;}
        public char Prefix {set; get;}
        public bool DevMode {set; get;}
        public string Discord_Link { set; get; } = string.Empty;

        private BotConfiguration() { Language = string.Empty;Prefix = '!';DevMode = false; }

        private static readonly Lazy<BotConfiguration> instance = new Lazy<BotConfiguration>(() => new BotConfiguration());
        public static BotConfiguration Instance { get { return instance.Value; } }
        public bool IsDevMode { get { return DevMode; } }
        public void InitializeBot(bool devMode, string discord_link)
        {
            // Get the Bot default configuration
            var botCfgTemp = JsonConvert.DeserializeObject<BotConfiguration>(File.ReadAllText("Configuration/botCfg.json"));

            if (botCfgTemp != null)
            {
                Language = botCfgTemp.Language;
                Prefix = botCfgTemp.Prefix;
            }
            else
            {
                Language = "en";
                Prefix = '!';
            }

            Discord_Link = discord_link;
            DevMode = devMode;
        }

    }
}
