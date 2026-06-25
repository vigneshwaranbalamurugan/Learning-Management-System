using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMSApi.DALLibrary.Migrations
{
    /// <inheritdoc />
    public partial class UserLoginUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 24, 10, 46, 53, 467, DateTimeKind.Utc).AddTicks(7700), new DateTime(2026, 6, 24, 10, 46, 53, 467, DateTimeKind.Utc).AddTicks(7700) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 24, 10, 46, 53, 467, DateTimeKind.Utc).AddTicks(7700), new DateTime(2026, 6, 24, 10, 46, 53, 467, DateTimeKind.Utc).AddTicks(7700) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 24, 10, 46, 53, 467, DateTimeKind.Utc).AddTicks(7700), new DateTime(2026, 6, 24, 10, 46, 53, 467, DateTimeKind.Utc).AddTicks(7700) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 24, 10, 46, 53, 467, DateTimeKind.Utc).AddTicks(4400), new DateTime(2026, 6, 24, 10, 46, 53, 467, DateTimeKind.Utc).AddTicks(4400) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 24, 10, 41, 5, 164, DateTimeKind.Utc).AddTicks(8690), new DateTime(2026, 6, 24, 10, 41, 5, 164, DateTimeKind.Utc).AddTicks(8690) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 24, 10, 41, 5, 164, DateTimeKind.Utc).AddTicks(8690), new DateTime(2026, 6, 24, 10, 41, 5, 164, DateTimeKind.Utc).AddTicks(8690) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 24, 10, 41, 5, 164, DateTimeKind.Utc).AddTicks(8690), new DateTime(2026, 6, 24, 10, 41, 5, 164, DateTimeKind.Utc).AddTicks(8690) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 24, 10, 41, 5, 164, DateTimeKind.Utc).AddTicks(5250), new DateTime(2026, 6, 24, 10, 41, 5, 164, DateTimeKind.Utc).AddTicks(5250) });
        }
    }
}
