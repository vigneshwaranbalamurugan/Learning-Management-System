using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMSApi.DALLibrary.Migrations
{
    /// <inheritdoc />
    public partial class Coursesectionupdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxAttempts",
                table: "CourseSections");

            migrationBuilder.DropColumn(
                name: "PassingMarks",
                table: "CourseSections");

            migrationBuilder.DropColumn(
                name: "TimeLimitMinutes",
                table: "CourseSections");

            migrationBuilder.DropColumn(
                name: "TotalMarks",
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaxAttempts",
                table: "CourseSections",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "PassingMarks",
                table: "CourseSections",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TimeLimitMinutes",
                table: "CourseSections",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalMarks",
                table: "CourseSections",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 4, 6, 14, 9, 389, DateTimeKind.Utc).AddTicks(7110), new DateTime(2026, 6, 4, 6, 14, 9, 389, DateTimeKind.Utc).AddTicks(7110) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 4, 6, 14, 9, 389, DateTimeKind.Utc).AddTicks(7110), new DateTime(2026, 6, 4, 6, 14, 9, 389, DateTimeKind.Utc).AddTicks(7110) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 4, 6, 14, 9, 389, DateTimeKind.Utc).AddTicks(7110), new DateTime(2026, 6, 4, 6, 14, 9, 389, DateTimeKind.Utc).AddTicks(7110) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 4, 6, 14, 9, 389, DateTimeKind.Utc).AddTicks(2970), new DateTime(2026, 6, 4, 6, 14, 9, 389, DateTimeKind.Utc).AddTicks(2970) });
        }
    }
}
