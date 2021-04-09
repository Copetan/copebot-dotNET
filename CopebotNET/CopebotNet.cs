using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using Tomlyn;
using Tomlyn.Model;

namespace CopebotNET
{
    class CopebotNet
    {
        private DiscordConfiguration Config { get; set; }
        
        private DiscordActivity Activity { get; set; }
        
        private CommandsNextConfiguration CommandsConfig { get; set; }
        
        private DiscordClient Client { get; set; }
        
        private CommandsNextExtension Commands { get; set; }

        public static void Main() {
            var bot = new CopebotNet();

            bot.Run().GetAwaiter().GetResult();
        }

        public CopebotNet() {
            var toml = File.ReadAllText("config/config.toml");
            var config = Toml.Parse(toml).ToModel();

            Config = new DiscordConfiguration {
                Token = (string) ((TomlTable)config["botConfig"])["token"],
                Intents = DiscordIntents.AllUnprivileged | DiscordIntents.AllUnprivileged
            };
            
            var activityTable = (TomlTable) ((TomlTable) config["botConfig"])["activity"];
            Activity = new DiscordActivity((string) activityTable["name"], (ActivityType)(long) activityTable["type"]);
            
            var commandsTable = (TomlTable) ((TomlTable) config["botConfig"])["commands"];
            CommandsConfig = new CommandsNextConfiguration {
                StringPrefixes = new List<string> {(string) commandsTable["prefix"]},
                EnableMentionPrefix = (bool) commandsTable["enableMention"],
                EnableDms = (bool) commandsTable["enableDms"]
            };

            Client = new DiscordClient(Config);

            Commands = Client.UseCommandsNext(CommandsConfig);
        }

        public async Task Run() {
            await Client.ConnectAsync(Activity);

            await Task.Delay(-1);
        }
    }
}