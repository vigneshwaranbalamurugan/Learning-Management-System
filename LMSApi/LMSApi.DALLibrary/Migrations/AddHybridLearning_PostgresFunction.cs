using Microsoft.EntityFrameworkCore.Migrations;

namespace LMSApi.DALLibrary.Migrations
{
    /// <summary>
    /// Standalone migration that creates the PostgreSQL helper function
    /// for computing batch seat availability.
    ///
    /// This migration must run AFTER <c>AddHybridLearning</c> (which creates
    /// the CourseBatches and Enrollments tables).
    ///
    /// Run order is controlled by the migration name timestamp prefix.
    /// </summary>
    public partial class AddBatchAvailabilityFunction : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
CREATE OR REPLACE FUNCTION get_batch_available_seats(p_batch_id INT)
RETURNS INT AS $$
DECLARE
    v_max_students   INT;
    v_enrolled_count INT;
BEGIN
    SELECT ""MaxStudents""
    INTO v_max_students
    FROM ""CourseBatches""
    WHERE ""Id"" = p_batch_id;

    IF NOT FOUND THEN
        RAISE EXCEPTION 'Batch with id % not found', p_batch_id;
    END IF;

    SELECT COUNT(*)
    INTO v_enrolled_count
    FROM ""Enrollments""
    WHERE ""BatchId"" = p_batch_id;

    RETURN GREATEST(0, v_max_students - v_enrolled_count);
END;
$$ LANGUAGE plpgsql;
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "DROP FUNCTION IF EXISTS get_batch_available_seats(INT);");
        }
    }
}
