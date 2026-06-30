using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMSApi.DALLibrary.Migrations
{
    /// <inheritdoc />
    public partial class AddIsDeletedToReviews : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Reviews",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "Requirements",
                table: "Courses",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LearningOutcomes",
                table: "Courses",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Courses",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000);

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 30, 6, 27, 0, 954, DateTimeKind.Utc).AddTicks(1280), new DateTime(2026, 6, 30, 6, 27, 0, 954, DateTimeKind.Utc).AddTicks(1280) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 30, 6, 27, 0, 954, DateTimeKind.Utc).AddTicks(1280), new DateTime(2026, 6, 30, 6, 27, 0, 954, DateTimeKind.Utc).AddTicks(1280) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 30, 6, 27, 0, 954, DateTimeKind.Utc).AddTicks(1280), new DateTime(2026, 6, 30, 6, 27, 0, 954, DateTimeKind.Utc).AddTicks(1280) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 30, 6, 27, 0, 953, DateTimeKind.Utc).AddTicks(8070), new DateTime(2026, 6, 30, 6, 27, 0, 953, DateTimeKind.Utc).AddTicks(8070) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Reviews");

            migrationBuilder.AlterColumn<string>(
                name: "Requirements",
                table: "Courses",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LearningOutcomes",
                table: "Courses",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Courses",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500);

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 26, 6, 25, 49, 182, DateTimeKind.Utc).AddTicks(5370), new DateTime(2026, 6, 26, 6, 25, 49, 182, DateTimeKind.Utc).AddTicks(5380) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 26, 6, 25, 49, 182, DateTimeKind.Utc).AddTicks(5380), new DateTime(2026, 6, 26, 6, 25, 49, 182, DateTimeKind.Utc).AddTicks(5380) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 26, 6, 25, 49, 182, DateTimeKind.Utc).AddTicks(5380), new DateTime(2026, 6, 26, 6, 25, 49, 182, DateTimeKind.Utc).AddTicks(5380) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 26, 6, 25, 49, 182, DateTimeKind.Utc).AddTicks(1950), new DateTime(2026, 6, 26, 6, 25, 49, 182, DateTimeKind.Utc).AddTicks(1950) });
        }
    }
}
