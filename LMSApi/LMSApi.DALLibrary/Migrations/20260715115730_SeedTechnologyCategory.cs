using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMSApi.DALLibrary.Migrations
{
    /// <inheritdoc />
    public partial class SeedTechnologyCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "CourseCategories",
                columns: new[] { "Id", "CreatedAt", "Description", "Name", "UpdatedAt" },
                values: new object[] { 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Learn programming, networking, and all things tech.", "Technology", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 7, 15, 11, 57, 28, 982, DateTimeKind.Utc).AddTicks(1020), new DateTime(2026, 7, 15, 11, 57, 28, 982, DateTimeKind.Utc).AddTicks(1020) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 7, 15, 11, 57, 28, 982, DateTimeKind.Utc).AddTicks(1030), new DateTime(2026, 7, 15, 11, 57, 28, 982, DateTimeKind.Utc).AddTicks(1030) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 7, 15, 11, 57, 28, 982, DateTimeKind.Utc).AddTicks(1030), new DateTime(2026, 7, 15, 11, 57, 28, 982, DateTimeKind.Utc).AddTicks(1030) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 7, 15, 11, 57, 28, 981, DateTimeKind.Utc).AddTicks(6690), new DateTime(2026, 7, 15, 11, 57, 28, 981, DateTimeKind.Utc).AddTicks(6690) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "CourseCategories",
                keyColumn: "Id",
                keyValue: 1);

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
    }
}
