using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMSApi.DALLibrary.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedtimestampinEnrolls : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "AccessExpiresAt",
                table: "Enrollments",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 12, 9, 27, 20, 993, DateTimeKind.Utc).AddTicks(8660), new DateTime(2026, 6, 12, 9, 27, 20, 993, DateTimeKind.Utc).AddTicks(8660) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 12, 9, 27, 20, 993, DateTimeKind.Utc).AddTicks(8660), new DateTime(2026, 6, 12, 9, 27, 20, 993, DateTimeKind.Utc).AddTicks(8660) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 12, 9, 27, 20, 993, DateTimeKind.Utc).AddTicks(8660), new DateTime(2026, 6, 12, 9, 27, 20, 993, DateTimeKind.Utc).AddTicks(8660) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 12, 9, 27, 20, 993, DateTimeKind.Utc).AddTicks(6180), new DateTime(2026, 6, 12, 9, 27, 20, 993, DateTimeKind.Utc).AddTicks(6180) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "AccessExpiresAt",
                table: "Enrollments",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 12, 9, 8, 23, 912, DateTimeKind.Utc).AddTicks(5740), new DateTime(2026, 6, 12, 9, 8, 23, 912, DateTimeKind.Utc).AddTicks(5740) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 12, 9, 8, 23, 912, DateTimeKind.Utc).AddTicks(5740), new DateTime(2026, 6, 12, 9, 8, 23, 912, DateTimeKind.Utc).AddTicks(5740) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 12, 9, 8, 23, 912, DateTimeKind.Utc).AddTicks(5740), new DateTime(2026, 6, 12, 9, 8, 23, 912, DateTimeKind.Utc).AddTicks(5740) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 12, 9, 8, 23, 912, DateTimeKind.Utc).AddTicks(3080), new DateTime(2026, 6, 12, 9, 8, 23, 912, DateTimeKind.Utc).AddTicks(3080) });
        }
    }
}
