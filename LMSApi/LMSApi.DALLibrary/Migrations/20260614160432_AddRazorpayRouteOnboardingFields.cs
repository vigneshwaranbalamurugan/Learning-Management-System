using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMSApi.DALLibrary.Migrations
{
    /// <inheritdoc />
    public partial class AddRazorpayRouteOnboardingFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AccountStatus",
                table: "InstructorPayoutAccounts",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsVerified",
                table: "InstructorPayoutAccounts",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "RazorpayProductId",
                table: "InstructorPayoutAccounts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RazorpayStakeholderId",
                table: "InstructorPayoutAccounts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "VerifiedAt",
                table: "InstructorPayoutAccounts",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 14, 16, 4, 32, 258, DateTimeKind.Utc).AddTicks(2470), new DateTime(2026, 6, 14, 16, 4, 32, 258, DateTimeKind.Utc).AddTicks(2470) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 14, 16, 4, 32, 258, DateTimeKind.Utc).AddTicks(2470), new DateTime(2026, 6, 14, 16, 4, 32, 258, DateTimeKind.Utc).AddTicks(2470) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 14, 16, 4, 32, 258, DateTimeKind.Utc).AddTicks(2470), new DateTime(2026, 6, 14, 16, 4, 32, 258, DateTimeKind.Utc).AddTicks(2470) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 14, 16, 4, 32, 258, DateTimeKind.Utc).AddTicks(310), new DateTime(2026, 6, 14, 16, 4, 32, 258, DateTimeKind.Utc).AddTicks(310) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccountStatus",
                table: "InstructorPayoutAccounts");

            migrationBuilder.DropColumn(
                name: "IsVerified",
                table: "InstructorPayoutAccounts");

            migrationBuilder.DropColumn(
                name: "RazorpayProductId",
                table: "InstructorPayoutAccounts");

            migrationBuilder.DropColumn(
                name: "RazorpayStakeholderId",
                table: "InstructorPayoutAccounts");

            migrationBuilder.DropColumn(
                name: "VerifiedAt",
                table: "InstructorPayoutAccounts");

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 14, 14, 33, 37, 422, DateTimeKind.Utc).AddTicks(3900), new DateTime(2026, 6, 14, 14, 33, 37, 422, DateTimeKind.Utc).AddTicks(3900) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 14, 14, 33, 37, 422, DateTimeKind.Utc).AddTicks(3900), new DateTime(2026, 6, 14, 14, 33, 37, 422, DateTimeKind.Utc).AddTicks(3900) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 14, 14, 33, 37, 422, DateTimeKind.Utc).AddTicks(3910), new DateTime(2026, 6, 14, 14, 33, 37, 422, DateTimeKind.Utc).AddTicks(3910) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 14, 14, 33, 37, 422, DateTimeKind.Utc).AddTicks(630), new DateTime(2026, 6, 14, 14, 33, 37, 422, DateTimeKind.Utc).AddTicks(630) });
        }
    }
}
