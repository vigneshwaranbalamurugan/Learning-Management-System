using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMSApi.DALLibrary.Migrations
{
    /// <inheritdoc />
    public partial class AddRefreshTokenToUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RefreshToken",
                table: "Users",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RefreshTokenExpiryTime",
                table: "Users",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 20, 6, 12, 26, 77, DateTimeKind.Utc).AddTicks(2330), new DateTime(2026, 6, 20, 6, 12, 26, 77, DateTimeKind.Utc).AddTicks(2330) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 20, 6, 12, 26, 77, DateTimeKind.Utc).AddTicks(2330), new DateTime(2026, 6, 20, 6, 12, 26, 77, DateTimeKind.Utc).AddTicks(2330) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 20, 6, 12, 26, 77, DateTimeKind.Utc).AddTicks(2330), new DateTime(2026, 6, 20, 6, 12, 26, 77, DateTimeKind.Utc).AddTicks(2330) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "RefreshToken", "RefreshTokenExpiryTime", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 20, 6, 12, 26, 76, DateTimeKind.Utc).AddTicks(8850), null, null, new DateTime(2026, 6, 20, 6, 12, 26, 76, DateTimeKind.Utc).AddTicks(8850) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RefreshToken",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "RefreshTokenExpiryTime",
                table: "Users");

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 19, 7, 29, 7, 2, DateTimeKind.Utc).AddTicks(9930), new DateTime(2026, 6, 19, 7, 29, 7, 2, DateTimeKind.Utc).AddTicks(9930) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 19, 7, 29, 7, 2, DateTimeKind.Utc).AddTicks(9930), new DateTime(2026, 6, 19, 7, 29, 7, 2, DateTimeKind.Utc).AddTicks(9930) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 19, 7, 29, 7, 2, DateTimeKind.Utc).AddTicks(9940), new DateTime(2026, 6, 19, 7, 29, 7, 2, DateTimeKind.Utc).AddTicks(9940) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 19, 7, 29, 7, 2, DateTimeKind.Utc).AddTicks(5820), new DateTime(2026, 6, 19, 7, 29, 7, 2, DateTimeKind.Utc).AddTicks(5820) });
        }
    }
}
