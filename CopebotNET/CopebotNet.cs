using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CopebotNET.Commands;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Extensions.Logging;
using Tomlyn;
using Tomlyn.Model;

namespace CopebotNET
{
    public class CopebotNet : IDisposable
    {
        public static bool Restart { get; set; }
        
        private readonly DiscordActivity _activity;
        private readonly DiscordClient _client;

        public static void Main() {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();

            do {
                var source = new CancellationTokenSource();
                var bot = new CopebotNet(source);

                bot.Run(source).GetAwaiter().GetResult();
                bot.Dispose();
                if (Restart)
                    Log.Information("Bot restarting");
            } while (Restart);
            
            Log.Information("Exiting application, goodbye!");
        }

        private CopebotNet(CancellationTokenSource tokenSource) {
            var toml = File.ReadAllText("config/config.toml");
            var config = Toml.Parse(toml).ToModel();
            var loggerFactory = new SerilogLoggerFactory();

            var config1 = new DiscordConfiguration {
                Token = (string) ((TomlTable)config["botConfig"])["token"],
                Intents = (DiscordIntents)(long) ((TomlTable)config["botConfig"])["intents"],
                LoggerFactory = loggerFactory
            };
            
            var activityTable = (TomlTable) ((TomlTable) config["botConfig"])["activity"];
            _activity = new DiscordActivity((string) activityTable["name"], (ActivityType)(long) activityTable["type"]);

            var services = new ServiceCollection()
                .AddSingleton(tokenSource)
                .BuildServiceProvider();
            
            var commandsTable = (TomlTable) ((TomlTable) config["botConfig"])["commands"];
            var commandsConfig = new CommandsNextConfiguration {
                StringPrefixes = new List<string> {(string) commandsTable["prefix"]},
                EnableMentionPrefix = (bool) commandsTable["enableMention"],
                EnableDms = (bool) commandsTable["enableDms"],
                Services = services
            };

            _client = new DiscordClient(config1);

            var commands = _client.UseCommandsNext(commandsConfig);
            
            commands.RegisterCommands<UngroupedCommands>();
            commands.RegisterCommands<SelfCommands>();
        }

        private async Task Run(CancellationTokenSource source) {
            await _client.ConnectAsync(_activity);

            try {
                await Task.Delay(-1, source.Token);
            }
            catch (TaskCanceledException) {
                Log.Information("Bot task cancelled");
            }
            finally {
                source.Dispose();
            }
        }

        public void Dispose() {
            _client?.Dispose();
        }
    }
}