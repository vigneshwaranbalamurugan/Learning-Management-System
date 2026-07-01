using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMSApi.DALLibrary.Migrations
{
    /// <inheritdoc />
    public partial class Payout : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "InstructorPayoutAccountId",
                table: "InstructorPayouts",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 7, 1, 8, 59, 22, 555, DateTimeKind.Utc).AddTicks(7900), new DateTime(2026, 7, 1, 8, 59, 22, 555, DateTimeKind.Utc).AddTicks(7900) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 7, 1, 8, 59, 22, 555, DateTimeKind.Utc).AddTicks(7900), new DateTime(2026, 7, 1, 8, 59, 22, 555, DateTimeKind.Utc).AddTicks(7900) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 7, 1, 8, 59, 22, 555, DateTimeKind.Utc).AddTicks(7900), new DateTime(2026, 7, 1, 8, 59, 22, 555, DateTimeKind.Utc).AddTicks(7900) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 7, 1, 8, 59, 22, 555, DateTimeKind.Utc).AddTicks(4510), new DateTime(2026, 7, 1, 8, 59, 22, 555, DateTimeKind.Utc).AddTicks(4510) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "InstructorPayoutAccountId",
                table: "InstructorPayouts",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 30, 6, 52, 9, 804, DateTimeKind.Utc).AddTicks(2210), new DateTime(2026, 6, 30, 6, 52, 9, 804, DateTimeKind.Utc).AddTicks(2210) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 30, 6, 52, 9, 804, DateTimeKind.Utc).AddTicks(2210), new DateTime(2026, 6, 30, 6, 52, 9, 804, DateTimeKind.Utc).AddTicks(2210) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 30, 6, 52, 9, 804, DateTimeKind.Utc).AddTicks(2210), new DateTime(2026, 6, 30, 6, 52, 9, 804, DateTimeKind.Utc).AddTicks(2210) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 30, 6, 52, 9, 803, DateTimeKind.Utc).AddTicks(8570), new DateTime(2026, 6, 30, 6, 52, 9, 803, DateTimeKind.Utc).AddTicks(8570) });
        }
    }
}
