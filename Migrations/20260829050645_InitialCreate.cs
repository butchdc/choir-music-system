using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace choir_music_system.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MusicSheets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    MassPart = table.Column<string>(type: "TEXT", nullable: false),
                    Composer = table.Column<string>(type: "TEXT", nullable: true),
                    Arrangement = table.Column<string>(type: "TEXT", nullable: true),
                    Key = table.Column<string>(type: "TEXT", nullable: true),
                    PdfFileName = table.Column<string>(type: "TEXT", nullable: false),
                    PdfPath = table.Column<string>(type: "TEXT", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MusicSheets", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MusicSheets");
        }
    }
}
