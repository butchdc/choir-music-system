using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace choir_music_system.Migrations
{
    /// <inheritdoc />
    public partial class RemoveMassPartFromMusicSheet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MassPart",
                table: "MusicSheets");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MassPart",
                table: "MusicSheets",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }
    }
}
