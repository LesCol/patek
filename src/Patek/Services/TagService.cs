using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Discord;
using Discord.Commands;
using LiteDB;
using Patek.Data;

namespace Patek.Services
{
    public class TagService
    {
        private readonly CommandService _commands;
        private readonly LiteDatabase _database;
        private readonly ILogger _logger;

        // TODO: C#8 Nullable
        private Optional<ModuleInfo> module;

        public TagService(CommandService commands, LiteDatabase database, ILoggerFactory loggerFactory)
        {
            _commands = commands;
            _database = database;
            _logger = loggerFactory.CreateLogger("tags");

            module = new Optional<ModuleInfo>();
        }

        public async Task BuildTagsAsync()
        {
            if (module.IsSpecified)
                await _commands.RemoveModuleAsync(module.Value);

            var tags = _database.GetCollection<Tag>().FindAll();

            module = await _commands.CreateModuleAsync("", module =>
            {
                foreach (var tag in tags)
                {
                    module.AddCommand(tag.Name, (context, @params, provider, command) =>
                    {
                        return context.Channel.SendMessageAsync(
                            $"{tag.Name}: {tag.Content}");
                    },
                    command => {});
                }
            });

            _logger.LogInformation("Built {} tags succesfully.", tags.Count());
        }
    }
}
