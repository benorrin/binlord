using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BinLord.Migrations
{
    /// <inheritdoc />
    public partial class AddBinActionTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "BinCollectionRecords",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddColumn<DateTime>(
                name: "BroughtInAt",
                table: "BinCollectionRecords",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TakenOutAt",
                table: "BinCollectionRecords",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BroughtInAt",
                table: "BinCollectionRecords");

            migrationBuilder.DropColumn(
                name: "TakenOutAt",
                table: "BinCollectionRecords");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "BinCollectionRecords",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);
        }
    }
}
