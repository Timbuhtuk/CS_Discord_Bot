namespace CS_Discord_Bot.Models;

public partial class Guild
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public ulong DiscordId { get; set; }

    public ulong? Anchor { get; set; }

    public virtual ICollection<Playlist> Playlists { get; set; } = new List<Playlist>();
}
