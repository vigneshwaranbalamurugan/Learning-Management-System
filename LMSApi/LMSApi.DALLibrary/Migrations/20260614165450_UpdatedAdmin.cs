using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMSApi.DALLibrary.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedAdmin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 14, 16, 54, 49, 968, DateTimeKind.Utc).AddTicks(8230), new DateTime(2026, 6, 14, 16, 54, 49, 968, DateTimeKind.Utc).AddTicks(8230) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 14, 16, 54, 49, 968, DateTimeKind.Utc).AddTicks(8230), new DateTime(2026, 6, 14, 16, 54, 49, 968, DateTimeKind.Utc).AddTicks(8230) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 14, 16, 54, 49, 968, DateTimeKind.Utc).AddTicks(8230), new DateTime(2026, 6, 14, 16, 54, 49, 968, DateTimeKind.Utc).AddTicks(8230) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "PasswordSalt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 14, 16, 54, 49, 968, DateTimeKind.Utc).AddTicks(4910), "K5CamRmhDuuJEyr50OpNsA==", new DateTime(2026, 6, 14, 16, 54, 49, 968, DateTimeKind.Utc).AddTicks(4910) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 14, 16, 4, 32, 258, DateTimeKind.Utc).AddTicks(2470), new DateTime(2026, 6, 14, 16, 4, 32, 258, DateTimeKind.Utc).AddTicks(2470) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 14, 16, 4, 32, 258, DateTimeKind.Utc).AddTicks(2470), new DateTime(2026, 6, 14, 16, 4, 32, 258, DateTimeKind.Utc).AddTicks(2470) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 14, 16, 4, 32, 258, DateTimeKind.Utc).AddTicks(2470), new DateTime(2026, 6, 14, 16, 4, 32, 258, DateTimeKind.Utc).AddTicks(2470) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "PasswordSalt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 14, 16, 4, 32, 258, DateTimeKind.Utc).AddTicks(310), null, new DateTime(2026, 6, 14, 16, 4, 32, 258, DateTimeKind.Utc).AddTicks(310) });
        }
    }
}
