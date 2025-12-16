using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MyApi.Migrations
{
    /// <inheritdoc />
    public partial class AddPlaylistSongReactions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "playlist_song_reactions",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    playlist_id = table.Column<int>(type: "integer", nullable: false),
                    song_id = table.Column<int>(type: "integer", nullable: false),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    is_like = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    playlist_song_playlist_id = table.Column<int>(type: "integer", nullable: true),
                    playlist_song_song_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_playlist_song_reactions", x => x.id);
                    table.ForeignKey(
                        name: "fk_playlist_song_reactions_playlist_songs_playlist_song_playli",
                        columns: x => new { x.playlist_song_playlist_id, x.playlist_song_song_id },
                        principalTable: "playlist_songs",
                        principalColumns: new[] { "playlist_id", "song_id" });
                    table.ForeignKey(
                        name: "fk_playlist_song_reactions_playlists_playlist_id",
                        column: x => x.playlist_id,
                        principalTable: "playlists",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_playlist_song_reactions_songs_song_id",
                        column: x => x.song_id,
                        principalTable: "songs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_playlist_song_reactions_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_playlist_song_reactions_playlist_id_song_id_user_id",
                table: "playlist_song_reactions",
                columns: new[] { "playlist_id", "song_id", "user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_playlist_song_reactions_playlist_song_playlist_id_playlist_",
                table: "playlist_song_reactions",
                columns: new[] { "playlist_song_playlist_id", "playlist_song_song_id" });

            migrationBuilder.CreateIndex(
                name: "ix_playlist_song_reactions_song_id",
                table: "playlist_song_reactions",
                column: "song_id");

            migrationBuilder.CreateIndex(
                name: "ix_playlist_song_reactions_user_id",
                table: "playlist_song_reactions",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "playlist_song_reactions");
        }
    }
}
