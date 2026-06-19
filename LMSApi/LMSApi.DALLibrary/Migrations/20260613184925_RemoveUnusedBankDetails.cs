using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMSApi.DALLibrary.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUnusedBankDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccountHolderName",
                table: "InstructorPayoutAccounts");

            migrationBuilder.DropColumn(
                name: "AccountNumber",
                table: "InstructorPayoutAccounts");

            migrationBuilder.DropColumn(
                name: "BankName",
                table: "InstructorPayoutAccounts");

            migrationBuilder.DropColumn(
                name: "IfscCode",
                table: "InstructorPayoutAccounts");

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 13, 18, 49, 25, 545, DateTimeKind.Utc).AddTicks(9350), new DateTime(2026, 6, 13, 18, 49, 25, 545, DateTimeKind.Utc).AddTicks(9350) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 13, 18, 49, 25, 545, DateTimeKind.Utc).AddTicks(9350), new DateTime(2026, 6, 13, 18, 49, 25, 545, DateTimeKind.Utc).AddTicks(9350) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 13, 18, 49, 25, 545, DateTimeKind.Utc).AddTicks(9360), new DateTime(2026, 6, 13, 18, 49, 25, 545, DateTimeKind.Utc).AddTicks(9360) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 13, 18, 49, 25, 545, DateTimeKind.Utc).AddTicks(5860), new DateTime(2026, 6, 13, 18, 49, 25, 545, DateTimeKind.Utc).AddTicks(5860) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AccountHolderName",
                table: "InstructorPayoutAccounts",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AccountNumber",
                table: "InstructorPayoutAccounts",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BankName",
                table: "InstructorPayoutAccounts",
                type: "character varying(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "IfscCode",
                table: "InstructorPayoutAccounts",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 13, 18, 36, 1, 884, DateTimeKind.Utc).AddTicks(3850), new DateTime(2026, 6, 13, 18, 36, 1, 884, DateTimeKind.Utc).AddTicks(3850) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 13, 18, 36, 1, 884, DateTimeKind.Utc).AddTicks(3850), new DateTime(2026, 6, 13, 18, 36, 1, 884, DateTimeKind.Utc).AddTicks(3850) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 13, 18, 36, 1, 884, DateTimeKind.Utc).AddTicks(3850), new DateTime(2026, 6, 13, 18, 36, 1, 884, DateTimeKind.Utc).AddTicks(3850) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 13, 18, 36, 1, 884, DateTimeKind.Utc).AddTicks(490), new DateTime(2026, 6, 13, 18, 36, 1, 884, DateTimeKind.Utc).AddTicks(490) });
        }
    }
}
