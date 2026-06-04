using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace LMSApi.DALLibrary.Migrations
{
    /// <inheritdoc />
    public partial class LessonsUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Duration",
                table: "Lessons");

            migrationBuilder.DropColumn(
                name: "VideoUrl",
                table: "Lessons");

            migrationBuilder.DropColumn(
                name: "EstimatedDuration",
                table: "LessonResources");

            migrationBuilder.RenameColumn(
                name: "ExternalUrl",
                table: "Lessons",
                newName: "ContentUrl");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Lessons",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000);

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "Lessons",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Lessons",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsPreview",
                table: "Lessons",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsPublished",
                table: "Lessons",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Lessons",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "StudentProgress",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StudentId = table.Column<int>(type: "integer", nullable: false),
                    CourseId = table.Column<int>(type: "integer", nullable: false),
                    LessonId = table.Column<int>(type: "integer", nullable: false),
                    IsCompleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    VideoWatchedPercentage = table.Column<decimal>(type: "numeric(5,2)", nullable: false, defaultValue: 0m),
                    LastWatchedSecond = table.Column<int>(type: "integer", nullable: false),
                    ProgressPercentage = table.Column<double>(type: "double precision", nullable: false, defaultValue: 0.0),
                    LastAccessed = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentProgress", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentProgress_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudentProgress_Lessons_LessonId",
                        column: x => x.LessonId,
                        principalTable: "Lessons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudentProgress_Users_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 4, 10, 45, 10, 238, DateTimeKind.Utc).AddTicks(9000), new DateTime(2026, 6, 4, 10, 45, 10, 238, DateTimeKind.Utc).AddTicks(9000) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 4, 10, 45, 10, 238, DateTimeKind.Utc).AddTicks(9000), new DateTime(2026, 6, 4, 10, 45, 10, 238, DateTimeKind.Utc).AddTicks(9000) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 4, 10, 45, 10, 238, DateTimeKind.Utc).AddTicks(9000), new DateTime(2026, 6, 4, 10, 45, 10, 238, DateTimeKind.Utc).AddTicks(9000) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 4, 10, 45, 10, 238, DateTimeKind.Utc).AddTicks(5270), new DateTime(2026, 6, 4, 10, 45, 10, 238, DateTimeKind.Utc).AddTicks(5270) });

            migrationBuilder.CreateIndex(
                name: "IX_StudentProgress_CourseId",
                table: "StudentProgress",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentProgress_LessonId",
                table: "StudentProgress",
                column: "LessonId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentProgress_StudentId",
                table: "StudentProgress",
                column: "StudentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StudentProgress");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Lessons");

            migrationBuilder.DropColumn(
                name: "IsPreview",
                table: "Lessons");

            migrationBuilder.DropColumn(
                name: "IsPublished",
                table: "Lessons");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Lessons");

            migrationBuilder.RenameColumn(
                name: "ContentUrl",
                table: "Lessons",
                newName: "ExternalUrl");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Lessons",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "Lessons",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "Duration",
                table: "Lessons",
                type: "interval",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<string>(
                name: "VideoUrl",
                table: "Lessons",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<TimeSpan>(
                name: "EstimatedDuration",
                table: "LessonResources",
                type: "interval",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 4, 7, 21, 9, 403, DateTimeKind.Utc).AddTicks(1120), new DateTime(2026, 6, 4, 7, 21, 9, 403, DateTimeKind.Utc).AddTicks(1120) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 4, 7, 21, 9, 403, DateTimeKind.Utc).AddTicks(1120), new DateTime(2026, 6, 4, 7, 21, 9, 403, DateTimeKind.Utc).AddTicks(1120) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 4, 7, 21, 9, 403, DateTimeKind.Utc).AddTicks(1120), new DateTime(2026, 6, 4, 7, 21, 9, 403, DateTimeKind.Utc).AddTicks(1120) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 4, 7, 21, 9, 402, DateTimeKind.Utc).AddTicks(7650), new DateTime(2026, 6, 4, 7, 21, 9, 402, DateTimeKind.Utc).AddTicks(7650) });
        }
    }
}
