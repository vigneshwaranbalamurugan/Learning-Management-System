using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMSApi.DALLibrary.Migrations
{
    /// <inheritdoc />
    public partial class AddIsActiveToPlatformFeeConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "PlatformFeeConfigs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "PlatformFeeConfigs");

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 7, 1, 8, 59, 22, 555, DateTimeKind.Utc).AddTicks(7900), new DateTime(2026, 7, 1, 8, 59, 22, 555, DateTimeKind.Utc).AddTicks(7900) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 7, 1, 8, 59, 22, 555, DateTimeKind.Utc).AddTicks(7900), new DateTime(2026, 7, 1, 8, 59, 22, 555, DateTimeKind.Utc).AddTicks(7900) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 7, 1, 8, 59, 22, 555, DateTimeKind.Utc).AddTicks(7900), new DateTime(2026, 7, 1, 8, 59, 22, 555, DateTimeKind.Utc).AddTicks(7900) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 7, 1, 8, 59, 22, 555, DateTimeKind.Utc).AddTicks(4510), new DateTime(2026, 7, 1, 8, 59, 22, 555, DateTimeKind.Utc).AddTicks(4510) });
        }
    }
}
