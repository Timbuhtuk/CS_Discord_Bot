using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CS_Discord_Bot.Migrations
{
    /// <inheritdoc />
    public partial class Simplify : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK__GuildPlay__guild__4316F928",
                table: "GuildPlaylist");

            migrationBuilder.DropForeignKey(
                name: "FK__GuildPlay__playl__440B1D61",
                table: "GuildPlaylist");

            migrationBuilder.DropForeignKey(
                name: "FK__SongPlayl__playl__403A8C7D",
                table: "SongPlaylist");

            migrationBuilder.DropForeignKey(
                name: "FK__SongPlayl__song___3F466844",
                table: "SongPlaylist");

            migrationBuilder.DropPrimaryKey(
                name: "PK__SongPlay__3213E83FA68EB569",
                table: "SongPlaylist");

            migrationBuilder.DropIndex(
                name: "IX_SongPlaylist_song_id",
                table: "SongPlaylist");

            migrationBuilder.DropPrimaryKey(
                name: "PK__GuildPla__3213E83FB4C15C0A",
                table: "GuildPlaylist");

            migrationBuilder.DropIndex(
                name: "IX_GuildPlaylist_guild_id",
                table: "GuildPlaylist");

            migrationBuilder.DropColumn(
                name: "id",
                table: "SongPlaylist");

            migrationBuilder.DropColumn(
                name: "id",
                table: "GuildPlaylist");

            migrationBuilder.RenameColumn(
                name: "song_id",
                table: "SongPlaylist",
                newName: "SongId");

            migrationBuilder.RenameColumn(
                name: "playlist_id",
                table: "SongPlaylist",
                newName: "PlaylistId");

            migrationBuilder.RenameIndex(
                name: "IX_SongPlaylist_playlist_id",
                table: "SongPlaylist",
                newName: "IX_SongPlaylist_PlaylistId");

            migrationBuilder.RenameColumn(
                name: "playlist_id",
                table: "GuildPlaylist",
                newName: "PlaylistId");

            migrationBuilder.RenameColumn(
                name: "guild_id",
                table: "GuildPlaylist",
                newName: "GuildId");

            migrationBuilder.RenameIndex(
                name: "IX_GuildPlaylist_playlist_id",
                table: "GuildPlaylist",
                newName: "IX_GuildPlaylist_PlaylistId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SongPlaylist",
                table: "SongPlaylist",
                columns: new[] { "SongId", "PlaylistId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_GuildPlaylist",
                table: "GuildPlaylist",
                columns: new[] { "GuildId", "PlaylistId" });

            migrationBuilder.AddForeignKey(
                name: "FK_GuildPlaylist_Guild_GuildId",
                table: "GuildPlaylist",
                column: "GuildId",
                principalTable: "Guild",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GuildPlaylist_Playlist_PlaylistId",
                table: "GuildPlaylist",
                column: "PlaylistId",
                principalTable: "Playlist",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SongPlaylist_Playlist_PlaylistId",
                table: "SongPlaylist",
                column: "PlaylistId",
                principalTable: "Playlist",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SongPlaylist_Song_SongId",
                table: "SongPlaylist",
                column: "SongId",
                principalTable: "Song",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GuildPlaylist_Guild_GuildId",
                table: "GuildPlaylist");

            migrationBuilder.DropForeignKey(
                name: "FK_GuildPlaylist_Playlist_PlaylistId",
                table: "GuildPlaylist");

            migrationBuilder.DropForeignKey(
                name: "FK_SongPlaylist_Playlist_PlaylistId",
                table: "SongPlaylist");

            migrationBuilder.DropForeignKey(
                name: "FK_SongPlaylist_Song_SongId",
                table: "SongPlaylist");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SongPlaylist",
                table: "SongPlaylist");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GuildPlaylist",
                table: "GuildPlaylist");

            migrationBuilder.RenameColumn(
                name: "PlaylistId",
                table: "SongPlaylist",
                newName: "playlist_id");

            migrationBuilder.RenameColumn(
                name: "SongId",
                table: "SongPlaylist",
                newName: "song_id");

            migrationBuilder.RenameIndex(
                name: "IX_SongPlaylist_PlaylistId",
                table: "SongPlaylist",
                newName: "IX_SongPlaylist_playlist_id");

            migrationBuilder.RenameColumn(
                name: "PlaylistId",
                table: "GuildPlaylist",
                newName: "playlist_id");

            migrationBuilder.RenameColumn(
                name: "GuildId",
                table: "GuildPlaylist",
                newName: "guild_id");

            migrationBuilder.RenameIndex(
                name: "IX_GuildPlaylist_PlaylistId",
                table: "GuildPlaylist",
                newName: "IX_GuildPlaylist_playlist_id");

            migrationBuilder.AddColumn<int>(
                name: "id",
                table: "SongPlaylist",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<int>(
                name: "id",
                table: "GuildPlaylist",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK__SongPlay__3213E83FA68EB569",
                table: "SongPlaylist",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK__GuildPla__3213E83FB4C15C0A",
                table: "GuildPlaylist",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "IX_SongPlaylist_song_id",
                table: "SongPlaylist",
                column: "song_id");

            migrationBuilder.CreateIndex(
                name: "IX_GuildPlaylist_guild_id",
                table: "GuildPlaylist",
                column: "guild_id");

            migrationBuilder.AddForeignKey(
                name: "FK__GuildPlay__guild__4316F928",
                table: "GuildPlaylist",
                column: "guild_id",
                principalTable: "Guild",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK__GuildPlay__playl__440B1D61",
                table: "GuildPlaylist",
                column: "playlist_id",
                principalTable: "Playlist",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK__SongPlayl__playl__403A8C7D",
                table: "SongPlaylist",
                column: "playlist_id",
                principalTable: "Playlist",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK__SongPlayl__song___3F466844",
                table: "SongPlaylist",
                column: "song_id",
                principalTable: "Song",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
