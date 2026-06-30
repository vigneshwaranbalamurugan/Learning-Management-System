using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMSApi.DALLibrary.Migrations
{
    /// <inheritdoc />
    public partial class Isdeleteds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Requirements",
                table: "Courses",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LearningOutcomes",
                table: "Courses",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000,
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Requirements",
                table: "Courses",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LearningOutcomes",
                table: "Courses",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000,
                oldNullable: true);

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
    }
}
