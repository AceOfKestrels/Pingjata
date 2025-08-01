using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pingjata.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CreateInitial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "channels",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    greeting = table.Column<string>(type: "text", nullable: false),
                    current_counter = table.Column<int>(type: "integer", nullable: false),
                    current_threshold = table.Column<int>(type: "integer", nullable: false),
                    threshold_range = table.Column<string>(type: "text", nullable: false),
                    winner_id = table.Column<string>(type: "text", nullable: true),
                    round_ended_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    row_version = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_channels", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "channels");
        }
    }
}
