using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using AutoCompleteBot.Commands;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoCompleteBot
{
    internal class AutoCompleteHandlerGeneral : AutocompleteHandler
    {
        public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
        {
            AutocompletionResult options = new AutocompletionResult();
            var cmdData = (SocketAutocompleteInteractionData)context.Interaction.Data;

            switch (cmdData.CommandName)
            {
                case CMD_Say.SLASH_CMD_ID:
                    {
                        CMD_Say cmd = new CMD_Say();
                        options = cmd.GenerateSuggestions(cmdData.Options);
                    }
                    break;

                default:
                    {
                        Debug.WriteLine($"You executed {cmdData.CommandName}");
                    }
                    break;
            }

            return options;
        }
    }

}
