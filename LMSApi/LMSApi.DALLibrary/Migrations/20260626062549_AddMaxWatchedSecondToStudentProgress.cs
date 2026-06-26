using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMSApi.DALLibrary.Migrations
{
    /// <inheritdoc />
    public partial class AddMaxWatchedSecondToStudentProgress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaxWatchedSecond",
                table: "StudentProgress",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 26, 6, 25, 49, 182, DateTimeKind.Utc).AddTicks(5370), new DateTime(2026, 6, 26, 6, 25, 49, 182, DateTimeKind.Utc).AddTicks(5380) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 26, 6, 25, 49, 182, DateTimeKind.Utc).AddTicks(5380), new DateTime(2026, 6, 26, 6, 25, 49, 182, DateTimeKind.Utc).AddTicks(5380) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 26, 6, 25, 49, 182, DateTimeKind.Utc).AddTicks(5380), new DateTime(2026, 6, 26, 6, 25, 49, 182, DateTimeKind.Utc).AddTicks(5380) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 26, 6, 25, 49, 182, DateTimeKind.Utc).AddTicks(1950), new DateTime(2026, 6, 26, 6, 25, 49, 182, DateTimeKind.Utc).AddTicks(1950) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxWatchedSecond",
                table: "StudentProgress");

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 25, 6, 33, 54, 643, DateTimeKind.Utc).AddTicks(5930), new DateTime(2026, 6, 25, 6, 33, 54, 643, DateTimeKind.Utc).AddTicks(5930) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 25, 6, 33, 54, 643, DateTimeKind.Utc).AddTicks(5940), new DateTime(2026, 6, 25, 6, 33, 54, 643, DateTimeKind.Utc).AddTicks(5940) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 25, 6, 33, 54, 643, DateTimeKind.Utc).AddTicks(5940), new DateTime(2026, 6, 25, 6, 33, 54, 643, DateTimeKind.Utc).AddTicks(5940) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 25, 6, 33, 54, 643, DateTimeKind.Utc).AddTicks(1930), new DateTime(2026, 6, 25, 6, 33, 54, 643, DateTimeKind.Utc).AddTicks(1930) });
        }
    }
}
