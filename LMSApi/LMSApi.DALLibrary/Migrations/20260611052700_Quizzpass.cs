using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMSApi.DALLibrary.Migrations
{
    /// <inheritdoc />
    public partial class Quizzpass : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 11, 5, 27, 0, 366, DateTimeKind.Utc).AddTicks(5940), new DateTime(2026, 6, 11, 5, 27, 0, 366, DateTimeKind.Utc).AddTicks(5940) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 11, 5, 27, 0, 366, DateTimeKind.Utc).AddTicks(5940), new DateTime(2026, 6, 11, 5, 27, 0, 366, DateTimeKind.Utc).AddTicks(5940) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 11, 5, 27, 0, 366, DateTimeKind.Utc).AddTicks(5940), new DateTime(2026, 6, 11, 5, 27, 0, 366, DateTimeKind.Utc).AddTicks(5940) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 11, 5, 27, 0, 366, DateTimeKind.Utc).AddTicks(3340), new DateTime(2026, 6, 11, 5, 27, 0, 366, DateTimeKind.Utc).AddTicks(3340) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 11, 5, 22, 28, 925, DateTimeKind.Utc).AddTicks(3660), new DateTime(2026, 6, 11, 5, 22, 28, 925, DateTimeKind.Utc).AddTicks(3660) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 11, 5, 22, 28, 925, DateTimeKind.Utc).AddTicks(3670), new DateTime(2026, 6, 11, 5, 22, 28, 925, DateTimeKind.Utc).AddTicks(3670) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 11, 5, 22, 28, 925, DateTimeKind.Utc).AddTicks(3670), new DateTime(2026, 6, 11, 5, 22, 28, 925, DateTimeKind.Utc).AddTicks(3670) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 11, 5, 22, 28, 925, DateTimeKind.Utc).AddTicks(1010), new DateTime(2026, 6, 11, 5, 22, 28, 925, DateTimeKind.Utc).AddTicks(1010) });
        }
    }
}
