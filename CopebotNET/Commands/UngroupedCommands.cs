using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using CoreRCON;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace CopebotNET.Commands
{
    public class UngroupedCommands : BaseCommandModule
    {
        private readonly ConcurrentDictionary<string, RCON> _rconDictionary;

        public UngroupedCommands(ConcurrentDictionary<string, RCON> rconDictionary) {
            _rconDictionary = rconDictionary;
        }
        
        [Command("test")]
        [Description("A test command")]
        public async Task Test(CommandContext context) {
            await context.TriggerTypingAsync();
            await context.RespondAsync("hello");
        }

        [Command("rcon")]
        [Description("An rcon test")]
        public async Task Rcon(CommandContext context) {
            await context.TriggerTypingAsync();

            foreach (var rconPair in _rconDictionary) {
                await rconPair.Value.SendCommandAsync("say test");
                await context.RespondAsync($"command sent to the {rconPair.Key} server");
            }
        }

        [Command("execute")]
        [Description("Execute a command on server")]
        public async Task Execute(CommandContext context, string server, [RemainingText] string commands) {
            try {
                var test = _rconDictionary[server].SendCommandAsync(commands);
                if (test.Wait(TimeSpan.FromSeconds(10))) {
                    await context.RespondAsync($"```{await test}```");
                }
                else {
                    test.Dispose();
                    await context.RespondAsync("Server connection timed out");
                }
            }
            catch (KeyNotFoundException) {
                await context.RespondAsync("Server does not exist! Please try another name");
            }
        }

        [Command("button")]
        [Description("Press!!!!")]
        public async Task Button(CommandContext context) {
            var builder = new DiscordMessageBuilder();
            var button = new DiscordButtonComponent(ButtonStyle.Success, "epic", "awesome");
            builder.WithContent("lol").AddComponents(button);
            await builder.SendAsync(context.Channel);
        }
    }
}