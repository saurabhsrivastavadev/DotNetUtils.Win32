using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DotNetUtils.Win32.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserActivityMetaInfoSet",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LatestMonitoringEventTime = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserActivityMetaInfoSet", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserActivitySessionSet",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserActivityState = table.Column<int>(type: "INTEGER", nullable: false),
                    SessionStartTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    SessionEndTime = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserActivitySessionSet", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserActivityMetaInfoSet");

            migrationBuilder.DropTable(
                name: "UserActivitySessionSet");
        }
    }
}
