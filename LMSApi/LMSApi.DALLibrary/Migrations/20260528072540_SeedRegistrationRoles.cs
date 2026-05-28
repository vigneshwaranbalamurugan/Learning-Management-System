using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace LMSApi.DALLibrary.Migrations
{
    /// <inheritdoc />
    public partial class SeedRegistrationRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "UserRoles",
                columns: new[] { "Id", "CreatedAt", "Description", "RoleName", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 5, 28, 0, 0, 0, 0, DateTimeKind.Utc), "Learner account", "Learner", new DateTime(2026, 5, 28, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 2, new DateTime(2026, 5, 28, 0, 0, 0, 0, DateTimeKind.Utc), "Instructor account", "Instructor", new DateTime(2026, 5, 28, 0, 0, 0, 0, DateTimeKind.Utc) }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 2);
        }
    }
}
