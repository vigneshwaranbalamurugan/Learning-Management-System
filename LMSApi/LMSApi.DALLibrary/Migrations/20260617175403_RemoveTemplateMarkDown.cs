using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMSApi.DALLibrary.Migrations
{
    /// <inheritdoc />
    public partial class RemoveTemplateMarkDown : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TemplateMarkDown",
                table: "CertificateTemplates");

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 17, 17, 54, 3, 445, DateTimeKind.Utc).AddTicks(7940), new DateTime(2026, 6, 17, 17, 54, 3, 445, DateTimeKind.Utc).AddTicks(7940) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 17, 17, 54, 3, 445, DateTimeKind.Utc).AddTicks(7940), new DateTime(2026, 6, 17, 17, 54, 3, 445, DateTimeKind.Utc).AddTicks(7940) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 17, 17, 54, 3, 445, DateTimeKind.Utc).AddTicks(7950), new DateTime(2026, 6, 17, 17, 54, 3, 445, DateTimeKind.Utc).AddTicks(7950) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 17, 17, 54, 3, 445, DateTimeKind.Utc).AddTicks(4260), new DateTime(2026, 6, 17, 17, 54, 3, 445, DateTimeKind.Utc).AddTicks(4260) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TemplateMarkDown",
                table: "CertificateTemplates",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 17, 17, 1, 57, 571, DateTimeKind.Utc).AddTicks(640), new DateTime(2026, 6, 17, 17, 1, 57, 571, DateTimeKind.Utc).AddTicks(640) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 17, 17, 1, 57, 571, DateTimeKind.Utc).AddTicks(640), new DateTime(2026, 6, 17, 17, 1, 57, 571, DateTimeKind.Utc).AddTicks(640) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 17, 17, 1, 57, 571, DateTimeKind.Utc).AddTicks(640), new DateTime(2026, 6, 17, 17, 1, 57, 571, DateTimeKind.Utc).AddTicks(640) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 17, 17, 1, 57, 570, DateTimeKind.Utc).AddTicks(7500), new DateTime(2026, 6, 17, 17, 1, 57, 570, DateTimeKind.Utc).AddTicks(7500) });
        }
    }
}
