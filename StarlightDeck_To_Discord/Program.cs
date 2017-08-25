using System;
using System.Net;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Threading.Tasks;
using DSharpPlus;
using System.Reactive;
using NCrontab;

namespace StarlightDeck_To_Discord
{
    class Program
    {
        //args[0] = API Key
        //args[1] = Target Channel Number
        static async Task Main(string[] args)
        {
            var schedule = CrontabSchedule.Parse("30 0 */3 * * *", new CrontabSchedule.ParseOptions() { IncludingSeconds = true });
            var discordClient = new DiscordClient(args[0]);
            await discordClient.Connect(ulong.Parse(args[1]));
            System.Reactive.Linq.Observable.Generate(0, d => true, d => d + 1, d => d, d => new DateTimeOffset(schedule.GetNextOccurrence(DateTime.Now)))
                .Subscribe(async x =>
                {
                    string target = await StarlightAPIController.GetAPI();
                    await discordClient.SendMessageToTarget(target);
                });
            Console.WriteLine();
            await discordClient.Disconnect();
        }
    }
    public class StarlightAPIController
    {
        public static async Task<string> GetAPI()
        {
            using (var webClient = new WebClient())
            {
                string str = await webClient.DownloadStringTaskAsync("https://api.tachibana.cool/v1/starlight/event/1024/ranking_list.json");
                var ReceivedAPIData = JsonConvert.DeserializeObject<List<StartlightAPI_EventResponse>>(str);
                return ReceivedAPIData.Last().ToString();
            }
        }
        public class StartlightAPI_EventResponse
        {
            public DateTime Date { get; set; }
            public long Rank1 { get; set; }
            public long Rank2 { get; set; }
            public long Rank3 { get; set; }
            public long Reward1 { get; set; }
            public long Reward2 { get; set; }
            public long Reward3 { get; set; }
            public long Reward4 { get; set; }
            public long Reward5 { get; set; }
            public override string ToString()
            {
                return
                    $"{this.Date.ToString("yyyy/MM/dd H:mm:ss")}現在{Environment.NewLine}" +
                    $"1位：{this.Rank1}{Environment.NewLine}" +
                    $"2000位：{this.Reward1}{Environment.NewLine}" +
                    $"10000位：{this.Reward2}{Environment.NewLine}" +
                    $"20000位：{this.Reward3}{Environment.NewLine}" +
                    $"60000位：{this.Reward4}{Environment.NewLine}" +
                    $"120000位：{this.Reward5}{Environment.NewLine}";
            }
        }
    }

    class DiscordClient
    {
        private DSharpPlus.DiscordClient Client { get; set; }
        private DSharpPlus.DiscordChannel TargetChannel { get; set; }
        public DiscordClient(string token)
        {
            var cfg = new DiscordConfig
            {
                Token = token,
                TokenType = TokenType.Bot,

                AutoReconnect = true,
                LogLevel = LogLevel.Debug,
                UseInternalLogHandler = true
            };
            this.Client = new DSharpPlus.DiscordClient(cfg);
        }
        public async Task Connect(ulong targetChannelId)
        {
            this.Client.Ready += async e =>
            {
                await Task.Yield();
                this.Client.DebugLogger.LogMessage(LogLevel.Info, "DiscordClient", "Ready! Setting status message..", DateTime.Now);
                var game = new Game()
                {
                    Name = "魔王降誕の儀式を幻視中",
                    StreamType = 0
                };
                await this.Client.UpdateStatusAsync(game, UserStatus.Online);
                this.Client.DebugLogger.LogMessage(LogLevel.Info, "DiscordClient", "Discord Ready", DateTime.Now);
            };
            this.Client.DebugLogger.LogMessage(LogLevel.Info, "DiscordClient", "Connecting", DateTime.Now);
            await this.Client.ConnectAsync();
            this.TargetChannel = await this.Client.GetChannelAsync(targetChannelId);
        }
        public async Task Disconnect()
        {
            await this.Client.DisconnectAsync();
        }
        public async Task SendMessageToTarget(string content)
        {
            this.Client.DebugLogger.LogMessage(LogLevel.Info, "DiscordClient", "Sending Data", DateTime.Now);
            await this.TargetChannel.SendMessageAsync(content);
        }
    }
}