using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMSApi.DALLibrary.Migrations
{
    /// <inheritdoc />
    public partial class lessonresource : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "LessonResources",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000);

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 7, 6, 16, 43, 22, 289, DateTimeKind.Utc).AddTicks(8450), new DateTime(2026, 7, 6, 16, 43, 22, 289, DateTimeKind.Utc).AddTicks(8450) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 7, 6, 16, 43, 22, 289, DateTimeKind.Utc).AddTicks(8450), new DateTime(2026, 7, 6, 16, 43, 22, 289, DateTimeKind.Utc).AddTicks(8450) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 7, 6, 16, 43, 22, 289, DateTimeKind.Utc).AddTicks(8450), new DateTime(2026, 7, 6, 16, 43, 22, 289, DateTimeKind.Utc).AddTicks(8450) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 7, 6, 16, 43, 22, 289, DateTimeKind.Utc).AddTicks(5080), new DateTime(2026, 7, 6, 16, 43, 22, 289, DateTimeKind.Utc).AddTicks(5080) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "LessonResources",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 7, 3, 17, 19, 41, 124, DateTimeKind.Utc).AddTicks(1810), new DateTime(2026, 7, 3, 17, 19, 41, 124, DateTimeKind.Utc).AddTicks(1810) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 7, 3, 17, 19, 41, 124, DateTimeKind.Utc).AddTicks(1810), new DateTime(2026, 7, 3, 17, 19, 41, 124, DateTimeKind.Utc).AddTicks(1810) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 7, 3, 17, 19, 41, 124, DateTimeKind.Utc).AddTicks(1810), new DateTime(2026, 7, 3, 17, 19, 41, 124, DateTimeKind.Utc).AddTicks(1810) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 7, 3, 17, 19, 41, 123, DateTimeKind.Utc).AddTicks(7870), new DateTime(2026, 7, 3, 17, 19, 41, 123, DateTimeKind.Utc).AddTicks(7870) });
        }
    }
}
