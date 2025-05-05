namespace CS_Discord_Bot.Models;

public partial class Playlist
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public ulong AuthorId { get; set; }

    public DateOnly CreationDate { get; set; }

    public bool IsPublic { get; set; } = false;

    public virtual ICollection<Guild> Guilds { get; set; } = new List<Guild>();

    public virtual ICollection<Song> Songs { get; set; } = new List<Song>();
}
