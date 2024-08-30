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

        private char Prefix;

        // Retrieve client and CommandService instance via vector
        public CommandHandler(DiscordSocketClient client, CommandService commands, IServiceProvider serviceProvider, char prefix)
        {
            InteractionServiceConfig _interationServiceConfig = new InteractionServiceConfig();
            _interationServiceConfig.EnableAutocompleteHandlers = true;
            _interactionService = new(_client, config: _interationServiceConfig);
            _commands = commands;
            _client = client;
            _serviceProvider = serviceProvider;
            Prefix = prefix;
        }

        /// <summary>
        /// Initialize the command handler
        /// </summary>
        /// <returns></returns>
        public async Task InstallCommandsAsync()
        {
            // Hook the MessageReceived event into our command handler
            _client.MessageReceived += HandleCommandAsync;
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
        /// Handle the command execution
        /// </summary>
        /// <param name="messageParam"></param>
        /// <returns></returns>
        private async Task HandleCommandAsync(SocketMessage messageParam)
        {
            // Don't process the command if it was a system message
            var message = messageParam as SocketUserMessage;
            if (message == null) return;

            // Create a number to track where the prefix ends and the command begins
            int argPos = 0;

            // Determine if the message is a command based on the prefix and make sure no bots trigger commands
            if (!(message.HasCharPrefix(Prefix, ref argPos) ||
                message.HasMentionPrefix(_client.CurrentUser, ref argPos)) ||
                message.Author.IsBot)
                return;

            // Create a WebSocket-based command context based on the message
            var context = new SocketCommandContext(_client, message);

            // Execute the command with the command context we just
            // created, along with the service provider for precondition checks.
            await _commands.ExecuteAsync(
                context: context,
                argPos: argPos,
                services: _serviceProvider);
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
