using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BinLord.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "BringInReminderSentAt",
                table: "BinCollectionRecords",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PutOutReminderSentAt",
                table: "BinCollectionRecords",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmailFrom",
                table: "AppSettings",
                type: "TEXT",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EmailNotificationsEnabled",
                table: "AppSettings",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "EmailTo",
                table: "AppSettings",
                type: "TEXT",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "NtfyEnabled",
                table: "AppSettings",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "NtfyServerUrl",
                table: "AppSettings",
                type: "TEXT",
                maxLength: 200,
                nullable: false,
                defaultValue: "https://ntfy.sh");

            migrationBuilder.AddColumn<string>(
                name: "NtfyToken",
                table: "AppSettings",
                type: "TEXT",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NtfyTopic",
                table: "AppSettings",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SmtpHost",
                table: "AppSettings",
                type: "TEXT",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SmtpPassword",
                table: "AppSettings",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SmtpPort",
                table: "AppSettings",
                type: "INTEGER",
                nullable: false,
                defaultValue: 587);

            migrationBuilder.AddColumn<bool>(
                name: "SmtpUseSsl",
                table: "AppSettings",
                type: "INTEGER",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "SmtpUsername",
                table: "AppSettings",
                type: "TEXT",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BringInReminderSentAt",
                table: "BinCollectionRecords");

            migrationBuilder.DropColumn(
                name: "PutOutReminderSentAt",
                table: "BinCollectionRecords");

            migrationBuilder.DropColumn(
                name: "EmailFrom",
                table: "AppSettings");

            migrationBuilder.DropColumn(
                name: "EmailNotificationsEnabled",
                table: "AppSettings");

            migrationBuilder.DropColumn(
                name: "EmailTo",
                table: "AppSettings");

            migrationBuilder.DropColumn(
                name: "NtfyEnabled",
                table: "AppSettings");

            migrationBuilder.DropColumn(
                name: "NtfyServerUrl",
                table: "AppSettings");

            migrationBuilder.DropColumn(
                name: "NtfyToken",
                table: "AppSettings");

            migrationBuilder.DropColumn(
                name: "NtfyTopic",
                table: "AppSettings");

            migrationBuilder.DropColumn(
                name: "SmtpHost",
                table: "AppSettings");

            migrationBuilder.DropColumn(
                name: "SmtpPassword",
                table: "AppSettings");

            migrationBuilder.DropColumn(
                name: "SmtpPort",
                table: "AppSettings");

            migrationBuilder.DropColumn(
                name: "SmtpUseSsl",
                table: "AppSettings");

            migrationBuilder.DropColumn(
                name: "SmtpUsername",
                table: "AppSettings");
        }
    }
}
