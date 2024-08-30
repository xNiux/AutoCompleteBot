using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using AutoCompleteBot;
using AutoCompleteBot.Commands;
using AutoCompleteBot.Configuration.Class;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Resources;
using static System.Net.Mime.MediaTypeNames;

public class Program
{
    /// <summary>
    /// Class for the main discord configuration
    /// </summary>
    private class ConfigurationClass
    {
        public string discord_link = string.Empty;
        public string token = string.Empty;
        public ulong guild_id = 0;
        public string discord_link_beta = string.Empty;
        public string token_beta = string.Empty;
        public ulong guild_id_beta = 0;

        public ConfigurationClass()
        {
            discord_link = string.Empty;
            token = string.Empty;
            token_beta = string.Empty;
            discord_link = string.Empty;
            guild_id = 0;
        }
    }

    private readonly DiscordSocketClient _client = new(
        new DiscordSocketConfig
        {
            // AlwaysDownloadUsers = true,
            // GatewayIntents = GatewayIntents.MessageContent |
            //                  GatewayIntents.DirectMessages |
            //                  GatewayIntents.GuildMembers |
            //                  GatewayIntents.GuildMessages |
            //                  GatewayIntents.GuildIntegrations |
            //                  GatewayIntents.Guilds
        });

    private readonly CommandService _commandService = new CommandService();
    private IServiceProvider _serviceProvider = CreateServiceProvider();
    private ConfigurationClass discordCfg = new();
    public static Task Main(string[] args) => new Program().MainAsync();


    /// <summary>
    /// Main task
    /// </summary>
    /// <returns></returns>
    public async Task MainAsync()
    {
        // Initialize Bot
        string token = InitializeBot();

        // Connect to discord
        _client.Log += DiscordLog;
        _client.Ready += Client_Ready;

        if (token != string.Empty)
        {
            // Start Discord link
            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            // Start Command Handler
            var commandHandler = new CommandHandler(_client, _commandService, _serviceProvider);
            await commandHandler.InstallCommandsAsync();

            // Block this task until the program is closed.
            await Task.Delay(-1);
        }
        else
        {
            Console.WriteLine("Can't find discordCfg.json ...");
        }
    }


    /// <summary>
    /// Initialize bot with json files
    /// </summary>
    /// <returns></returns>
    private string InitializeBot()
    {
        // Load JSON configuration
        var temp = JsonConvert.DeserializeObject<ConfigurationClass>(File.ReadAllText("./Configuration/discordCfg.json"));

        if (temp != null)
        {
            string token;

            // Get the Discord configuration
            discordCfg = temp;

            // Beta Bot version
            if (IsDevMode())
            {
                token = discordCfg.token_beta;
                BotConfiguration.Instance.DevMode = true;
                BotConfiguration.Instance.Discord_Link = discordCfg.discord_link_beta;
                BotConfiguration.Instance.Guild_Id = discordCfg.guild_id_beta;
                Console.WriteLine("==> Start in beta mode\n");
            }
            // Production Bot version
            else
            {
                token = discordCfg.token;
                BotConfiguration.Instance.DevMode = false;
                BotConfiguration.Instance.Discord_Link = discordCfg.discord_link;
                BotConfiguration.Instance.Guild_Id = discordCfg.guild_id;
                Console.WriteLine("==> Start in production mode\n");
            }

            return token;
        }
        else
        {
            Console.WriteLine("Can't find discordCfg.json ...");
            return string.Empty;
        }
    }

    /// <summary>
    /// Return the developper mode status
    /// </summary>
    /// <returns></returns>
    public bool IsDevMode()
    {
        // Set the environment
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

        if (env == "Production")
        {
            return false;
        }
        else
        {
            return true;
        }
    }



    private async Task GetSlashCommandsAsync(SocketGuild? guild, List<string> commands)
    {
        IReadOnlyCollection<SocketApplicationCommand> applicationCommands;

        if (guild == null)
        {
            applicationCommands = await _client.GetGlobalApplicationCommandsAsync();

        }
        else
        {
            applicationCommands = await guild.GetApplicationCommandsAsync();
        }

        Console.WriteLine("\n");

        foreach (var applicationCommand in applicationCommands)
        {
            if (applicationCommand.Type == ApplicationCommandType.Slash || applicationCommand.Type == ApplicationCommandType.User)
            {
                commands.Add(applicationCommand.Name);
                Console.WriteLine($"Found existing command : Name: {applicationCommand.Name}, Description: {applicationCommand.Description}");
            }
        }
    }

    /// <summary>
    /// Initialize commands
    /// </summary>
    private async Task InitializeCommands()

    {
        object destination;
        IReadOnlyCollection<SocketApplicationCommand> applicationCommands;

        // In dev mode, commands are only created on Guild
        if (IsDevMode() == true)
        {
            var guild = _client.GetGuild(BotConfiguration.Instance.Guild_Id);

            destination = guild;
            applicationCommands = await guild.GetApplicationCommandsAsync();
        }
        // Global commands on production version
        else
        {
            destination = _client;
            applicationCommands = await _client.GetGlobalApplicationCommandsAsync();

        }


        CMD_Say.Init(applicationCommands, destination);
    }



    private static IServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();

        // Ajoutez vos services ici
        // Par exemple :
        services.AddSingleton<DiscordSocketClient>();
        services.AddSingleton<CommandService>();

        return services.BuildServiceProvider();
    }


    #region Handler


    /// <summary>
    /// Called when bot is started and ready
    /// </summary>
    /// <returns></returns>
    private async Task Client_Ready()
    {
        Console.WriteLine("Bot is now ready !");
        await InitializeCommands();
    }


    /// <summary>
    /// Task to write Discord log message on console
    /// </summary>
    /// <param name="msg"></param>
    /// <returns></returns>
    private Task DiscordLog(LogMessage msg)
    {
        Console.WriteLine(msg.ToString());
        return Task.CompletedTask;
    }

    #endregion
}

