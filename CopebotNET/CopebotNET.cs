using System;
using System.IO;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using Hjson;
using Newtonsoft.Json;

namespace CopebotNET
{
    class CopebotNET
    {
        public DiscordConfiguration Config { get; set; }
        private DiscordClient Client { get; set; }
        
        private DiscordActivity Activity { get; set; }
        
        public static void Main() {
            var bot = new CopebotNET();

            bot.run().GetAwaiter().GetResult();
        }

        public CopebotNET() {
            var configHjson = HjsonValue.Load("config/botConfig.hjson").Qo();

            Config = new DiscordConfiguration {
                Token = configHjson.Qs("token"),
                Intents = DiscordIntents.AllUnprivileged | DiscordIntents.AllUnprivileged
            };

            Activity = new DiscordActivity("hmm, the bot", ActivityType.ListeningTo);
            
            Client = new DiscordClient(Config);
        }

        public async Task run() {
            await Client.ConnectAsync(Activity);

            await Task.Delay(-1);
        }
    }
}