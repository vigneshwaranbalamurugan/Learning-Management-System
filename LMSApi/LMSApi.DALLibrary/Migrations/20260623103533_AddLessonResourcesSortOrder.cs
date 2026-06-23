using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMSApi.DALLibrary.Migrations
{
    /// <inheritdoc />
    public partial class AddLessonResourcesSortOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "SortOrder",
                table: "LessonResources",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 0);

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 23, 10, 35, 33, 134, DateTimeKind.Utc).AddTicks(2420), new DateTime(2026, 6, 23, 10, 35, 33, 134, DateTimeKind.Utc).AddTicks(2420) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 23, 10, 35, 33, 134, DateTimeKind.Utc).AddTicks(2430), new DateTime(2026, 6, 23, 10, 35, 33, 134, DateTimeKind.Utc).AddTicks(2430) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 23, 10, 35, 33, 134, DateTimeKind.Utc).AddTicks(2430), new DateTime(2026, 6, 23, 10, 35, 33, 134, DateTimeKind.Utc).AddTicks(2430) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 23, 10, 35, 33, 133, DateTimeKind.Utc).AddTicks(9020), new DateTime(2026, 6, 23, 10, 35, 33, 133, DateTimeKind.Utc).AddTicks(9030) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "SortOrder",
                table: "LessonResources",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 23, 9, 54, 3, 345, DateTimeKind.Utc).AddTicks(2880), new DateTime(2026, 6, 23, 9, 54, 3, 345, DateTimeKind.Utc).AddTicks(2880) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 23, 9, 54, 3, 345, DateTimeKind.Utc).AddTicks(2880), new DateTime(2026, 6, 23, 9, 54, 3, 345, DateTimeKind.Utc).AddTicks(2880) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 23, 9, 54, 3, 345, DateTimeKind.Utc).AddTicks(2880), new DateTime(2026, 6, 23, 9, 54, 3, 345, DateTimeKind.Utc).AddTicks(2880) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 23, 9, 54, 3, 344, DateTimeKind.Utc).AddTicks(9030), new DateTime(2026, 6, 23, 9, 54, 3, 344, DateTimeKind.Utc).AddTicks(9030) });
        }
    }
}
