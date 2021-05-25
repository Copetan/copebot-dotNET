using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace CopebotNET.Commands
{
    [Group("self")]
    [Aliases("bot", "Copebot")]
    [Description("Bot administration commands. Can only be executed by an owner of the bot!")]
    [RequireOwner]
    public class SelfCommands : BaseCommandModule
    {
        private readonly CancellationTokenSource _source;

        public SelfCommands(CancellationTokenSource source) {
            _source = source;
        }

        [Command("shutdown")]
        [Description("Shuts down the bot")]
        public async Task Shutdown(CommandContext context) {
            await context.RespondAsync("Shutting down bot. Goodbye!");
            await context.Client.DisconnectAsync();
            CopebotNet.Restart = false;
            _source.Cancel();
        }

        [Command("restart")]
        [Description("Restarts the bot")]
        public async Task Restart(CommandContext context) {
            await context.RespondAsync("Restarting bot. See you later!");
            await context.Client.DisconnectAsync();
            CopebotNet.Restart = true;
            _source.Cancel();
        }
    }
}