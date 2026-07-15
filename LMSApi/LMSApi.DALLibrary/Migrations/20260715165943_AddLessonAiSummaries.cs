using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace LMSApi.DALLibrary.Migrations
{
    /// <inheritdoc />
    public partial class AddLessonAiSummaries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LessonAiSummaries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LessonId = table.Column<int>(type: "integer", nullable: false),
                    Summary = table.Column<string>(type: "text", nullable: false, defaultValue: ""),
                    KeyPointsJson = table.Column<string>(type: "text", nullable: false, defaultValue: "[]"),
                    Notes = table.Column<string>(type: "text", nullable: false, defaultValue: ""),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "generating"),
                    GeneratedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LessonAiSummaries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LessonAiSummaries_Lessons_LessonId",
                        column: x => x.LessonId,
                        principalTable: "Lessons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 7, 15, 16, 59, 43, 462, DateTimeKind.Utc).AddTicks(4790), new DateTime(2026, 7, 15, 16, 59, 43, 462, DateTimeKind.Utc).AddTicks(4790) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 7, 15, 16, 59, 43, 462, DateTimeKind.Utc).AddTicks(4790), new DateTime(2026, 7, 15, 16, 59, 43, 462, DateTimeKind.Utc).AddTicks(4790) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 7, 15, 16, 59, 43, 462, DateTimeKind.Utc).AddTicks(4800), new DateTime(2026, 7, 15, 16, 59, 43, 462, DateTimeKind.Utc).AddTicks(4800) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 7, 15, 16, 59, 43, 462, DateTimeKind.Utc).AddTicks(1610), new DateTime(2026, 7, 15, 16, 59, 43, 462, DateTimeKind.Utc).AddTicks(1610) });

            migrationBuilder.CreateIndex(
                name: "IX_LessonAiSummaries_LessonId",
                table: "LessonAiSummaries",
                column: "LessonId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LessonAiSummaries");

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 7, 15, 11, 57, 28, 982, DateTimeKind.Utc).AddTicks(1020), new DateTime(2026, 7, 15, 11, 57, 28, 982, DateTimeKind.Utc).AddTicks(1020) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 7, 15, 11, 57, 28, 982, DateTimeKind.Utc).AddTicks(1030), new DateTime(2026, 7, 15, 11, 57, 28, 982, DateTimeKind.Utc).AddTicks(1030) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 7, 15, 11, 57, 28, 982, DateTimeKind.Utc).AddTicks(1030), new DateTime(2026, 7, 15, 11, 57, 28, 982, DateTimeKind.Utc).AddTicks(1030) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 7, 15, 11, 57, 28, 981, DateTimeKind.Utc).AddTicks(6690), new DateTime(2026, 7, 15, 11, 57, 28, 981, DateTimeKind.Utc).AddTicks(6690) });
        }
    }
}
