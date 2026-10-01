using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BinLord.Migrations
{
    /// <inheritdoc />
    public partial class AddAppSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AppName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    TimeZoneId = table.Column<string>(type: "TEXT", nullable: false),
                    BringInAfterHour = table.Column<int>(type: "INTEGER", nullable: false),
                    FeedOccurrencesPerBin = table.Column<int>(type: "INTEGER", nullable: false),
                    HistoryOccurrencesPerBin = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppSettings", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppSettings");
        }
    }
}
