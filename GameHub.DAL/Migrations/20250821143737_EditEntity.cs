using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameHub.DAL.Migrations
{
    /// <inheritdoc />
    public partial class EditEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GameRegistration_Game_GameId",
                table: "GameRegistration");

            migrationBuilder.DropIndex(
                name: "IX_GameRegistration_GameId",
                table: "GameRegistration");

            migrationBuilder.DropColumn(
                name: "GameId",
                table: "GameRegistration");

            migrationBuilder.AddColumn<string>(
                name: "AvatarPath",
                table: "Player",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvatarPath",
                table: "Player");

            migrationBuilder.AddColumn<int>(
                name: "GameId",
                table: "GameRegistration",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_GameRegistration_GameId",
                table: "GameRegistration",
                column: "GameId");

            migrationBuilder.AddForeignKey(
                name: "FK_GameRegistration_Game_GameId",
                table: "GameRegistration",
                column: "GameId",
                principalTable: "Game",
                principalColumn: "Id");
        }
    }
}
