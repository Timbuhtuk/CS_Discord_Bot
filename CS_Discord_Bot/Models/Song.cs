using System;
using System.Collections.Generic;

namespace CS_Discord_Bot.Models;

public partial class Song
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string AuthorName { get; set; } = null!;

    public double? Duration { get; set; } // in minutes (to midnight (ㆆ_ㆆ) )

    public string? Link { get; set; }

    public string? FilePath { get; set; }

    public virtual ICollection<Playlist> Playlists { get; set; } = new List<Playlist>();
}
