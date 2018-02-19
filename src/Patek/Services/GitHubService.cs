using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Discord.WebSocket;

namespace Patek.Services
{
    public class GitHubService
    {
        private const string RepositoryFormat = "#{0}: https://github.com/RogueException/Discord.Net/issues/{0}";

        private readonly Filter _filter;
        private readonly Regex _regex = new Regex(@"##([0-9]+)");

        public GitHubService(DiscordSocketClient client, IOptions<Filter> filter)
        {
            _filter = filter.Value;

            client.MessageReceived += MessageReceivedAsync;
        }

        private async Task MessageReceivedAsync(SocketMessage msg)
        {
            if (!_filter.IsWhitelisted(msg.Channel)) return;

            var matches = _regex.Matches(msg.Content);
            if (matches.Count == 0) return;

            StringBuilder content = new StringBuilder();
            foreach (Match match in matches)
                content.AppendLine(string.Format(RepositoryFormat, match.Value.Substring(2)));

            await msg.Channel.SendMessageAsync(content.ToString());
        }
    }
}
