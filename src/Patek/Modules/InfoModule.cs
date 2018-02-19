using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Discord;
using Discord.Commands;

namespace Patek.Modules
{
    [Name("Bot Information")]
    public class InfoModule : PatekModuleBase
    {
        public InfoModule(IOptions<Filter> filter)
            => Filter = filter.Value;

        [DontInject]
        public Filter Filter { get; set; }

        [Command("info")]
        [Alias("about", "whoami", "owner")]
        [Name("info")]
        [Summary("Get bot info")]
        public async Task InfoAsync()
        {
            var app = await Context.Client.GetApplicationInfoAsync();

            await ReplyAsync(
                $"Patek is a private-use Discord bot for Discord.Net's support channels.\n\n" +
                $"{Format.Bold("Info")}\n" +
                $"- Author: {app.Owner} ({app.Owner.Id})\n" +
                $"- Library: Discord.Net ({DiscordConfig.Version})\n" +
                $"- Runtime: {RuntimeInformation.FrameworkDescription} {RuntimeInformation.ProcessArchitecture} " +
                    $"({RuntimeInformation.OSDescription} {RuntimeInformation.OSArchitecture})\n" +
                $"- Uptime: {GetUptime()}\n\n" +
                
                $"{Format.Bold("Stats")}\n" +
                $"- Heap Size: {GetHeapSize()}MiB\n" +
                $"- Guilds: {Context.Client.Guilds.Count}\n" +
                $"- Channels: {Context.Client.Guilds.Sum(g => g.Channels.Count)}\n" +
                $"- Users: {Context.Client.Guilds.Sum(g => g.Users.Count)}\n");
        }

        [Command("debug")]
        [RequireElevatedActor]
        [Name("debug*")]
        [Summary("Get bot debug info")]
        public async Task DebugAsync()
        {
            var embed = new EmbedBuilder()
                .WithTitle("Debug")
                .AddField("Whitelisted Channels", 
                    string.Join(", ", Filter.Channels.Select(x => MentionUtils.MentionChannel(x))))
                .AddField("Elevated Users",
                    string.Join(", ", Filter.Users.Select(x => MentionUtils.MentionUser(x))))
                .Build();

            await ReplyAsync("", embed: embed);
        }

        private static string GetUptime() => (DateTime.Now - Process.GetCurrentProcess().StartTime).ToString(@"dd\.hh\:mm\:ss");
        private static string GetHeapSize() => Math.Round(GC.GetTotalMemory(true) / (1024.0 * 1024.0), 2).ToString();
    }
}
