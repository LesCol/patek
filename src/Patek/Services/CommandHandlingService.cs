using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Discord;
using Discord.Addons.MicrosoftLogging;
using Discord.Commands;
using Discord.WebSocket;

namespace Patek.Services
{
    public class CommandHandlingService
    {
        private readonly CommandService _commands;
        private readonly Options _options;
        private readonly DiscordSocketClient _discord;
        private readonly Filter _filter;
        private IServiceProvider _provider;

        public CommandHandlingService(IServiceProvider provider,
            DiscordSocketClient discord,
            CommandService commands,
            IOptions<Filter> filter,
            IOptions<Options> options)
        {
            _provider = provider;

            _commands = commands;
            _discord = discord;
            _filter = filter.Value;
            _options = options.Value;

            var logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger("commands");
            _commands.Log += new LogAdapter(logger).Log;

            _discord.MessageReceived += MessageReceived;
        }

        public async Task InitializeAsync(IServiceProvider provider)
        {
            _provider = provider;
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly());
        }

        private async Task MessageReceived(SocketMessage rawMessage)
        {
            // Ignore system messages and messages from bots
            if (!(rawMessage is SocketUserMessage message)) return;
            if (message.Source != MessageSource.User) return;
            if (!_filter.IsWhitelisted(message.Channel)) return;

            int argPos = 0;
            if (!(message.HasMentionPrefix(_discord.CurrentUser, ref argPos) || message.HasStringPrefix(_options.Prefix, ref argPos))) return;

            var context = new SocketCommandContext(_discord, message);
            var result = await _commands.ExecuteAsync(context, argPos, _provider);

            if (result.Error.HasValue &&
                result.Error.Value != CommandError.UnknownCommand)
                await context.Channel.SendMessageAsync(result.ToString());
        }
    }
}
