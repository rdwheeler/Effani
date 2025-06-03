using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Effani.Device.Services.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AssayData",
                columns: table => new
                {
                    AssayId = table.Column<string>(type: "TEXT", nullable: false),
                    AssayJson = table.Column<string>(type: "TEXT", nullable: false),
                    AssayName = table.Column<string>(type: "TEXT", nullable: false),
                    UploadedOn = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssayData", x => x.AssayId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssayData");
        }
    }
}
