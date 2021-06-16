using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using CopebotNET.Commands;
using CoreRCON;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Extensions.Logging;
using Tomlyn;
using Tomlyn.Model;

namespace CopebotNET
{
    public class CopebotNet : IDisposable
    {
        private readonly DiscordActivity _activity;
        private readonly DiscordClient _client;
        private ConcurrentDictionary<string, RCON> _rconDictionary;

        private CopebotNet(CancellationTokenSource tokenSource) {
            var toml = File.ReadAllText("config/config.toml");
            var config = Toml.Parse(toml).ToModel();
            var loggerFactory = new SerilogLoggerFactory();


            static void RconAction() {
                Log.Information("Rcon disconnected");
            }
            _rconDictionary = new ConcurrentDictionary<string, RCON>();
            foreach (var serverTable in (TomlTableArray) ((TomlTable) config["rconConfig"])["servers"]) {
                var infoTable = (TomlTable) serverTable["info"];
                var rcon = new RCON(IPAddress.Parse((string) infoTable["ip"]), (ushort) (long) infoTable["port"],
                    (string) infoTable["password"]);
                rcon.OnDisconnected += RconAction;
                _rconDictionary.TryAdd((string) serverTable["name"], rcon);
            }

            var config1 = new DiscordConfiguration {
                Token = (string) ((TomlTable) config["botConfig"])["token"],
                Intents = (DiscordIntents) (long) ((TomlTable) config["botConfig"])["intents"],
                LoggerFactory = loggerFactory
            };

            var activityTable = (TomlTable) ((TomlTable) config["botConfig"])["activity"];
            _activity = new DiscordActivity((string) activityTable["name"],
                (ActivityType) (long) activityTable["type"]);

            var services = new ServiceCollection()
                .AddSingleton(tokenSource)
                .AddSingleton(_rconDictionary)
                .BuildServiceProvider();

            var commandsTable = (TomlTable) ((TomlTable) config["botConfig"])["commands"];
            var commandsConfig = new CommandsNextConfiguration {
                StringPrefixes = new List<string> {(string) commandsTable["prefix"]},
                EnableMentionPrefix = (bool) commandsTable["enableMention"],
                EnableDms = (bool) commandsTable["enableDms"],
                Services = services
            };

            var interactivityConfig = new InteractivityConfiguration {
                Timeout = TimeSpan.FromMinutes(1)
            };

            _client = new DiscordClient(config1);

            var commands = _client.UseCommandsNext(commandsConfig);

            commands.RegisterCommands<UngroupedCommands>();
            commands.RegisterCommands<SelfCommands>();
            
            var slashCommands = _client.UseSlashCommands();
            
            slashCommands.RegisterCommands<SlashCommands>(663309482956423178);

            _client.UseInteractivity(interactivityConfig);
        }

        public static bool Restart { get; set; }

        public void Dispose() {
            _client.DisconnectAsync().GetAwaiter().GetResult();
            foreach (var (name ,rcon) in _rconDictionary) {
                try {
                    rcon?.Dispose();
                }
                catch (Exception) {
                    Log.Error("Rcon {E} already disconnected", name);
                }
            }
            _rconDictionary.Clear();
            _rconDictionary = null;
            _client?.Dispose();
            GC.SuppressFinalize(this);
        }

        public static void Main() {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();

            do {
                var source = new CancellationTokenSource();
                var bot = new CopebotNet(source);

                bot.Run(source).GetAwaiter().GetResult();
                bot.Dispose();
                GC.Collect();
                if (Restart)
                    Log.Information("Bot restarting");
            } while (Restart);

            Log.Information("Exiting application, goodbye!");
        }

        private async Task Run(CancellationTokenSource source) {
            await _client.ConnectAsync(_activity);
            foreach (var (rconKey, rconValue) in _rconDictionary) {
                try {
                    await rconValue.ConnectAsync();
                    Log.Information("RCON for server {G} connected", rconKey);
                }
                catch (SocketException) {
                    Log.Error("Could not connect to {P}", rconKey);
                }
            }

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

        ~CopebotNet() {
            Dispose();
        }
    }
}