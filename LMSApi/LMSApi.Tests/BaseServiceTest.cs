using AutoMapper;
using LMSApi.DALLibrary.Contexts;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace LMSApi.Tests
{
    public abstract class BaseServiceTest
    {
        protected LMSDbContext DbContext { get; private set; } = null!;
        protected IMapper Mapper { get; private set; } = null!;

        [SetUp]
        public virtual void SetUp()
        {
            var options = new DbContextOptionsBuilder<LMSDbContext>()
                .UseNpgsql("Host=localhost;Port=5432;Database=lmstestdb;Username=postgres;Password=978681")
                .Options;

            DbContext = new LMSDbContext(options);
            DbContext.Database.EnsureCreated(); // Or EnsureDeleted() then EnsureCreated() based on how isolation is preferred
            
            // Create the Postgres function manually for test DB since migrations don't run
            DbContext.Database.ExecuteSqlRaw(@"
                CREATE OR REPLACE FUNCTION get_batch_available_seats(p_batch_id INTEGER)
                RETURNS INTEGER
                LANGUAGE plpgsql
                AS $$
                DECLARE
                    v_max_students INTEGER;
                    v_enrolled INTEGER;
                BEGIN
                    SELECT ""MaxStudents"" INTO v_max_students
                    FROM ""CourseBatches""
                    WHERE ""Id"" = p_batch_id;

                    SELECT COUNT(*) INTO v_enrolled
                    FROM ""Enrollments""
                    WHERE ""BatchId"" = p_batch_id AND ""EnrollmentStatus"" = 0;

                    RETURN COALESCE(v_max_students, 0) - v_enrolled;
                END;
                $$;

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

            // Initialize AutoMapper with the profiles from the BALLibrary assembly
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddMaps(typeof(LMSApi.BALLibrary.Services.AuthService).Assembly);
            });
            Mapper = config.CreateMapper();
        }

        [TearDown]
        public virtual void TearDown()
        {
            DbContext.Database.EnsureDeleted();
            DbContext.Dispose();
        }
    }
}
