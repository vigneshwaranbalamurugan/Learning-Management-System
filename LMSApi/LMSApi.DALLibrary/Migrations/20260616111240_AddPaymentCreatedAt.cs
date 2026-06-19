using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMSApi.DALLibrary.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentCreatedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Payments",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 16, 11, 12, 40, 241, DateTimeKind.Utc).AddTicks(290), new DateTime(2026, 6, 16, 11, 12, 40, 241, DateTimeKind.Utc).AddTicks(300) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 16, 11, 12, 40, 241, DateTimeKind.Utc).AddTicks(300), new DateTime(2026, 6, 16, 11, 12, 40, 241, DateTimeKind.Utc).AddTicks(300) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 16, 11, 12, 40, 241, DateTimeKind.Utc).AddTicks(300), new DateTime(2026, 6, 16, 11, 12, 40, 241, DateTimeKind.Utc).AddTicks(300) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 16, 11, 12, 40, 240, DateTimeKind.Utc).AddTicks(7970), new DateTime(2026, 6, 16, 11, 12, 40, 240, DateTimeKind.Utc).AddTicks(7970) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Payments");

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
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 14, 16, 54, 49, 968, DateTimeKind.Utc).AddTicks(4910), new DateTime(2026, 6, 14, 16, 54, 49, 968, DateTimeKind.Utc).AddTicks(4910) });
        }
    }
}
