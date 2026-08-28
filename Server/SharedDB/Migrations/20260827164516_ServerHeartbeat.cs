using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SharedDB.Migrations
{
    /// <inheritdoc />
    public partial class ServerHeartbeat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastPingTime",
                table: "ServerInfo",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastPingTime",
                table: "ServerInfo");
        }
    }
}
