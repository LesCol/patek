using System.Threading.Tasks;
using Discord;
using Discord.Addons.EmojiTools;
using Discord.Commands;

namespace Patek.Modules
{
    public class PatekModuleBase : ModuleBase<SocketCommandContext>
    {
        public static readonly Emoji TagNotFound = EmojiExtensions.FromText("mag_right");
        public static readonly Emoji Pass = EmojiExtensions.FromText("ok_hand");
        public static readonly Emoji Fail = EmojiExtensions.FromText("octagonal_sign");
        public static readonly Emoji Removed = EmojiExtensions.FromText("put_litter_in_its_place");

        public Task ReactAsync(IEmote emote)
            => Context.Message.AddReactionAsync(emote);
    }
}
