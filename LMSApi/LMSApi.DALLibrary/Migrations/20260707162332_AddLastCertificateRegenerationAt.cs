using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMSApi.DALLibrary.Migrations
{
    /// <inheritdoc />
    public partial class AddLastCertificateRegenerationAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastCertificateRegenerationAt",
                table: "UserProfiles",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 7, 7, 16, 23, 32, 401, DateTimeKind.Utc).AddTicks(8160), new DateTime(2026, 7, 7, 16, 23, 32, 401, DateTimeKind.Utc).AddTicks(8160) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 7, 7, 16, 23, 32, 401, DateTimeKind.Utc).AddTicks(8160), new DateTime(2026, 7, 7, 16, 23, 32, 401, DateTimeKind.Utc).AddTicks(8160) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 7, 7, 16, 23, 32, 401, DateTimeKind.Utc).AddTicks(8160), new DateTime(2026, 7, 7, 16, 23, 32, 401, DateTimeKind.Utc).AddTicks(8160) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 7, 7, 16, 23, 32, 401, DateTimeKind.Utc).AddTicks(4430), new DateTime(2026, 7, 7, 16, 23, 32, 401, DateTimeKind.Utc).AddTicks(4430) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastCertificateRegenerationAt",
                table: "UserProfiles");

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
    }
}
