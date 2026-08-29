using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace choir_music_system.Migrations
{
    /// <inheritdoc />
    public partial class AddMassMusicSheets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MassMusicSheets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MassId = table.Column<int>(type: "INTEGER", nullable: false),
                    MusicSheetId = table.Column<int>(type: "INTEGER", nullable: false),
                    MassPart = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    DisplayOrder = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MassMusicSheets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MassMusicSheets_Masses_MassId",
                        column: x => x.MassId,
                        principalTable: "Masses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MassMusicSheets_MusicSheets_MusicSheetId",
                        column: x => x.MusicSheetId,
                        principalTable: "MusicSheets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MassMusicSheets_MassId",
                table: "MassMusicSheets",
                column: "MassId");

            migrationBuilder.CreateIndex(
                name: "IX_MassMusicSheets_MusicSheetId",
                table: "MassMusicSheets",
                column: "MusicSheetId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MassMusicSheets");
        }
    }
}
