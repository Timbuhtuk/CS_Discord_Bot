using System;
using System.Collections.Generic;

namespace CS_Discord_Bot.Models;

public partial class SongPlaylist
{
    public int Id { get; set; }

    public int SongId { get; set; }

    public int PlaylistId { get; set; }

    public virtual Playlist Playlist { get; set; } = null!;

    public virtual Song Song { get; set; } = null!;
}
