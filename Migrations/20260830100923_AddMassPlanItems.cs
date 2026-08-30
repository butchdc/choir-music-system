using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace choir_music_system.Migrations
{
    /// <inheritdoc />
    public partial class AddMassPlanItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MassPlanItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MassId = table.Column<int>(type: "INTEGER", nullable: false),
                    ItemType = table.Column<string>(type: "TEXT", nullable: false),
                    SongId = table.Column<int>(type: "INTEGER", nullable: true),
                    PresentationItemId = table.Column<int>(type: "INTEGER", nullable: true),
                    MassPart = table.Column<string>(type: "TEXT", nullable: true),
                    DisplayOrder = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MassPlanItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MassPlanItems_Masses_MassId",
                        column: x => x.MassId,
                        principalTable: "Masses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MassPlanItems_MusicSheets_SongId",
                        column: x => x.SongId,
                        principalTable: "MusicSheets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MassPlanItems_PresentationItems_PresentationItemId",
                        column: x => x.PresentationItemId,
                        principalTable: "PresentationItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MassPlanItems_MassId",
                table: "MassPlanItems",
                column: "MassId");

            migrationBuilder.CreateIndex(
                name: "IX_MassPlanItems_PresentationItemId",
                table: "MassPlanItems",
                column: "PresentationItemId");

            migrationBuilder.CreateIndex(
                name: "IX_MassPlanItems_SongId",
                table: "MassPlanItems",
                column: "SongId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MassPlanItems");
        }
    }
}
