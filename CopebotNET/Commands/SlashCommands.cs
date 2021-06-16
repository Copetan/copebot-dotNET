using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace CopebotNET.Commands
{
    public class SlashCommands : SlashCommandModule
    {
        [SlashCommand("epic", "A slash command that does a cool")]
        public async Task SlashTest(InteractionContext context) {
            await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().WithContent("you are cool"));
        }
    }
}