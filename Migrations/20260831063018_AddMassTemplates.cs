using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace choir_music_system.Migrations
{
    /// <inheritdoc />
    public partial class AddMassTemplates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MassTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MassTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MassTemplateItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MassTemplateId = table.Column<int>(type: "INTEGER", nullable: false),
                    ItemType = table.Column<string>(type: "TEXT", nullable: false),
                    SongId = table.Column<int>(type: "INTEGER", nullable: true),
                    PresentationItemId = table.Column<int>(type: "INTEGER", nullable: true),
                    MassPart = table.Column<string>(type: "TEXT", nullable: true),
                    DisplayOrder = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MassTemplateItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MassTemplateItems_MassTemplates_MassTemplateId",
                        column: x => x.MassTemplateId,
                        principalTable: "MassTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MassTemplateItems_MusicSheets_SongId",
                        column: x => x.SongId,
                        principalTable: "MusicSheets",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MassTemplateItems_PresentationItems_PresentationItemId",
                        column: x => x.PresentationItemId,
                        principalTable: "PresentationItems",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_MassTemplateItems_MassTemplateId",
                table: "MassTemplateItems",
                column: "MassTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_MassTemplateItems_PresentationItemId",
                table: "MassTemplateItems",
                column: "PresentationItemId");

            migrationBuilder.CreateIndex(
                name: "IX_MassTemplateItems_SongId",
                table: "MassTemplateItems",
                column: "SongId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MassTemplateItems");

            migrationBuilder.DropTable(
                name: "MassTemplates");
        }
    }
}
