using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMSApi.DALLibrary.Migrations
{
    /// <inheritdoc />
    public partial class Quizztimestamp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "AvailableUntil",
                table: "Quizzes",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "AvailableFrom",
                table: "Quizzes",
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
                values: new object[] { new DateTime(2026, 6, 5, 4, 46, 42, 329, DateTimeKind.Utc).AddTicks(800), new DateTime(2026, 6, 5, 4, 46, 42, 329, DateTimeKind.Utc).AddTicks(800) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 5, 4, 46, 42, 329, DateTimeKind.Utc).AddTicks(810), new DateTime(2026, 6, 5, 4, 46, 42, 329, DateTimeKind.Utc).AddTicks(810) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 5, 4, 46, 42, 329, DateTimeKind.Utc).AddTicks(810), new DateTime(2026, 6, 5, 4, 46, 42, 329, DateTimeKind.Utc).AddTicks(810) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 5, 4, 46, 42, 328, DateTimeKind.Utc).AddTicks(7610), new DateTime(2026, 6, 5, 4, 46, 42, 328, DateTimeKind.Utc).AddTicks(7610) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "AvailableUntil",
                table: "Quizzes",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "AvailableFrom",
                table: "Quizzes",
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
                values: new object[] { new DateTime(2026, 6, 5, 4, 43, 24, 89, DateTimeKind.Utc).AddTicks(5410), new DateTime(2026, 6, 5, 4, 43, 24, 89, DateTimeKind.Utc).AddTicks(5410) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 5, 4, 43, 24, 89, DateTimeKind.Utc).AddTicks(5410), new DateTime(2026, 6, 5, 4, 43, 24, 89, DateTimeKind.Utc).AddTicks(5410) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 5, 4, 43, 24, 89, DateTimeKind.Utc).AddTicks(5410), new DateTime(2026, 6, 5, 4, 43, 24, 89, DateTimeKind.Utc).AddTicks(5410) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 5, 4, 43, 24, 88, DateTimeKind.Utc).AddTicks(9190), new DateTime(2026, 6, 5, 4, 43, 24, 88, DateTimeKind.Utc).AddTicks(9190) });
        }
    }
}
