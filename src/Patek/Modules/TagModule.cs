using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Discord;
using Discord.Commands;
using Patek.Data;
using Patek.Services;
using LiteDB;

namespace Patek.Modules
{
    [Name("Tag Management")]
    public class TagModule : PatekModuleBase
    {
        public LiteDatabase Database { get; set; }
        public TagService TagService { get; set; }

        private readonly Options _options;

        public TagModule(IOptions<Options> options)
        {
            _options = options.Value;
        }

        [Command("tag create")]
        [RequireElevatedActor]
        [Name("tag create* <name> <text>")]
        [Summary("Add a tag")]
        public async Task CreateAsync(string name, [Remainder] string content)
        {
            var tag = new Tag(name, content, Context.User);
            Database.GetCollection<Tag>().Insert(tag);
            await TagService.BuildTagsAsync();
            await ReactAsync(Pass);
        }

        [Command("tag set name")]
        [Alias("tag setname", "tag update name", "tag name")]
        [RequireElevatedActor]
        [Name("tag name* <name> <new>")]
        [Summary("Change a tag's name")]
        public async Task SetNameAsync(string before, string after)
        {
            var tags = Database.GetCollection<Tag>();
            var tag = tags.FindOne(x => x.Name == before);
            if (tag == null)
            {
                await ReactAsync(TagNotFound);
                return;
            }

            tag.Name = after;
            tag.ActorId = Context.User.Id;
            tag.UpdatedAt = DateTimeOffset.Now;

            tags.Update(tag);
            await TagService.BuildTagsAsync();
            await ReactAsync(Pass);
        }

        [Command("tag set content")]
        [Alias("tag set text", "tag update text", "tag text", "tag content")]
        [RequireElevatedActor]
        [Name("tag text* <name> <text>")]
        [Summary("Change a tag's content")]
        public async Task SetContentAsync(string name, [Remainder] string content)
        {
            var tags = Database.GetCollection<Tag>();
            var tag = tags.FindOne(x => x.Name == name);
            if (tag == null)
            {
                await ReactAsync(TagNotFound);
                return;
            }

            tag.Content = content;
            tag.ActorId = Context.User.Id;
            tag.UpdatedAt = DateTimeOffset.Now;

            tags.Update(tag);
            await TagService.BuildTagsAsync();
            await ReactAsync(Pass);
        }

        [Command("tag get content")]
        [Alias("tag raw", "tag content")]
        [Name("tag raw <name>")]
        [Summary("Get a tag's raw content")]
        public async Task GetRawContentAsync(string name)
        {
            var tags = Database.GetCollection<Tag>();
            var tag = tags.FindOne(x => x.Name == name);
            if (tag == null)
            {
                await ReactAsync(TagNotFound);
                return;
            }

            await ReplyAsync(Format.Sanitize(tag.Content));
        }

        [Command("tag delete")]
        [RequireElevatedActor]
        [Name("tag delete* <name>")]
        [Summary("Pretend to delete a tag")]
        public Task DeleteAsync([Remainder] string name)
            => ReplyAsync($"Are you sure you want to do this? If so, use {_options.Prefix}tag destroy {name}");

        [Command("tag destroy")]
        [RequireElevatedActor]
        [Name("tag destroy* <name>")]
        [Summary("Actually delete a tag")]
        public async Task DestroyAsync(string name)
        {
            var tags = Database.GetCollection<Tag>();
            var tag = tags.FindOne(t => t.Name == name);
            if (tag == null)
            {
                await ReactAsync(TagNotFound);
                return;
            }
            tags.Delete(tag.Id);
            await TagService.BuildTagsAsync();
            await ReactAsync(Removed);
        }

        [Command("tag list")]
        [Alias("tags")]
        [Name("tags")]
        [Summary("List the tags")]
        public async Task ListAsync()
        {
            var tags = Database.GetCollection<Tag>().FindAll();
            await ReplyAsync($"Tags: {string.Join(", ", tags.Select(t => t.Name))}");
        }

        [Command("tag info")]
        [Alias("tag owner", "tag whois", "tag about")]
        [Name("tag info name")]
        [Summary("Get info about a tag")]
        public async Task InfoAsync(string name)
        {
            var tag = Database.GetCollection<Tag>().FindOne(t => t.Name == name);
            if (tag == null)
            {
                await ReactAsync(TagNotFound);
                return;
            }
            var modifiedBy = tag.ActorId.HasValue ? $"{Context.Guild.GetUser(tag.ActorId.Value)?.ToString() ?? "<user unknown>"}" : "<not modified>";
            await ReplyAsync(
                $"{Format.Bold("Tag Info")}\n" +
                $"- Name: {tag.Name}\n" +
                $"- Owner: {Context.Guild.GetUser(tag.OwnerId)?.ToString() ?? "<user unknown>"} (ID: {tag.OwnerId})\n" +
                $"- Created At: {tag.CreatedAt}\n" +
                $"- Modifed By: {(tag.ActorId.HasValue ? $"{Context.Guild.GetUser(tag.ActorId.Value)?.ToString() ?? "<user unknown>"}" : "<not modified>")}\n" +
                $"- Modified At: {(tag.UpdatedAt.HasValue ? tag.UpdatedAt.ToString() : "<not modified>")}\n"
            );
        }
    }
}
