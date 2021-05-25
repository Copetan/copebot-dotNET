using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace CopebotNET.Commands
{
    public class UngroupedCommands : BaseCommandModule
    {
        [Command("test")]
        [Description("A test command")]
        public async Task Test(CommandContext context) {
            await context.TriggerTypingAsync();
            await context.RespondAsync("hello");
        }
    }
}