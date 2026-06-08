using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMSApi.DALLibrary.Migrations
{
    /// <inheritdoc />
    public partial class AssignmentDbUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AttachmentType",
                table: "Assignments",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 8, 9, 13, 28, 494, DateTimeKind.Utc).AddTicks(1880), new DateTime(2026, 6, 8, 9, 13, 28, 494, DateTimeKind.Utc).AddTicks(1880) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 8, 9, 13, 28, 494, DateTimeKind.Utc).AddTicks(1880), new DateTime(2026, 6, 8, 9, 13, 28, 494, DateTimeKind.Utc).AddTicks(1890) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 8, 9, 13, 28, 494, DateTimeKind.Utc).AddTicks(1890), new DateTime(2026, 6, 8, 9, 13, 28, 494, DateTimeKind.Utc).AddTicks(1890) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 8, 9, 13, 28, 493, DateTimeKind.Utc).AddTicks(8630), new DateTime(2026, 6, 8, 9, 13, 28, 493, DateTimeKind.Utc).AddTicks(8630) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AttachmentType",
                table: "Assignments");

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
    }
}
