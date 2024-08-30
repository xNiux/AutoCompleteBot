using AutoCompleteBot.Commands;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using System.Reflection;
using System.Diagnostics;

namespace AutoCompleteBot
{
    public class CommandHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly IServiceProvider _serviceProvider;
        private InteractionService _interactionService { get; set; }

        // Retrieve client and CommandService instance via vector
        public CommandHandler(DiscordSocketClient client, CommandService commands, IServiceProvider serviceProvider)
        {
            InteractionServiceConfig _interationServiceConfig = new InteractionServiceConfig();
            _interationServiceConfig.EnableAutocompleteHandlers = true;
            _interactionService = new(_client, config: _interationServiceConfig);
            _commands = commands;
            _client = client;
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Initialize the command handler
        /// </summary>
        /// <returns></returns>
        public async Task InstallCommandsAsync()
        {
            // Hook the MessageReceived event into our command handler
            _client.AutocompleteExecuted += AutocompleteExecuted;
            _client.SlashCommandExecuted += SlashCommandHandler;
            _client.InteractionCreated += InteractionCreated;

            // Here we discover all of the command modules in the entry 
            // assembly and load them. Starting from Discord.NET 2.0, a
            // service provider is required to be passed into the
            // module registration method to inject the 
            // required dependencies.
            //
            // If you do not use Dependency Injection, pass null.
            // See Dependency Injection guide for more information.
            await _commands.AddModulesAsync(assembly: Assembly.GetEntryAssembly(),
                                            services: _serviceProvider);
        }

        private async Task AutocompleteExecuted(SocketAutocompleteInteraction interaction)
        {
            var context = new SocketInteractionContext(_client, interaction);
            await _interactionService.ExecuteCommandAsync(context, services: _serviceProvider);
        }


        private async Task InteractionCreated(SocketInteraction interaction)
        {
            if (interaction.Type == InteractionType.ApplicationCommandAutocomplete)
            {
                var context = new SocketInteractionContext(_client, interaction);
                await _interactionService.ExecuteCommandAsync(context, services: _serviceProvider);
            }
        }

        /// <summary>
        /// Slash command handler
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        private async Task SlashCommandHandler(SocketSlashCommand command)
        {
            // TODO : Improve data options
            switch (command.Data.Name)
            {

                case CMD_Say.SLASH_CMD_ID:
                    {
                        CMD_Say cmd = new();
                        string echo = "Nothing to repeat" ;
                        var option = command.Data.Options.First().Value.ToString();

                        if (option != null)
                        {
                            echo = option;
                        }

                        await cmd.HandlerSlashCommand(command, echo);
                    }
                    break;

                default:
                    {
                        await command.RespondAsync($"You executed {command.Data.Name}");
                    }
                    break;
            }
        }
    }
}
