using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMSApi.DALLibrary.Migrations
{
    /// <inheritdoc />
    public partial class AddRazorpayAccountDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AccountNumber",
                table: "InstructorPayoutAccounts",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ContactName",
                table: "InstructorPayoutAccounts",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Email",
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

            migrationBuilder.AddColumn<string>(
                name: "LegalBusinessName",
                table: "InstructorPayoutAccounts",
                type: "character varying(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Phone",
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
                values: new object[] { new DateTime(2026, 6, 13, 19, 6, 59, 177, DateTimeKind.Utc).AddTicks(7110), new DateTime(2026, 6, 13, 19, 6, 59, 177, DateTimeKind.Utc).AddTicks(7110) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 13, 19, 6, 59, 177, DateTimeKind.Utc).AddTicks(7110), new DateTime(2026, 6, 13, 19, 6, 59, 177, DateTimeKind.Utc).AddTicks(7110) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 13, 19, 6, 59, 177, DateTimeKind.Utc).AddTicks(7110), new DateTime(2026, 6, 13, 19, 6, 59, 177, DateTimeKind.Utc).AddTicks(7110) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 13, 19, 6, 59, 177, DateTimeKind.Utc).AddTicks(3860), new DateTime(2026, 6, 13, 19, 6, 59, 177, DateTimeKind.Utc).AddTicks(3860) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccountNumber",
                table: "InstructorPayoutAccounts");

            migrationBuilder.DropColumn(
                name: "ContactName",
                table: "InstructorPayoutAccounts");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "InstructorPayoutAccounts");

            migrationBuilder.DropColumn(
                name: "IfscCode",
                table: "InstructorPayoutAccounts");

            migrationBuilder.DropColumn(
                name: "LegalBusinessName",
                table: "InstructorPayoutAccounts");

            migrationBuilder.DropColumn(
                name: "Phone",
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
    }
}
