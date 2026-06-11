using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMSApi.DALLibrary.Migrations
{
    /// <inheritdoc />
    public partial class AddPublishStatusToContent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPublished",
                table: "Lessons");

            migrationBuilder.DropColumn(
                name: "IsPublished",
                table: "CourseSections");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Lessons",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "LessonResources",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "CourseSections",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 10, 10, 56, 34, 57, DateTimeKind.Utc).AddTicks(9210), new DateTime(2026, 6, 10, 10, 56, 34, 57, DateTimeKind.Utc).AddTicks(9210) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 10, 10, 56, 34, 57, DateTimeKind.Utc).AddTicks(9210), new DateTime(2026, 6, 10, 10, 56, 34, 57, DateTimeKind.Utc).AddTicks(9210) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 10, 10, 56, 34, 57, DateTimeKind.Utc).AddTicks(9210), new DateTime(2026, 6, 10, 10, 56, 34, 57, DateTimeKind.Utc).AddTicks(9210) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 10, 10, 56, 34, 57, DateTimeKind.Utc).AddTicks(6870), new DateTime(2026, 6, 10, 10, 56, 34, 57, DateTimeKind.Utc).AddTicks(6870) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Lessons");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "LessonResources");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "CourseSections");

            migrationBuilder.AddColumn<bool>(
                name: "IsPublished",
                table: "Lessons",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsPublished",
                table: "CourseSections",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 10, 4, 17, 57, 563, DateTimeKind.Utc).AddTicks(7830), new DateTime(2026, 6, 10, 4, 17, 57, 563, DateTimeKind.Utc).AddTicks(7830) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 10, 4, 17, 57, 563, DateTimeKind.Utc).AddTicks(7830), new DateTime(2026, 6, 10, 4, 17, 57, 563, DateTimeKind.Utc).AddTicks(7830) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 10, 4, 17, 57, 563, DateTimeKind.Utc).AddTicks(7830), new DateTime(2026, 6, 10, 4, 17, 57, 563, DateTimeKind.Utc).AddTicks(7830) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 10, 4, 17, 57, 563, DateTimeKind.Utc).AddTicks(4460), new DateTime(2026, 6, 10, 4, 17, 57, 563, DateTimeKind.Utc).AddTicks(4460) });
        }
    }
}
