using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMSApi.DALLibrary.Migrations
{
    /// <inheritdoc />
    public partial class AddNameLastChangedAtToUserProfiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "NameLastChangedAt",
                table: "UserProfiles",
                type: "timestamp without time zone",
                nullable: true);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NameLastChangedAt",
                table: "UserProfiles");

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 7, 7, 16, 23, 32, 401, DateTimeKind.Utc).AddTicks(8160), new DateTime(2026, 7, 7, 16, 23, 32, 401, DateTimeKind.Utc).AddTicks(8160) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 7, 7, 16, 23, 32, 401, DateTimeKind.Utc).AddTicks(8160), new DateTime(2026, 7, 7, 16, 23, 32, 401, DateTimeKind.Utc).AddTicks(8160) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 7, 7, 16, 23, 32, 401, DateTimeKind.Utc).AddTicks(8160), new DateTime(2026, 7, 7, 16, 23, 32, 401, DateTimeKind.Utc).AddTicks(8160) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 7, 7, 16, 23, 32, 401, DateTimeKind.Utc).AddTicks(4430), new DateTime(2026, 7, 7, 16, 23, 32, 401, DateTimeKind.Utc).AddTicks(4430) });
        }
    }
}
