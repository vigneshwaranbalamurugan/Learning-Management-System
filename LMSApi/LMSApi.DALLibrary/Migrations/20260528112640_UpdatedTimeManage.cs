using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMSApi.DALLibrary.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedTimeManage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 5, 28, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 5, 28, 0, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 5, 28, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 5, 28, 0, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 5, 28, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 5, 28, 0, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 5, 28, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 5, 28, 0, 0, 0, 0, DateTimeKind.Utc) });
        }
    }
}
