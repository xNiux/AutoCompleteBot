using Discord.Commands;
using Discord.WebSocket;
using AutoCompleteBot.Configuration.Class;
using Discord;
using Discord.Net;
using Newtonsoft.Json;
using Discord.Interactions;

namespace AutoCompleteBot.Commands
{
    // Keep in mind your module **must** be public and inherit ModuleBase.
    // If it isn't, it will not be discovered by AddModulesAsync!
    public class CMD_Say : ModuleBase<SocketCommandContext>
    {
        public const string SLASH_CMD_ID = "say";
        private const string PARAMETER_CMD = "anything";



        /// <summary>
        /// Command initialization
        /// </summary>
        /// <param name="cmdList"></param>
        /// <param name="destination"></param>
        public static void Init(IReadOnlyCollection<SocketApplicationCommand> cmdList, object destination)
        {
            if (cmdList.Where(item => item.Name == SLASH_CMD_ID).Count() <= 0)
            {
                CreateSlashCommand(destination);
                Console.WriteLine($"Create a slashed command : {SLASH_CMD_ID}");
            }
            else
            {
                Console.WriteLine($"Found existing slashed command : {SLASH_CMD_ID}");
            }
        }




        /// <summary>
        /// Create slash command
        /// </summary>
        /// <param name="destination"></param>
        public static async void CreateSlashCommand(object destination)
        {
            // Next, lets create our slash command builder. This is like the embed builder but for slash commands.
            var slashedCommand = new SlashCommandBuilder();

            // Note: Names have to be all lowercase and match the regular expression ^[\w-]{3,32}$
            slashedCommand.WithName(SLASH_CMD_ID);

            // Descriptions can have a max length of 100.
            slashedCommand.WithDescription("Repeat what you want");

            // Add requested options
            slashedCommand.AddOption(PARAMETER_CMD, ApplicationCommandOptionType.String, "anything what you want I repeat", isAutocomplete: true, isRequired: true);

            try
            {
                // Now that we have our builder, we can call the CreateApplicationCommandAsync method to make our slash command.
                if (destination.GetType() == typeof(SocketGuild))
                {
                    var socketGuild = (SocketGuild)destination;
                    await socketGuild.CreateApplicationCommandAsync(slashedCommand.Build());
                    Console.WriteLine($"Slashed {SLASH_CMD_ID} command created on guild {socketGuild.Name} !");
                }
                else if (destination.GetType() == typeof(DiscordSocketClient))
                {
                    var socketClient = (DiscordSocketClient)destination;
                    await socketClient.CreateGlobalApplicationCommandAsync(slashedCommand.Build());
                    Console.WriteLine($"Slashed {SLASH_CMD_ID} global command created !");
                }
            }
            catch (HttpException exception)
            {
                // If our command was invalid, we should catch an ApplicationCommandException. This exception contains the path of the error as well as the error message. You can serialize the Error field in the exception to get a visual of where your error is.
                var json = JsonConvert.SerializeObject(exception.Errors, Formatting.Indented);

                // You can send this error somewhere or just print it to the console, for this example we're just going to print it.
                Console.WriteLine(json);
            }
        }




        /// <summary>
        /// Slash Command handler
        /// </summary>
        /// <param name="command"></param>
        /// <param name="parameterWithAutocompletion"></param>
        /// <returns></returns>
        public async Task HandlerSlashCommand(SocketSlashCommand command, [Discord.Commands.Summary("nom_du_paramètre"), Autocomplete(typeof(AutoCompleteHandlerGeneral))] string parameterWithAutocompletion)
        {
            var echo = parameterWithAutocompletion;

            await command.RespondAsync(echo);
        }




        /// <summary>
        /// Autocompletion suggestion generation
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public AutocompletionResult GenerateSuggestions(IReadOnlyCollection<AutocompleteOption> options)
        {
            // Create a collection with suggestions for autocomplete
            IEnumerable<AutocompleteResult> results = new[]
            {
            new AutocompleteResult("Name1", "value111"),
            new AutocompleteResult("Name2", "value2")
            };

            // max - 25 suggestions at a time (API limit)
            return AutocompletionResult.FromSuccess(results.Take(25));
        }
    }
}
