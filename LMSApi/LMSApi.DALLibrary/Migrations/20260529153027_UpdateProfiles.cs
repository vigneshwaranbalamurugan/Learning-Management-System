using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMSApi.DALLibrary.Migrations
{
    /// <inheritdoc />
    public partial class UpdateProfiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.AlterColumn<DateOnly>(
                name: "DateOfBirth",
                table: "UserProfiles",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "UserProfiles",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "UserProfiles",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 5, 29, 15, 30, 27, 534, DateTimeKind.Utc).AddTicks(2280), new DateTime(2026, 5, 29, 15, 30, 27, 534, DateTimeKind.Utc).AddTicks(2280) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 5, 29, 15, 30, 27, 534, DateTimeKind.Utc).AddTicks(2280), new DateTime(2026, 5, 29, 15, 30, 27, 534, DateTimeKind.Utc).AddTicks(2280) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 5, 29, 15, 30, 27, 534, DateTimeKind.Utc).AddTicks(2280), new DateTime(2026, 5, 29, 15, 30, 27, 534, DateTimeKind.Utc).AddTicks(2280) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 5, 29, 15, 30, 27, 533, DateTimeKind.Utc).AddTicks(8590), new DateTime(2026, 5, 29, 15, 30, 27, 533, DateTimeKind.Utc).AddTicks(8600) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "UserProfiles");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateOfBirth",
                table: "UserProfiles",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 5, 28, 16, 48, 53, 503, DateTimeKind.Utc).AddTicks(50), new DateTime(2026, 5, 28, 16, 48, 53, 503, DateTimeKind.Utc).AddTicks(50) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 5, 28, 16, 48, 53, 503, DateTimeKind.Utc).AddTicks(50), new DateTime(2026, 5, 28, 16, 48, 53, 503, DateTimeKind.Utc).AddTicks(50) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 5, 28, 16, 48, 53, 503, DateTimeKind.Utc).AddTicks(50), new DateTime(2026, 5, 28, 16, 48, 53, 503, DateTimeKind.Utc).AddTicks(50) });

            migrationBuilder.InsertData(
                table: "UserRoles",
                columns: new[] { "Id", "CreatedAt", "Description", "RoleName", "UpdatedAt" },
                values: new object[] { 4, new DateTime(2026, 5, 28, 16, 48, 53, 503, DateTimeKind.Utc).AddTicks(60), "SuperAdmin account", "SuperAdmin", new DateTime(2026, 5, 28, 16, 48, 53, 503, DateTimeKind.Utc).AddTicks(60) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 5, 28, 16, 48, 53, 502, DateTimeKind.Utc).AddTicks(6860), new DateTime(2026, 5, 28, 16, 48, 53, 502, DateTimeKind.Utc).AddTicks(6860) });
        }
    }
}
