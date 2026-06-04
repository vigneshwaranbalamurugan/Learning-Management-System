using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace LMSApi.DALLibrary.Migrations
{
    /// <inheritdoc />
    public partial class BatchesAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<TimeSpan>(
                name: "EstimatedDuration",
                table: "LessonResources",
                type: "interval",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<TimeSpan>(
                name: "EstimatedDuration",
                table: "CourseSections",
                type: "interval",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AlterColumn<int>(
                name: "Level",
                table: "Courses",
                type: "integer",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "Language",
                table: "Courses",
                type: "integer",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "CourseAccessType",
                table: "Courses",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "DefaultAssignmentDeadlineDays",
                table: "Courses",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CourseBatches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CourseId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EnrollmentStartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EnrollmentEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    MaxStudents = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseBatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CourseBatches_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Enrollments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    CourseId = table.Column<int>(type: "integer", nullable: false),
                    EnrolledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ProgressPercentage = table.Column<decimal>(type: "numeric(5,2)", nullable: false, defaultValue: 0m),
                    IsCompleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    BatchId = table.Column<int>(type: "integer", nullable: true),
                    AccessExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EnrollmentStatus = table.Column<int>(type: "integer", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Enrollments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Enrollments_CourseBatches_BatchId",
                        column: x => x.BatchId,
                        principalTable: "CourseBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Enrollments_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Enrollments_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 4, 5, 3, 58, 996, DateTimeKind.Utc).AddTicks(9500), new DateTime(2026, 6, 4, 5, 3, 58, 996, DateTimeKind.Utc).AddTicks(9500) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 4, 5, 3, 58, 996, DateTimeKind.Utc).AddTicks(9500), new DateTime(2026, 6, 4, 5, 3, 58, 996, DateTimeKind.Utc).AddTicks(9500) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 4, 5, 3, 58, 996, DateTimeKind.Utc).AddTicks(9500), new DateTime(2026, 6, 4, 5, 3, 58, 996, DateTimeKind.Utc).AddTicks(9500) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 4, 5, 3, 58, 996, DateTimeKind.Utc).AddTicks(6250), "TPZPlYPS43ldK8EYFX67pHzyMNFmt69wd9N2cUNObYs=", new DateTime(2026, 6, 4, 5, 3, 58, 996, DateTimeKind.Utc).AddTicks(6250) });

            migrationBuilder.CreateIndex(
                name: "IX_CourseBatches_CourseId",
                table: "CourseBatches",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_Enrollments_BatchId",
                table: "Enrollments",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_Enrollments_CourseId",
                table: "Enrollments",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_Enrollments_UserId_CourseId",
                table: "Enrollments",
                columns: new[] { "UserId", "CourseId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Enrollments");

            migrationBuilder.DropTable(
                name: "CourseBatches");

            migrationBuilder.DropColumn(
                name: "EstimatedDuration",
                table: "LessonResources");

            migrationBuilder.DropColumn(
                name: "EstimatedDuration",
                table: "CourseSections");

            migrationBuilder.DropColumn(
                name: "CourseAccessType",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "DefaultAssignmentDeadlineDays",
                table: "Courses");

            migrationBuilder.AlterColumn<int>(
                name: "Level",
                table: "Courses",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 1);

            migrationBuilder.AlterColumn<int>(
                name: "Language",
                table: "Courses",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 1);

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 2, 11, 26, 40, 751, DateTimeKind.Utc).AddTicks(3620), new DateTime(2026, 6, 2, 11, 26, 40, 751, DateTimeKind.Utc).AddTicks(3620) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 2, 11, 26, 40, 751, DateTimeKind.Utc).AddTicks(3620), new DateTime(2026, 6, 2, 11, 26, 40, 751, DateTimeKind.Utc).AddTicks(3620) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 2, 11, 26, 40, 751, DateTimeKind.Utc).AddTicks(3620), new DateTime(2026, 6, 2, 11, 26, 40, 751, DateTimeKind.Utc).AddTicks(3620) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 2, 11, 26, 40, 750, DateTimeKind.Utc).AddTicks(9980), "QWRtaW5AMTIz", new DateTime(2026, 6, 2, 11, 26, 40, 751, DateTimeKind.Utc).AddTicks(20) });
        }
    }
}
