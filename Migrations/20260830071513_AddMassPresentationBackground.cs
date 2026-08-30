using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace choir_music_system.Migrations
{
    /// <inheritdoc />
    public partial class AddMassPresentationBackground : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PresentationBackgroundPath",
                table: "Masses",
                type: "TEXT",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PresentationBackgroundPath",
                table: "Masses");
        }
    }
}
