using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CS_Discord_Bot.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Guild",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Discord_id = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                    Anchor = table.Column<decimal>(type: "decimal(20,0)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Guild__3213E83FA17C45A2", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Playlist",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Author_id = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                    Creation_date = table.Column<DateOnly>(type: "date", nullable: false),
                    Is_public = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Playlist__3213E83F9A148AA8", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Song",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Author_name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false, defaultValue: "NN"),
                    Duration = table.Column<double>(type: "float", nullable: true),
                    Link = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    File_path = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Song__3213E83F33BB9D15", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "GuildPlaylist",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    guild_id = table.Column<int>(type: "int", nullable: false),
                    playlist_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__GuildPla__3213E83FB4C15C0A", x => x.id);
                    table.ForeignKey(
                        name: "FK__GuildPlay__guild__4316F928",
                        column: x => x.guild_id,
                        principalTable: "Guild",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK__GuildPlay__playl__440B1D61",
                        column: x => x.playlist_id,
                        principalTable: "Playlist",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SongPlaylist",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    song_id = table.Column<int>(type: "int", nullable: false),
                    playlist_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__SongPlay__3213E83FA68EB569", x => x.id);
                    table.ForeignKey(
                        name: "FK__SongPlayl__playl__403A8C7D",
                        column: x => x.playlist_id,
                        principalTable: "Playlist",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK__SongPlayl__song___3F466844",
                        column: x => x.song_id,
                        principalTable: "Song",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GuildPlaylist_guild_id",
                table: "GuildPlaylist",
                column: "guild_id");

            migrationBuilder.CreateIndex(
                name: "IX_GuildPlaylist_playlist_id",
                table: "GuildPlaylist",
                column: "playlist_id");

            migrationBuilder.CreateIndex(
                name: "IX_SongPlaylist_playlist_id",
                table: "SongPlaylist",
                column: "playlist_id");

            migrationBuilder.CreateIndex(
                name: "IX_SongPlaylist_song_id",
                table: "SongPlaylist",
                column: "song_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GuildPlaylist");

            migrationBuilder.DropTable(
                name: "SongPlaylist");

            migrationBuilder.DropTable(
                name: "Guild");

            migrationBuilder.DropTable(
                name: "Playlist");

            migrationBuilder.DropTable(
                name: "Song");
        }
    }
}
