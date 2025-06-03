using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Effani.Device.Services.Migrations
{
    /// <inheritdoc />
    public partial class CreatedOn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedOn",
                table: "AssayData",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedOn",
                table: "AssayData");
        }
    }
}
