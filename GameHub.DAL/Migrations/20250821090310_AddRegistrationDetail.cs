using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameHub.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddRegistrationDetail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK__GameRegi__GameId",
                table: "GameRegistration");

            migrationBuilder.DropIndex(
                name: "UQ__GameRegi__PlayerGame",
                table: "GameRegistration");

            migrationBuilder.AlterColumn<int>(
                name: "GameId",
                table: "GameRegistration",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateTable(
                name: "GameRegistrationDetail",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GameRegistrationId = table.Column<int>(type: "int", nullable: false),
                    GameId = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__GameRegiDetail__ID", x => x.Id);
                    table.ForeignKey(
                        name: "FK__GameRegDetail__GameId",
                        column: x => x.GameId,
                        principalTable: "Game",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK__GameRegDetail__RegistrationId",
                        column: x => x.GameRegistrationId,
                        principalTable: "GameRegistration",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GameRegistration_PlayerId",
                table: "GameRegistration",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_GameRegistrationDetail_GameId",
                table: "GameRegistrationDetail",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "UQ__GameRegDetail__RegGame",
                table: "GameRegistrationDetail",
                columns: new[] { "GameRegistrationId", "GameId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_GameRegistration_Game_GameId",
                table: "GameRegistration",
                column: "GameId",
                principalTable: "Game",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GameRegistration_Game_GameId",
                table: "GameRegistration");

            migrationBuilder.DropTable(
                name: "GameRegistrationDetail");

            migrationBuilder.DropIndex(
                name: "IX_GameRegistration_PlayerId",
                table: "GameRegistration");

            migrationBuilder.AlterColumn<int>(
                name: "GameId",
                table: "GameRegistration",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "UQ__GameRegi__PlayerGame",
                table: "GameRegistration",
                columns: new[] { "PlayerId", "GameId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK__GameRegi__GameId",
                table: "GameRegistration",
                column: "GameId",
                principalTable: "Game",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
