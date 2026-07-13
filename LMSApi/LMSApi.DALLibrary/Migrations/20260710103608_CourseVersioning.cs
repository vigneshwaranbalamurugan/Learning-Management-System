using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMSApi.DALLibrary.Migrations
{
    /// <inheritdoc />
    public partial class CourseVersioning : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 7, 10, 10, 36, 8, 307, DateTimeKind.Utc).AddTicks(9600), new DateTime(2026, 7, 10, 10, 36, 8, 307, DateTimeKind.Utc).AddTicks(9610) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 7, 10, 10, 36, 8, 307, DateTimeKind.Utc).AddTicks(9610), new DateTime(2026, 7, 10, 10, 36, 8, 307, DateTimeKind.Utc).AddTicks(9610) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 7, 10, 10, 36, 8, 307, DateTimeKind.Utc).AddTicks(9610), new DateTime(2026, 7, 10, 10, 36, 8, 307, DateTimeKind.Utc).AddTicks(9610) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 7, 10, 10, 36, 8, 307, DateTimeKind.Utc).AddTicks(5980), new DateTime(2026, 7, 10, 10, 36, 8, 307, DateTimeKind.Utc).AddTicks(5980) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
        }
    }
}
