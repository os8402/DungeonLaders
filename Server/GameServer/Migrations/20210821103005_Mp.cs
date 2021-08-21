using Microsoft.EntityFrameworkCore.Migrations;

namespace GameServer.Migrations
{
    public partial class Mp : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CurExp",
                table: "Player",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MaxMp",
                table: "Player",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Mp",
                table: "Player",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurExp",
                table: "Player");

            migrationBuilder.DropColumn(
                name: "MaxMp",
                table: "Player");

            migrationBuilder.DropColumn(
                name: "Mp",
                table: "Player");
        }
    }
}
