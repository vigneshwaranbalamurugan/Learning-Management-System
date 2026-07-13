using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMSApi.DALLibrary.Migrations
{
    /// <inheritdoc />
    public partial class AddCourseVersioningFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "StudentProgress",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsOnLatestVersion",
                table: "Enrollments",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsBeingUpdated",
                table: "Courses",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PreviousPublishedSnapshotJson",
                table: "Courses",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PublishedSnapshotJson",
                table: "Courses",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VersionNumber",
                table: "Courses",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 7, 10, 9, 38, 39, 519, DateTimeKind.Utc).AddTicks(2050), new DateTime(2026, 7, 10, 9, 38, 39, 519, DateTimeKind.Utc).AddTicks(2050) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 7, 10, 9, 38, 39, 519, DateTimeKind.Utc).AddTicks(2060), new DateTime(2026, 7, 10, 9, 38, 39, 519, DateTimeKind.Utc).AddTicks(2060) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 7, 10, 9, 38, 39, 519, DateTimeKind.Utc).AddTicks(2060), new DateTime(2026, 7, 10, 9, 38, 39, 519, DateTimeKind.Utc).AddTicks(2060) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 7, 10, 9, 38, 39, 518, DateTimeKind.Utc).AddTicks(6600), new DateTime(2026, 7, 10, 9, 38, 39, 518, DateTimeKind.Utc).AddTicks(6600) });

            migrationBuilder.CreateIndex(
                name: "IX_Enrollments_IsOnLatestVersion",
                table: "Enrollments",
                column: "IsOnLatestVersion");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Enrollments_IsOnLatestVersion",
                table: "Enrollments");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "StudentProgress");

            migrationBuilder.DropColumn(
                name: "IsOnLatestVersion",
                table: "Enrollments");

            migrationBuilder.DropColumn(
                name: "IsBeingUpdated",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "PreviousPublishedSnapshotJson",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "PublishedSnapshotJson",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "VersionNumber",
                table: "Courses");

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 7, 7, 16, 39, 35, 256, DateTimeKind.Utc).AddTicks(8100), new DateTime(2026, 7, 7, 16, 39, 35, 256, DateTimeKind.Utc).AddTicks(8100) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 7, 7, 16, 39, 35, 256, DateTimeKind.Utc).AddTicks(8100), new DateTime(2026, 7, 7, 16, 39, 35, 256, DateTimeKind.Utc).AddTicks(8100) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 7, 7, 16, 39, 35, 256, DateTimeKind.Utc).AddTicks(8100), new DateTime(2026, 7, 7, 16, 39, 35, 256, DateTimeKind.Utc).AddTicks(8100) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 7, 7, 16, 39, 35, 256, DateTimeKind.Utc).AddTicks(4860), new DateTime(2026, 7, 7, 16, 39, 35, 256, DateTimeKind.Utc).AddTicks(4860) });
        }
    }
}
