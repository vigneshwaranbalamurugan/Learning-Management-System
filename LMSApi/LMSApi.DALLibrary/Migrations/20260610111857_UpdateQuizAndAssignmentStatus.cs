using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMSApi.DALLibrary.Migrations
{
    /// <inheritdoc />
    public partial class UpdateQuizAndAssignmentStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPublished",
                table: "Quizzes");

            migrationBuilder.DropColumn(
                name: "IsPublished",
                table: "Assignments");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Quizzes",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Assignments",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 10, 11, 18, 56, 779, DateTimeKind.Utc).AddTicks(5840), new DateTime(2026, 6, 10, 11, 18, 56, 779, DateTimeKind.Utc).AddTicks(5840) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 10, 11, 18, 56, 779, DateTimeKind.Utc).AddTicks(5840), new DateTime(2026, 6, 10, 11, 18, 56, 779, DateTimeKind.Utc).AddTicks(5840) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 10, 11, 18, 56, 779, DateTimeKind.Utc).AddTicks(5850), new DateTime(2026, 6, 10, 11, 18, 56, 779, DateTimeKind.Utc).AddTicks(5850) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 10, 11, 18, 56, 779, DateTimeKind.Utc).AddTicks(3320), new DateTime(2026, 6, 10, 11, 18, 56, 779, DateTimeKind.Utc).AddTicks(3320) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Quizzes");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Assignments");

            migrationBuilder.AddColumn<bool>(
                name: "IsPublished",
                table: "Quizzes",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsPublished",
                table: "Assignments",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 10, 10, 56, 34, 57, DateTimeKind.Utc).AddTicks(9210), new DateTime(2026, 6, 10, 10, 56, 34, 57, DateTimeKind.Utc).AddTicks(9210) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 10, 10, 56, 34, 57, DateTimeKind.Utc).AddTicks(9210), new DateTime(2026, 6, 10, 10, 56, 34, 57, DateTimeKind.Utc).AddTicks(9210) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 10, 10, 56, 34, 57, DateTimeKind.Utc).AddTicks(9210), new DateTime(2026, 6, 10, 10, 56, 34, 57, DateTimeKind.Utc).AddTicks(9210) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 10, 10, 56, 34, 57, DateTimeKind.Utc).AddTicks(6870), new DateTime(2026, 6, 10, 10, 56, 34, 57, DateTimeKind.Utc).AddTicks(6870) });
        }
    }
}
