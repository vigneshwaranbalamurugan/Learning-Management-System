using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMSApi.DALLibrary.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentBatchId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BatchId",
                table: "Payments",
                type: "integer",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 16, 11, 25, 14, 86, DateTimeKind.Utc).AddTicks(1140), new DateTime(2026, 6, 16, 11, 25, 14, 86, DateTimeKind.Utc).AddTicks(1140) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 16, 11, 25, 14, 86, DateTimeKind.Utc).AddTicks(1140), new DateTime(2026, 6, 16, 11, 25, 14, 86, DateTimeKind.Utc).AddTicks(1140) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 16, 11, 25, 14, 86, DateTimeKind.Utc).AddTicks(1140), new DateTime(2026, 6, 16, 11, 25, 14, 86, DateTimeKind.Utc).AddTicks(1140) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 16, 11, 25, 14, 85, DateTimeKind.Utc).AddTicks(8740), new DateTime(2026, 6, 16, 11, 25, 14, 85, DateTimeKind.Utc).AddTicks(8740) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BatchId",
                table: "Payments");

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
    }
}
