using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Paste.Shared.Migrations
{
    public partial class AddDateAndUsageStats : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastViewed",
                table: "Uploads",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "Timestamp",
                table: "Uploads",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<int>(
                name: "Views",
                table: "Uploads",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastViewed",
                table: "Uploads");

            migrationBuilder.DropColumn(
                name: "Timestamp",
                table: "Uploads");

            migrationBuilder.DropColumn(
                name: "Views",
                table: "Uploads");
        }
    }
}
