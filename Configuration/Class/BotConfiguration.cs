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
        public bool DevMode {set; get;}
        public string Discord_Link { set; get; } = string.Empty;
        public ulong Guild_Id {set;get;}

        private BotConfiguration() { DevMode = false; }

        private static readonly Lazy<BotConfiguration> instance = new Lazy<BotConfiguration>(() => new BotConfiguration());
        public static BotConfiguration Instance { get { return instance.Value; } }
        public bool IsDevMode { get { return DevMode; } }
        public void InitializeBot(bool devMode, string discord_link, ulong guild_id)
        {
            Discord_Link = discord_link;
            DevMode = devMode;
            Guild_Id = guild_id;
        }

    }
}
