using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace choir_music_system.Migrations
{
    /// <inheritdoc />
    public partial class AddSuggestedMassPartToMusicSheet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SuggestedMassPart",
                table: "MusicSheets",
                type: "TEXT",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SuggestedMassPart",
                table: "MusicSheets");
        }
    }
}
