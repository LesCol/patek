using System.Linq;
using Discord;

namespace Patek
{
    public class Filter
    {
        public bool IsWhitelisted(IChannel channel)
            => Channels.Contains(channel.Id) || Guilds.Contains((channel as IGuildChannel)?.GuildId ?? 1UL);
        public bool IsElevated(IUser user)
            => Users.Contains(user.Id);

        public ulong[] Guilds { get; set; } = new ulong[0];
        public ulong[] Channels { get; set; } = new ulong[0];
        public ulong[] Users { get; set; } = new ulong[0];
    }
}
