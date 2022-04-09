using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreateAllSongPlaylist
{
    public class PlaylistEntity
    {
        public string playlistTitle { get; set; } = "";
        public string playlistAuthor { get; set; } = "";
        public string playlistDescription { get; set; } = "";
        public ReadOnlyCollection<SongInfoEntity> songs { get; set; } = new ReadOnlyCollection<SongInfoEntity>(Array.Empty<SongInfoEntity>());
    }
}
