using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace choir_music_system.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomSongPresentation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CustomPresentationFileName",
                table: "MusicSheets",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomPresentationPath",
                table: "MusicSheets",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CustomPresentationUpdatedDate",
                table: "MusicSheets",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomPresentationFileName",
                table: "MusicSheets");

            migrationBuilder.DropColumn(
                name: "CustomPresentationPath",
                table: "MusicSheets");

            migrationBuilder.DropColumn(
                name: "CustomPresentationUpdatedDate",
                table: "MusicSheets");
        }
    }
}
