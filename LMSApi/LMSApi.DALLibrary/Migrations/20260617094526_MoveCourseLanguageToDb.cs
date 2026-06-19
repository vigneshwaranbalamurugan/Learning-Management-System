using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace LMSApi.DALLibrary.Migrations
{
    /// <inheritdoc />
    public partial class MoveCourseLanguageToDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Language",
                table: "Courses",
                newName: "LanguageId");

            migrationBuilder.CreateTable(
                name: "CourseLanguages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseLanguages", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "CourseLanguages",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "English" },
                    { 2, "Tamil" },
                    { 3, "Hindi" }
                });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 17, 9, 45, 26, 118, DateTimeKind.Utc).AddTicks(6900), new DateTime(2026, 6, 17, 9, 45, 26, 118, DateTimeKind.Utc).AddTicks(6910) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 17, 9, 45, 26, 118, DateTimeKind.Utc).AddTicks(6910), new DateTime(2026, 6, 17, 9, 45, 26, 118, DateTimeKind.Utc).AddTicks(6910) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 17, 9, 45, 26, 118, DateTimeKind.Utc).AddTicks(6910), new DateTime(2026, 6, 17, 9, 45, 26, 118, DateTimeKind.Utc).AddTicks(6910) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 17, 9, 45, 26, 118, DateTimeKind.Utc).AddTicks(3650), new DateTime(2026, 6, 17, 9, 45, 26, 118, DateTimeKind.Utc).AddTicks(3650) });

            migrationBuilder.CreateIndex(
                name: "IX_Courses_LanguageId",
                table: "Courses",
                column: "LanguageId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseLanguages_Name",
                table: "CourseLanguages",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_CourseLanguages_LanguageId",
                table: "Courses",
                column: "LanguageId",
                principalTable: "CourseLanguages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Courses_CourseLanguages_LanguageId",
                table: "Courses");

            migrationBuilder.DropTable(
                name: "CourseLanguages");

            migrationBuilder.DropIndex(
                name: "IX_Courses_LanguageId",
                table: "Courses");

            migrationBuilder.RenameColumn(
                name: "LanguageId",
                table: "Courses",
                newName: "Language");

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 16, 11, 25, 14, 86, DateTimeKind.Utc).AddTicks(1140), new DateTime(2026, 6, 16, 11, 25, 14, 86, DateTimeKind.Utc).AddTicks(1140) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 16, 11, 25, 14, 86, DateTimeKind.Utc).AddTicks(1140), new DateTime(2026, 6, 16, 11, 25, 14, 86, DateTimeKind.Utc).AddTicks(1140) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 16, 11, 25, 14, 86, DateTimeKind.Utc).AddTicks(1140), new DateTime(2026, 6, 16, 11, 25, 14, 86, DateTimeKind.Utc).AddTicks(1140) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 16, 11, 25, 14, 85, DateTimeKind.Utc).AddTicks(8740), new DateTime(2026, 6, 16, 11, 25, 14, 85, DateTimeKind.Utc).AddTicks(8740) });
        }
    }
}
