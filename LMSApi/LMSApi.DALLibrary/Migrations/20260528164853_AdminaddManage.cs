using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMSApi.DALLibrary.Migrations
{
    /// <inheritdoc />
    public partial class AdminaddManage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 5, 28, 16, 48, 53, 503, DateTimeKind.Utc).AddTicks(50), new DateTime(2026, 5, 28, 16, 48, 53, 503, DateTimeKind.Utc).AddTicks(50) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 5, 28, 16, 48, 53, 503, DateTimeKind.Utc).AddTicks(50), new DateTime(2026, 5, 28, 16, 48, 53, 503, DateTimeKind.Utc).AddTicks(50) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 5, 28, 16, 48, 53, 503, DateTimeKind.Utc).AddTicks(50), new DateTime(2026, 5, 28, 16, 48, 53, 503, DateTimeKind.Utc).AddTicks(50) });

            migrationBuilder.InsertData(
                table: "UserRoles",
                columns: new[] { "Id", "CreatedAt", "Description", "RoleName", "UpdatedAt" },
                values: new object[] { 4, new DateTime(2026, 5, 28, 16, 48, 53, 503, DateTimeKind.Utc).AddTicks(60), "SuperAdmin account", "SuperAdmin", new DateTime(2026, 5, 28, 16, 48, 53, 503, DateTimeKind.Utc).AddTicks(60) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 5, 28, 16, 48, 53, 502, DateTimeKind.Utc).AddTicks(6860), new DateTime(2026, 5, 28, 16, 48, 53, 502, DateTimeKind.Utc).AddTicks(6860) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 5, 28, 11, 26, 40, 376, DateTimeKind.Utc).AddTicks(4470), new DateTime(2026, 5, 28, 11, 26, 40, 376, DateTimeKind.Utc).AddTicks(4470) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 5, 28, 11, 26, 40, 376, DateTimeKind.Utc).AddTicks(4480), new DateTime(2026, 5, 28, 11, 26, 40, 376, DateTimeKind.Utc).AddTicks(4480) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 5, 28, 11, 26, 40, 376, DateTimeKind.Utc).AddTicks(4480), new DateTime(2026, 5, 28, 11, 26, 40, 376, DateTimeKind.Utc).AddTicks(4480) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 5, 28, 11, 26, 40, 376, DateTimeKind.Utc).AddTicks(7430), new DateTime(2026, 5, 28, 11, 26, 40, 376, DateTimeKind.Utc).AddTicks(7430) });
        }
    }
}
