using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMSApi.DALLibrary.Migrations
{
    /// <inheritdoc />
    public partial class MakePanNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Pan",
                table: "InstructorLinkedAccounts",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(10)",
                oldMaxLength: 10);

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 19, 7, 29, 7, 2, DateTimeKind.Utc).AddTicks(9930), new DateTime(2026, 6, 19, 7, 29, 7, 2, DateTimeKind.Utc).AddTicks(9930) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 19, 7, 29, 7, 2, DateTimeKind.Utc).AddTicks(9930), new DateTime(2026, 6, 19, 7, 29, 7, 2, DateTimeKind.Utc).AddTicks(9930) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 19, 7, 29, 7, 2, DateTimeKind.Utc).AddTicks(9940), new DateTime(2026, 6, 19, 7, 29, 7, 2, DateTimeKind.Utc).AddTicks(9940) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 19, 7, 29, 7, 2, DateTimeKind.Utc).AddTicks(5820), new DateTime(2026, 6, 19, 7, 29, 7, 2, DateTimeKind.Utc).AddTicks(5820) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Pan",
                table: "InstructorLinkedAccounts",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(10)",
                oldMaxLength: 10,
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 19, 6, 40, 58, 334, DateTimeKind.Utc).AddTicks(9880), new DateTime(2026, 6, 19, 6, 40, 58, 334, DateTimeKind.Utc).AddTicks(9880) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 19, 6, 40, 58, 334, DateTimeKind.Utc).AddTicks(9890), new DateTime(2026, 6, 19, 6, 40, 58, 334, DateTimeKind.Utc).AddTicks(9890) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 19, 6, 40, 58, 334, DateTimeKind.Utc).AddTicks(9890), new DateTime(2026, 6, 19, 6, 40, 58, 334, DateTimeKind.Utc).AddTicks(9890) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 19, 6, 40, 58, 334, DateTimeKind.Utc).AddTicks(6570), new DateTime(2026, 6, 19, 6, 40, 58, 334, DateTimeKind.Utc).AddTicks(6570) });
        }
    }
}
