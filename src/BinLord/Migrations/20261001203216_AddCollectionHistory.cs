using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BinLord.Migrations
{
    /// <inheritdoc />
    public partial class AddCollectionHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BinCollectionRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BinScheduleId = table.Column<int>(type: "INTEGER", nullable: false),
                    CollectionDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BinCollectionRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BinCollectionRecords_BinSchedules_BinScheduleId",
                        column: x => x.BinScheduleId,
                        principalTable: "BinSchedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BinCollectionRecords_BinScheduleId_CollectionDate",
                table: "BinCollectionRecords",
                columns: new[] { "BinScheduleId", "CollectionDate" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BinCollectionRecords");
        }
    }
}
