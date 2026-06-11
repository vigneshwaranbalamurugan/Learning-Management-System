using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMSApi.DALLibrary.Migrations
{
    /// <inheritdoc />
    public partial class PaymentDbUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Enrollments_EnrollmentId",
                table: "Payments");

            migrationBuilder.AlterColumn<int>(
                name: "EnrollmentId",
                table: "Payments",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 9, 9, 13, 4, 869, DateTimeKind.Utc).AddTicks(5930), new DateTime(2026, 6, 9, 9, 13, 4, 869, DateTimeKind.Utc).AddTicks(5930) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 9, 9, 13, 4, 869, DateTimeKind.Utc).AddTicks(5930), new DateTime(2026, 6, 9, 9, 13, 4, 869, DateTimeKind.Utc).AddTicks(5930) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 9, 9, 13, 4, 869, DateTimeKind.Utc).AddTicks(5930), new DateTime(2026, 6, 9, 9, 13, 4, 869, DateTimeKind.Utc).AddTicks(5930) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 9, 9, 13, 4, 869, DateTimeKind.Utc).AddTicks(2620), new DateTime(2026, 6, 9, 9, 13, 4, 869, DateTimeKind.Utc).AddTicks(2620) });

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Enrollments_EnrollmentId",
                table: "Payments",
                column: "EnrollmentId",
                principalTable: "Enrollments",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Enrollments_EnrollmentId",
                table: "Payments");

            migrationBuilder.AlterColumn<int>(
                name: "EnrollmentId",
                table: "Payments",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 9, 9, 4, 56, 442, DateTimeKind.Utc).AddTicks(7530), new DateTime(2026, 6, 9, 9, 4, 56, 442, DateTimeKind.Utc).AddTicks(7530) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 9, 9, 4, 56, 442, DateTimeKind.Utc).AddTicks(7530), new DateTime(2026, 6, 9, 9, 4, 56, 442, DateTimeKind.Utc).AddTicks(7530) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 9, 9, 4, 56, 442, DateTimeKind.Utc).AddTicks(7530), new DateTime(2026, 6, 9, 9, 4, 56, 442, DateTimeKind.Utc).AddTicks(7530) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 9, 9, 4, 56, 442, DateTimeKind.Utc).AddTicks(4220), new DateTime(2026, 6, 9, 9, 4, 56, 442, DateTimeKind.Utc).AddTicks(4220) });

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Enrollments_EnrollmentId",
                table: "Payments",
                column: "EnrollmentId",
                principalTable: "Enrollments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
