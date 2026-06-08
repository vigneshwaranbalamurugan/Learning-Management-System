using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMSApi.DALLibrary.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedPublish : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPublished",
                table: "Courses",
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
                values: new object[] { new DateTime(2026, 6, 6, 17, 21, 58, 601, DateTimeKind.Utc).AddTicks(6620), new DateTime(2026, 6, 6, 17, 21, 58, 601, DateTimeKind.Utc).AddTicks(6620) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 6, 17, 21, 58, 601, DateTimeKind.Utc).AddTicks(6620), new DateTime(2026, 6, 6, 17, 21, 58, 601, DateTimeKind.Utc).AddTicks(6620) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 6, 17, 21, 58, 601, DateTimeKind.Utc).AddTicks(6620), new DateTime(2026, 6, 6, 17, 21, 58, 601, DateTimeKind.Utc).AddTicks(6620) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 6, 17, 21, 58, 601, DateTimeKind.Utc).AddTicks(3470), new DateTime(2026, 6, 6, 17, 21, 58, 601, DateTimeKind.Utc).AddTicks(3470) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPublished",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "IsPublished",
                table: "Assignments");

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 6, 16, 59, 17, 516, DateTimeKind.Utc).AddTicks(4610), new DateTime(2026, 6, 6, 16, 59, 17, 516, DateTimeKind.Utc).AddTicks(4610) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 6, 16, 59, 17, 516, DateTimeKind.Utc).AddTicks(4610), new DateTime(2026, 6, 6, 16, 59, 17, 516, DateTimeKind.Utc).AddTicks(4620) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 6, 16, 59, 17, 516, DateTimeKind.Utc).AddTicks(4620), new DateTime(2026, 6, 6, 16, 59, 17, 516, DateTimeKind.Utc).AddTicks(4620) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 6, 16, 59, 17, 516, DateTimeKind.Utc).AddTicks(1270), new DateTime(2026, 6, 6, 16, 59, 17, 516, DateTimeKind.Utc).AddTicks(1270) });
        }
    }
}
