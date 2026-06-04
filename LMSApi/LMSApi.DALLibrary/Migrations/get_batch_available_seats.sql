-- ============================================================
-- PostgreSQL Function: get_batch_available_seats
-- 
-- Returns the number of available seats in a CourseBatch.
-- Called by CourseBatchRepository.GetAvailableSeatsAsync()
-- via EF Core: Database.SqlQuery<int>($"SELECT get_batch_available_seats({batchId})")
-- ============================================================

CREATE OR REPLACE FUNCTION get_batch_available_seats(p_batch_id INT)
RETURNS INT AS $$
DECLARE
    v_max_students  INT;
    v_enrolled_count INT;
BEGIN
    -- Get max capacity for the batch
    SELECT "MaxStudents"
    INTO v_max_students
    FROM "CourseBatches"
    WHERE "Id" = p_batch_id;

    IF NOT FOUND THEN
        RAISE EXCEPTION 'Batch with id % not found', p_batch_id;
    END IF;

    -- Count active enrollments in this batch
    SELECT COUNT(*)
    INTO v_enrolled_count
    FROM "Enrollments"
    WHERE "BatchId" = p_batch_id;

    -- Return available seats, never negative
    RETURN GREATEST(0, v_max_students - v_enrolled_count);
END;
$$ LANGUAGE plpgsql;
