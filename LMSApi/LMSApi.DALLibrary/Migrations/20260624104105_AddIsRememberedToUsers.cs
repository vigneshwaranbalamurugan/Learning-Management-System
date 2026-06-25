using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMSApi.DALLibrary.Migrations
{
    /// <inheritdoc />
    public partial class AddIsRememberedToUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsRemembered",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

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
                columns: new[] { "CreatedAt", "IsRemembered", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 24, 10, 41, 5, 164, DateTimeKind.Utc).AddTicks(5250), false, new DateTime(2026, 6, 24, 10, 41, 5, 164, DateTimeKind.Utc).AddTicks(5250) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsRemembered",
                table: "Users");

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 23, 10, 35, 33, 134, DateTimeKind.Utc).AddTicks(2420), new DateTime(2026, 6, 23, 10, 35, 33, 134, DateTimeKind.Utc).AddTicks(2420) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 23, 10, 35, 33, 134, DateTimeKind.Utc).AddTicks(2430), new DateTime(2026, 6, 23, 10, 35, 33, 134, DateTimeKind.Utc).AddTicks(2430) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 23, 10, 35, 33, 134, DateTimeKind.Utc).AddTicks(2430), new DateTime(2026, 6, 23, 10, 35, 33, 134, DateTimeKind.Utc).AddTicks(2430) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 23, 10, 35, 33, 133, DateTimeKind.Utc).AddTicks(9020), new DateTime(2026, 6, 23, 10, 35, 33, 133, DateTimeKind.Utc).AddTicks(9030) });
        }
    }
}
