using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BeatIt.Migrations
{
    /// <inheritdoc />
    public partial class AlterGameIdToIgdbGameId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Backlog_Game_GameId",
                table: "Backlog");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Game_GameId",
                table: "Game");

            migrationBuilder.RenameColumn(
                name: "GameId",
                table: "Game",
                newName: "IgdbGameId");

            migrationBuilder.RenameIndex(
                name: "IX_Game_GameId",
                table: "Game",
                newName: "IX_Game_IgdbGameId");

            migrationBuilder.AddForeignKey(
                name: "FK_Backlog_Game_GameId",
                table: "Backlog",
                column: "GameId",
                principalTable: "Game",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Backlog_Game_GameId",
                table: "Backlog");

            migrationBuilder.RenameColumn(
                name: "IgdbGameId",
                table: "Game",
                newName: "GameId");

            migrationBuilder.RenameIndex(
                name: "IX_Game_IgdbGameId",
                table: "Game",
                newName: "IX_Game_GameId");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Game_GameId",
                table: "Game",
                column: "GameId");

            migrationBuilder.AddForeignKey(
                name: "FK_Backlog_Game_GameId",
                table: "Backlog",
                column: "GameId",
                principalTable: "Game",
                principalColumn: "GameId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
