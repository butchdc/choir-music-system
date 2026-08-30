using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace choir_music_system.Migrations
{
    /// <inheritdoc />
    public partial class AddSongLicensing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OneLicenseNumber",
                table: "MusicSheets",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Publisher",
                table: "MusicSheets",
                type: "TEXT",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CopyrightText",
                table: "MusicSheets",
                type: "TEXT",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OneLicenseNumber",
                table: "MusicSheets");

            migrationBuilder.DropColumn(
                name: "Publisher",
                table: "MusicSheets");

            migrationBuilder.DropColumn(
                name: "CopyrightText",
                table: "MusicSheets");
        }
    }
}
