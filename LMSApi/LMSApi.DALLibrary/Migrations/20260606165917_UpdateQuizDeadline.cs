using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace LMSApi.DALLibrary.Migrations
{
    /// <inheritdoc />
    public partial class UpdateQuizDeadline : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvailableFrom",
                table: "Quizzes");

            migrationBuilder.DropColumn(
                name: "AvailableUntil",
                table: "Quizzes");

            migrationBuilder.RenameColumn(
                name: "DurationLimitInDays",
                table: "Assignments",
                newName: "DeadlineInDays");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Quizzes",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "DeadlineInDays",
                table: "Quizzes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Quizzes",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    CourseId = table.Column<int>(type: "integer", nullable: false),
                    EnrollmentId = table.Column<int>(type: "integer", nullable: false),
                    RazorpayOrderId = table.Column<string>(type: "text", nullable: false),
                    RazorpayPaymentId = table.Column<string>(type: "text", nullable: true),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    Currency = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    PaidAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RawResponse = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Payments_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Payments_Enrollments_EnrollmentId",
                        column: x => x.EnrollmentId,
                        principalTable: "Enrollments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Payments_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 6, 16, 59, 17, 516, DateTimeKind.Utc).AddTicks(4610), new DateTime(2026, 6, 6, 16, 59, 17, 516, DateTimeKind.Utc).AddTicks(4610) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 6, 16, 59, 17, 516, DateTimeKind.Utc).AddTicks(4610), new DateTime(2026, 6, 6, 16, 59, 17, 516, DateTimeKind.Utc).AddTicks(4620) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 6, 16, 59, 17, 516, DateTimeKind.Utc).AddTicks(4620), new DateTime(2026, 6, 6, 16, 59, 17, 516, DateTimeKind.Utc).AddTicks(4620) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 6, 16, 59, 17, 516, DateTimeKind.Utc).AddTicks(1270), new DateTime(2026, 6, 6, 16, 59, 17, 516, DateTimeKind.Utc).AddTicks(1270) });

            migrationBuilder.CreateIndex(
                name: "IX_Payments_CourseId",
                table: "Payments",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_EnrollmentId",
                table: "Payments",
                column: "EnrollmentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payments_UserId",
                table: "Payments",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Quizzes");

            migrationBuilder.DropColumn(
                name: "DeadlineInDays",
                table: "Quizzes");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Quizzes");

            migrationBuilder.RenameColumn(
                name: "DeadlineInDays",
                table: "Assignments",
                newName: "DurationLimitInDays");

            migrationBuilder.AddColumn<DateTime>(
                name: "AvailableFrom",
                table: "Quizzes",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AvailableUntil",
                table: "Quizzes",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 5, 6, 21, 56, 775, DateTimeKind.Utc).AddTicks(4920), new DateTime(2026, 6, 5, 6, 21, 56, 775, DateTimeKind.Utc).AddTicks(4920) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 5, 6, 21, 56, 775, DateTimeKind.Utc).AddTicks(4920), new DateTime(2026, 6, 5, 6, 21, 56, 775, DateTimeKind.Utc).AddTicks(4930) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 5, 6, 21, 56, 775, DateTimeKind.Utc).AddTicks(4930), new DateTime(2026, 6, 5, 6, 21, 56, 775, DateTimeKind.Utc).AddTicks(4930) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 5, 6, 21, 56, 775, DateTimeKind.Utc).AddTicks(1650), new DateTime(2026, 6, 5, 6, 21, 56, 775, DateTimeKind.Utc).AddTicks(1650) });
        }
    }
}
