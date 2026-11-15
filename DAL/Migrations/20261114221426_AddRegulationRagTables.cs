using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddRegulationRagTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RegulationVersions",
                columns: table => new
                {
                    VersionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VersionNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegulationVersions", x => x.VersionId);
                });

            migrationBuilder.CreateTable(
                name: "RegulationChunks",
                columns: table => new
                {
                    ChunkId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VersionId = table.Column<int>(type: "int", nullable: false),
                    SectionNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Embedding = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegulationChunks", x => x.ChunkId);
                    table.ForeignKey(
                        name: "FK_RegulationChunks_RegulationVersions_VersionId",
                        column: x => x.VersionId,
                        principalTable: "RegulationVersions",
                        principalColumn: "VersionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RegulationChunks_VersionId_SectionNumber",
                table: "RegulationChunks",
                columns: new[] { "VersionId", "SectionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RegulationVersions_VersionNumber",
                table: "RegulationVersions",
                column: "VersionNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RegulationChunks");

            migrationBuilder.DropTable(
                name: "RegulationVersions");
        }
    }
}
