using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMSApi.DALLibrary.Migrations
{
    /// <inheritdoc />
    public partial class Admin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 2, 11, 26, 40, 751, DateTimeKind.Utc).AddTicks(3620), new DateTime(2026, 6, 2, 11, 26, 40, 751, DateTimeKind.Utc).AddTicks(3620) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 2, 11, 26, 40, 751, DateTimeKind.Utc).AddTicks(3620), new DateTime(2026, 6, 2, 11, 26, 40, 751, DateTimeKind.Utc).AddTicks(3620) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 2, 11, 26, 40, 751, DateTimeKind.Utc).AddTicks(3620), new DateTime(2026, 6, 2, 11, 26, 40, 751, DateTimeKind.Utc).AddTicks(3620) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "Email", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 2, 11, 26, 40, 750, DateTimeKind.Utc).AddTicks(9980), "admin@gmail.com", new DateTime(2026, 6, 2, 11, 26, 40, 751, DateTimeKind.Utc).AddTicks(20) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 2, 11, 6, 19, 927, DateTimeKind.Utc).AddTicks(8990), new DateTime(2026, 6, 2, 11, 6, 19, 927, DateTimeKind.Utc).AddTicks(8990) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 2, 11, 6, 19, 927, DateTimeKind.Utc).AddTicks(8990), new DateTime(2026, 6, 2, 11, 6, 19, 927, DateTimeKind.Utc).AddTicks(8990) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 2, 11, 6, 19, 927, DateTimeKind.Utc).AddTicks(8990), new DateTime(2026, 6, 2, 11, 6, 19, 927, DateTimeKind.Utc).AddTicks(8990) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "Email", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 2, 11, 6, 19, 927, DateTimeKind.Utc).AddTicks(5370), "Admin@gmail.com", new DateTime(2026, 6, 2, 11, 6, 19, 927, DateTimeKind.Utc).AddTicks(5370) });
        }
    }
}
