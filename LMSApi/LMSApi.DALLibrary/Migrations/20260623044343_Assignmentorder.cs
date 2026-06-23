using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMSApi.DALLibrary.Migrations
{
    /// <inheritdoc />
    public partial class Assignmentorder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                table: "Assignments",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 23, 4, 43, 43, 295, DateTimeKind.Utc).AddTicks(8430), new DateTime(2026, 6, 23, 4, 43, 43, 295, DateTimeKind.Utc).AddTicks(8430) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 23, 4, 43, 43, 295, DateTimeKind.Utc).AddTicks(8430), new DateTime(2026, 6, 23, 4, 43, 43, 295, DateTimeKind.Utc).AddTicks(8430) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 23, 4, 43, 43, 295, DateTimeKind.Utc).AddTicks(8430), new DateTime(2026, 6, 23, 4, 43, 43, 295, DateTimeKind.Utc).AddTicks(8430) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 23, 4, 43, 43, 295, DateTimeKind.Utc).AddTicks(5120), new DateTime(2026, 6, 23, 4, 43, 43, 295, DateTimeKind.Utc).AddTicks(5120) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SortOrder",
                table: "Assignments");

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 20, 6, 12, 26, 77, DateTimeKind.Utc).AddTicks(2330), new DateTime(2026, 6, 20, 6, 12, 26, 77, DateTimeKind.Utc).AddTicks(2330) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 20, 6, 12, 26, 77, DateTimeKind.Utc).AddTicks(2330), new DateTime(2026, 6, 20, 6, 12, 26, 77, DateTimeKind.Utc).AddTicks(2330) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 20, 6, 12, 26, 77, DateTimeKind.Utc).AddTicks(2330), new DateTime(2026, 6, 20, 6, 12, 26, 77, DateTimeKind.Utc).AddTicks(2330) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 20, 6, 12, 26, 76, DateTimeKind.Utc).AddTicks(8850), new DateTime(2026, 6, 20, 6, 12, 26, 76, DateTimeKind.Utc).AddTicks(8850) });
        }
    }
}
