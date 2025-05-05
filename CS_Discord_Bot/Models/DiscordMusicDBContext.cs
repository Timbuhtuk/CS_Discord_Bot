using CS_Discord_Bot.Models;
using CS_Discord_Bot;
using Microsoft.EntityFrameworkCore;

public partial class DiscordMusicDBContext : DbContext
{
    public DiscordMusicDBContext()
    {
    }

    public DiscordMusicDBContext(DbContextOptions<DiscordMusicDBContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Guild> Guilds { get; set; }
    public virtual DbSet<Playlist> Playlists { get; set; }
    public virtual DbSet<Song> Songs { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer(Program.app_config["connection_string"]);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Guild>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Guild__3213E83FA17C45A2");
            entity.ToTable("Guild");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DiscordId).HasColumnName("Discord_id");
            entity.Property(e => e.Name).HasMaxLength(128);
        });

        modelBuilder.Entity<Playlist>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Playlist__3213E83F9A148AA8");
            entity.ToTable("Playlist");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AuthorId).HasColumnName("Author_id");
            entity.Property(e => e.CreationDate).HasColumnName("Creation_date");
            entity.Property(e => e.IsPublic).HasColumnName("Is_public");
            entity.Property(e => e.Name).HasMaxLength(128);
        });

        modelBuilder.Entity<Song>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Song__3213E83F33BB9D15");
            entity.ToTable("Song");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AuthorName)
                .HasMaxLength(128)
                .HasDefaultValue("NN")
                .HasColumnName("Author_name");
            entity.Property(e => e.FilePath)
                .HasMaxLength(320)
                .HasColumnName("File_path");
            entity.Property(e => e.Link).HasMaxLength(256);
            entity.Property(e => e.Name).HasMaxLength(128);
        });

        // Настраиваем многие-ко-многим без промежуточных сущностей
        modelBuilder.Entity<Song>()
            .HasMany(s => s.Playlists)
            .WithMany(p => p.Songs)
            .UsingEntity<Dictionary<string, object>>(
                "SongPlaylist",
                j => j.HasOne<Playlist>().WithMany().HasForeignKey("PlaylistId"),
                j => j.HasOne<Song>().WithMany().HasForeignKey("SongId"),
                j => j.HasKey("SongId", "PlaylistId")
            );

        modelBuilder.Entity<Guild>()
            .HasMany(g => g.Playlists)
            .WithMany(p => p.Guilds)
            .UsingEntity<Dictionary<string, object>>(
                "GuildPlaylist",
                j => j.HasOne<Playlist>().WithMany().HasForeignKey("PlaylistId"),
                j => j.HasOne<Guild>().WithMany().HasForeignKey("GuildId"),
                j => j.HasKey("GuildId", "PlaylistId")
            );

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

    public static async Task<int> ResolveMissingfilesInDB()
    {
        using LogScope log_scope = new LogScope("CleanUpDb called", ConsoleColor.Red);
        using var context = new DiscordMusicDBContext();
        int resolved = 0;

        List<Song> songs = context.Songs.ToList();
        foreach (Song song in songs)
        {
            if (song.FilePath == null || !File.Exists(song.FilePath))
            {
                Song? downloaded_song = await AudioDownloader.Download(song, Program.app_config["music_client:music_folder"]!);
                if (downloaded_song == null)
                {
                    context.Remove(song);
                    await Logs.AddLog($"{song.Name} - removed from DB");
                }
                else
                {
                    song.FilePath = downloaded_song.FilePath;
                }

                resolved++;
            }
        }
        List<string?> file_pathes = context.Songs.Select(s => s.FilePath).ToList();
        foreach (var file in Directory.GetFiles(Program.app_config["music_client:music_folder"]!))
        {
            if (!file_pathes.Contains(file))
                File.Delete(file);
        }
        context.SaveChanges();
        return resolved;
    }
}
