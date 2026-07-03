using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMSApi.DALLibrary.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftDeleteToCourses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Courses",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Courses",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 7, 3, 17, 19, 41, 124, DateTimeKind.Utc).AddTicks(1810), new DateTime(2026, 7, 3, 17, 19, 41, 124, DateTimeKind.Utc).AddTicks(1810) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 7, 3, 17, 19, 41, 124, DateTimeKind.Utc).AddTicks(1810), new DateTime(2026, 7, 3, 17, 19, 41, 124, DateTimeKind.Utc).AddTicks(1810) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 7, 3, 17, 19, 41, 124, DateTimeKind.Utc).AddTicks(1810), new DateTime(2026, 7, 3, 17, 19, 41, 124, DateTimeKind.Utc).AddTicks(1810) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 7, 3, 17, 19, 41, 123, DateTimeKind.Utc).AddTicks(7870), new DateTime(2026, 7, 3, 17, 19, 41, 123, DateTimeKind.Utc).AddTicks(7870) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Courses");

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 7, 3, 11, 16, 39, 410, DateTimeKind.Utc).AddTicks(2430), new DateTime(2026, 7, 3, 11, 16, 39, 410, DateTimeKind.Utc).AddTicks(2430) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 7, 3, 11, 16, 39, 410, DateTimeKind.Utc).AddTicks(2430), new DateTime(2026, 7, 3, 11, 16, 39, 410, DateTimeKind.Utc).AddTicks(2430) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 7, 3, 11, 16, 39, 410, DateTimeKind.Utc).AddTicks(2430), new DateTime(2026, 7, 3, 11, 16, 39, 410, DateTimeKind.Utc).AddTicks(2430) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 7, 3, 11, 16, 39, 409, DateTimeKind.Utc).AddTicks(8850), new DateTime(2026, 7, 3, 11, 16, 39, 409, DateTimeKind.Utc).AddTicks(8850) });
        }
    }
}
