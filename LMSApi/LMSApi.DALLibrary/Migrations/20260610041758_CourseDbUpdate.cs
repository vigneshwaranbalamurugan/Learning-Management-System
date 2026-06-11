using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMSApi.DALLibrary.Migrations
{
    /// <inheritdoc />
    public partial class CourseDbUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPublished",
                table: "Courses");

            migrationBuilder.RenameColumn(
                name: "DefaultAssignmentDeadlineDays",
                table: "Courses",
                newName: "DefaultDeadlineDays");

            migrationBuilder.AlterColumn<string>(
                name: "Requirements",
                table: "Courses",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "LearningOutcomes",
                table: "Courses",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "IntroVideoUrl",
                table: "Courses",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000);

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 10, 4, 17, 57, 563, DateTimeKind.Utc).AddTicks(7830), new DateTime(2026, 6, 10, 4, 17, 57, 563, DateTimeKind.Utc).AddTicks(7830) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 10, 4, 17, 57, 563, DateTimeKind.Utc).AddTicks(7830), new DateTime(2026, 6, 10, 4, 17, 57, 563, DateTimeKind.Utc).AddTicks(7830) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 10, 4, 17, 57, 563, DateTimeKind.Utc).AddTicks(7830), new DateTime(2026, 6, 10, 4, 17, 57, 563, DateTimeKind.Utc).AddTicks(7830) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 10, 4, 17, 57, 563, DateTimeKind.Utc).AddTicks(4460), new DateTime(2026, 6, 10, 4, 17, 57, 563, DateTimeKind.Utc).AddTicks(4460) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DefaultDeadlineDays",
                table: "Courses",
                newName: "DefaultAssignmentDeadlineDays");

            migrationBuilder.AlterColumn<string>(
                name: "Requirements",
                table: "Courses",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LearningOutcomes",
                table: "Courses",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "IntroVideoUrl",
                table: "Courses",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPublished",
                table: "Courses",
                type: "boolean",
                nullable: false,
                defaultValue: false);

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
        }
    }
}
