using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMSApi.DALLibrary.Migrations
{
    /// <inheritdoc />
    public partial class CoursesectionOrderupdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                table: "CourseSections",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 4, 7, 21, 9, 403, DateTimeKind.Utc).AddTicks(1120), new DateTime(2026, 6, 4, 7, 21, 9, 403, DateTimeKind.Utc).AddTicks(1120) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 4, 7, 21, 9, 403, DateTimeKind.Utc).AddTicks(1120), new DateTime(2026, 6, 4, 7, 21, 9, 403, DateTimeKind.Utc).AddTicks(1120) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 4, 7, 21, 9, 403, DateTimeKind.Utc).AddTicks(1120), new DateTime(2026, 6, 4, 7, 21, 9, 403, DateTimeKind.Utc).AddTicks(1120) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 4, 7, 21, 9, 402, DateTimeKind.Utc).AddTicks(7650), new DateTime(2026, 6, 4, 7, 21, 9, 402, DateTimeKind.Utc).AddTicks(7650) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SortOrder",
                table: "CourseSections");

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 4, 7, 0, 38, 918, DateTimeKind.Utc).AddTicks(5720), new DateTime(2026, 6, 4, 7, 0, 38, 918, DateTimeKind.Utc).AddTicks(5720) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 4, 7, 0, 38, 918, DateTimeKind.Utc).AddTicks(5720), new DateTime(2026, 6, 4, 7, 0, 38, 918, DateTimeKind.Utc).AddTicks(5720) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 4, 7, 0, 38, 918, DateTimeKind.Utc).AddTicks(5720), new DateTime(2026, 6, 4, 7, 0, 38, 918, DateTimeKind.Utc).AddTicks(5720) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 4, 7, 0, 38, 918, DateTimeKind.Utc).AddTicks(2420), new DateTime(2026, 6, 4, 7, 0, 38, 918, DateTimeKind.Utc).AddTicks(2420) });
        }
    }
}
