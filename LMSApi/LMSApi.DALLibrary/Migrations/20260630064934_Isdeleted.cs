using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMSApi.DALLibrary.Migrations
{
    /// <inheritdoc />
    public partial class Isdeleted : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 30, 6, 49, 33, 657, DateTimeKind.Utc).AddTicks(5570), new DateTime(2026, 6, 30, 6, 49, 33, 657, DateTimeKind.Utc).AddTicks(5570) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 30, 6, 49, 33, 657, DateTimeKind.Utc).AddTicks(5580), new DateTime(2026, 6, 30, 6, 49, 33, 657, DateTimeKind.Utc).AddTicks(5580) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 30, 6, 49, 33, 657, DateTimeKind.Utc).AddTicks(5580), new DateTime(2026, 6, 30, 6, 49, 33, 657, DateTimeKind.Utc).AddTicks(5580) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 30, 6, 49, 33, 657, DateTimeKind.Utc).AddTicks(2170), new DateTime(2026, 6, 30, 6, 49, 33, 657, DateTimeKind.Utc).AddTicks(2170) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 30, 6, 27, 0, 954, DateTimeKind.Utc).AddTicks(1280), new DateTime(2026, 6, 30, 6, 27, 0, 954, DateTimeKind.Utc).AddTicks(1280) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 30, 6, 27, 0, 954, DateTimeKind.Utc).AddTicks(1280), new DateTime(2026, 6, 30, 6, 27, 0, 954, DateTimeKind.Utc).AddTicks(1280) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 30, 6, 27, 0, 954, DateTimeKind.Utc).AddTicks(1280), new DateTime(2026, 6, 30, 6, 27, 0, 954, DateTimeKind.Utc).AddTicks(1280) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 30, 6, 27, 0, 953, DateTimeKind.Utc).AddTicks(8070), new DateTime(2026, 6, 30, 6, 27, 0, 953, DateTimeKind.Utc).AddTicks(8070) });
        }
    }
}
