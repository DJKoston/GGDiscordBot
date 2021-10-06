using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordBot.DAL.Models.Playlists
{
    public class Playlist : Entity
    {
        public ulong RequestorId { get; set; }
        public ulong GuildId { get; set; }
        public Uri TrackURL { get; set; }
    }
}
