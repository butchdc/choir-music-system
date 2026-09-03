using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace choir_music_system.Migrations
{
    /// <inheritdoc />
    public partial class AddFinalMassPresentation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FinalPresentationFileName",
                table: "Masses",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FinalPresentationPath",
                table: "Masses",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FinalPresentationUpdatedDate",
                table: "Masses",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FinalPresentationFileName",
                table: "Masses");

            migrationBuilder.DropColumn(
                name: "FinalPresentationPath",
                table: "Masses");

            migrationBuilder.DropColumn(
                name: "FinalPresentationUpdatedDate",
                table: "Masses");
        }
    }
}
