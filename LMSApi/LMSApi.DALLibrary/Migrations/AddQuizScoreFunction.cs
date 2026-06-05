using Microsoft.EntityFrameworkCore.Migrations;

namespace LMSApi.DALLibrary.Migrations
{
    /// <summary>
    /// Standalone migration that creates the PostgreSQL helper functions
    /// for computing quiz attempt scores, pass status, and remaining attempts.
    /// </summary>
    public partial class AddQuizScoreFunction : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop old version if exists
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS calculate_quiz_score(INT);");

            // 1. calculate_quiz_score
            migrationBuilder.Sql(@"
CREATE OR REPLACE FUNCTION calculate_quiz_score(p_attempt_id INT)
RETURNS DOUBLE PRECISION AS $$
DECLARE
    v_score DOUBLE PRECISION;
BEGIN
    SELECT COALESCE(SUM(qq.""Mark""), 0.0)
    INTO v_score
    FROM ""QuizAnswers"" qa
    INNER JOIN ""QuizQuestions"" qq ON qq.""Id"" = qa.""QuestionId""
    INNER JOIN ""QuizOptions"" qo ON qo.""Id"" = qa.""SelectedOptionId""
    WHERE qa.""AttemptId"" = p_attempt_id
      AND qo.""IsCorrect"" = TRUE;
    
    RETURN v_score;
END;
$$ LANGUAGE plpgsql;
            ");

            // 2. calculate_pass_status
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

            // 3. get_remaining_attempts
            migrationBuilder.Sql(@"
CREATE OR REPLACE FUNCTION get_remaining_attempts(p_quiz_id INT, p_user_id INT)
RETURNS INT AS $$
DECLARE
    v_max_attempts INT;
    v_attempt_count INT;
BEGIN
    SELECT ""MaxAttempts"" INTO v_max_attempts
    FROM ""Quizzes""
    WHERE ""Id"" = p_quiz_id;
    
    SELECT COUNT(*) INTO v_attempt_count
    FROM ""QuizAttempts""
    WHERE ""QuizId"" = p_quiz_id 
      AND ""UserId"" = p_user_id
      AND ""Status"" != 'Expired';
    
    RETURN GREATEST(0, v_max_attempts - v_attempt_count);
END;
$$ LANGUAGE plpgsql;
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS calculate_quiz_score(INT);");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS calculate_pass_status(INT);");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS get_remaining_attempts(INT, INT);");
        }
    }
}
