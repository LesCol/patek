using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Discord;
using Discord.Addons.MicrosoftLogging;
using Discord.Commands;
using Discord.WebSocket;
using LiteDB;
using Patek.Services;

namespace Patek
{
    class Program
    {
        static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();

        private DiscordSocketClient _client;
        private IConfiguration _config;

        public async Task MainAsync()
        {
            _client = new DiscordSocketClient();
            _config = BuildConfig();

            var services = ConfigureServices();
            await services.GetRequiredService<CommandHandlingService>().InitializeAsync(services);
            await services.GetRequiredService<TagService>().BuildTagsAsync();

            var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("discord");
            _client.UseMicrosoftLogging(logger);

            var options = services.GetRequiredService<IOptions<Options>>().Value;

            await _client.LoginAsync(TokenType.Bot, options.Token);
            await _client.StartAsync();

            await Task.Delay(-1);
        }

        private IServiceProvider ConfigureServices()
        {
            return new ServiceCollection()
                // Base
                .AddSingleton(_client)
                .AddSingleton<CommandService>()
                .AddSingleton<CommandHandlingService>()
                // Logging
                .AddLogging(x => x.AddConsole())
                // Configuration
                .AddSingleton(_config)
                .Configure<Options>(_config)
                .Configure<Filter>(_config)
                // Extra
                .AddSingleton(new LiteDatabase($"{GetConfigurationRoot()}/patek.db"))
                .AddSingleton<TagService>()
                .BuildServiceProvider();
        }

        private IConfiguration BuildConfig()
        {
            return new ConfigurationBuilder()
                .SetBasePath(GetConfigurationRoot())
                .AddJsonFile("config.json")
                .AddJsonFile("filter.json", true)
                .Build();
        }

        private string GetConfigurationRoot()
        {
            var cwd = Directory.GetCurrentDirectory();
            var sln = Directory.GetFiles(cwd).Any(f => f.Contains(".sln"));
            return sln ? cwd : Path.Combine(cwd, "..", "..");
        }
    }
}