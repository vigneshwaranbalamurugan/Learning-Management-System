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
