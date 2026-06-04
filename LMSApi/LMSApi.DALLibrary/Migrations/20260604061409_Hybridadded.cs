using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMSApi.DALLibrary.Migrations
{
    /// <inheritdoc />
    public partial class Hybridadded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 4, 6, 14, 9, 389, DateTimeKind.Utc).AddTicks(7110), new DateTime(2026, 6, 4, 6, 14, 9, 389, DateTimeKind.Utc).AddTicks(7110) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 4, 6, 14, 9, 389, DateTimeKind.Utc).AddTicks(7110), new DateTime(2026, 6, 4, 6, 14, 9, 389, DateTimeKind.Utc).AddTicks(7110) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 4, 6, 14, 9, 389, DateTimeKind.Utc).AddTicks(7110), new DateTime(2026, 6, 4, 6, 14, 9, 389, DateTimeKind.Utc).AddTicks(7110) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 4, 6, 14, 9, 389, DateTimeKind.Utc).AddTicks(2970), new DateTime(2026, 6, 4, 6, 14, 9, 389, DateTimeKind.Utc).AddTicks(2970) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 4, 5, 3, 58, 996, DateTimeKind.Utc).AddTicks(9500), new DateTime(2026, 6, 4, 5, 3, 58, 996, DateTimeKind.Utc).AddTicks(9500) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 4, 5, 3, 58, 996, DateTimeKind.Utc).AddTicks(9500), new DateTime(2026, 6, 4, 5, 3, 58, 996, DateTimeKind.Utc).AddTicks(9500) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 4, 5, 3, 58, 996, DateTimeKind.Utc).AddTicks(9500), new DateTime(2026, 6, 4, 5, 3, 58, 996, DateTimeKind.Utc).AddTicks(9500) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 4, 5, 3, 58, 996, DateTimeKind.Utc).AddTicks(6250), new DateTime(2026, 6, 4, 5, 3, 58, 996, DateTimeKind.Utc).AddTicks(6250) });
        }
    }
}
