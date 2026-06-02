using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMSApi.DALLibrary.Migrations
{
    /// <inheritdoc />
    public partial class AdminAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "UserRoles",
                columns: new[] { "Id", "CreatedAt", "Description", "RoleName", "UpdatedAt" },
                values: new object[] { 3, new DateTime(2026, 5, 28, 0, 0, 0, 0, DateTimeKind.Utc), "Admin account", "Admin", new DateTime(2026, 5, 28, 0, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "CurrentTokenType", "Email", "IsActive", "IsEmailVerified", "LastLoginAt", "PasswordHash", "PasswordSalt", "RoleId", "UpdatedAt", "VerificationToken", "VerificationTokenExpiry" },
                values: new object[] { 1, new DateTime(2026, 5, 28, 0, 0, 0, 0, DateTimeKind.Utc), null, "admin@gmail.com", true, true, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "TPZPlYPS43ldK8EYFX67pHzyMNFmt69wd9N2cUNObYs=", null, 3, new DateTime(2026, 5, 28, 0, 0, 0, 0, DateTimeKind.Utc), null, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 3);
        }
    }
}
