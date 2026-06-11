using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMSApi.DALLibrary.Migrations
{
    /// <inheritdoc />
    public partial class UpdateQuizPassingPercentage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PassingMarks",
                table: "Quizzes",
                newName: "PassingPercentage");

            migrationBuilder.Sql(@"
CREATE OR REPLACE FUNCTION calculate_pass_status(p_attempt_id INT)
RETURNS BOOLEAN AS $$
DECLARE
    v_score DOUBLE PRECISION;
    v_passing_percentage INT;
    v_quiz_id INT;
    v_total_marks DOUBLE PRECISION;
BEGIN
    SELECT ""Score"", ""QuizId"" INTO v_score, v_quiz_id
    FROM ""QuizAttempts""
    WHERE ""Id"" = p_attempt_id;
    
    SELECT ""PassingPercentage"" INTO v_passing_percentage
    FROM ""Quizzes""
    WHERE ""Id"" = v_quiz_id;
    
    SELECT COALESCE(SUM(""Mark""), 0.0) INTO v_total_marks
    FROM ""QuizQuestions""
    WHERE ""QuizId"" = v_quiz_id;
    
    IF v_total_marks = 0 THEN
        RETURN FALSE;
    END IF;

    RETURN (v_score / v_total_marks) * 100 >= v_passing_percentage;
END;
$$ LANGUAGE plpgsql;
            ");

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 11, 5, 22, 28, 925, DateTimeKind.Utc).AddTicks(3660), new DateTime(2026, 6, 11, 5, 22, 28, 925, DateTimeKind.Utc).AddTicks(3660) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 11, 5, 22, 28, 925, DateTimeKind.Utc).AddTicks(3670), new DateTime(2026, 6, 11, 5, 22, 28, 925, DateTimeKind.Utc).AddTicks(3670) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 11, 5, 22, 28, 925, DateTimeKind.Utc).AddTicks(3670), new DateTime(2026, 6, 11, 5, 22, 28, 925, DateTimeKind.Utc).AddTicks(3670) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 11, 5, 22, 28, 925, DateTimeKind.Utc).AddTicks(1010), new DateTime(2026, 6, 11, 5, 22, 28, 925, DateTimeKind.Utc).AddTicks(1010) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PassingPercentage",
                table: "Quizzes",
                newName: "PassingMarks");

            migrationBuilder.Sql(@"
CREATE OR REPLACE FUNCTION calculate_pass_status(p_attempt_id INT)
RETURNS BOOLEAN AS $$
DECLARE
    v_score DOUBLE PRECISION;
    v_passing_marks INT;
    v_quiz_id INT;
BEGIN
    SELECT ""Score"", ""QuizId"" INTO v_score, v_quiz_id
    FROM ""QuizAttempts""
    WHERE ""Id"" = p_attempt_id;
    
    SELECT ""PassingMarks"" INTO v_passing_marks
    FROM ""Quizzes""
    WHERE ""Id"" = v_quiz_id;
    
    RETURN v_score >= v_passing_marks;
END;
$$ LANGUAGE plpgsql;
            ");

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 10, 11, 18, 56, 779, DateTimeKind.Utc).AddTicks(5840), new DateTime(2026, 6, 10, 11, 18, 56, 779, DateTimeKind.Utc).AddTicks(5840) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 10, 11, 18, 56, 779, DateTimeKind.Utc).AddTicks(5840), new DateTime(2026, 6, 10, 11, 18, 56, 779, DateTimeKind.Utc).AddTicks(5840) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 10, 11, 18, 56, 779, DateTimeKind.Utc).AddTicks(5850), new DateTime(2026, 6, 10, 11, 18, 56, 779, DateTimeKind.Utc).AddTicks(5850) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 10, 11, 18, 56, 779, DateTimeKind.Utc).AddTicks(3320), new DateTime(2026, 6, 10, 11, 18, 56, 779, DateTimeKind.Utc).AddTicks(3320) });
        }
    }
}
