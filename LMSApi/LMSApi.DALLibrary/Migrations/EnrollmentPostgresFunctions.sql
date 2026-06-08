-- calculate_available_seats
CREATE OR REPLACE FUNCTION calculate_available_seats(p_batch_id INT)
RETURNS INT AS $$
DECLARE
    v_max_students INT;
    v_active_enrollments INT;
BEGIN
    -- Get MaxStudents
    SELECT "MaxStudents" INTO v_max_students
    FROM "CourseBatches"
    WHERE "Id" = p_batch_id;

    IF NOT FOUND THEN
        RETURN 0;
    END IF;

    -- Get Count of Active Enrollments
    SELECT COUNT(*) INTO v_active_enrollments
    FROM "Enrollments"
    WHERE "BatchId" = p_batch_id AND "EnrollmentStatus" = 0; -- Assuming 0 is Active

    -- Return available seats
    RETURN GREATEST(v_max_students - v_active_enrollments, 0);
END;
$$ LANGUAGE plpgsql;


-- calculate_assignment_deadline
CREATE OR REPLACE FUNCTION calculate_assignment_deadline(p_user_id INT, p_assignment_id INT)
RETURNS TIMESTAMP AS $$
DECLARE
    v_course_id INT;
    v_deadline_days INT;
    v_batch_id INT;
    v_course_access_type INT;
    v_enrolled_at TIMESTAMP;
    v_batch_start TIMESTAMP;
    v_deadline TIMESTAMP;
BEGIN
    -- Get CourseId and DeadlineInDays from Assignment
    SELECT cs."CourseId", a."DeadlineInDays"
    INTO v_course_id, v_deadline_days
    FROM "Assignments" a
    JOIN "CourseSections" cs ON a."CourseSectionId" = cs."Id"
    WHERE a."Id" = p_assignment_id;

    IF v_deadline_days = 0 OR v_deadline_days IS NULL THEN
        RETURN NULL; -- No deadline
    END IF;

    -- Get Enrollment Details
    SELECT "EnrolledAt", "BatchId"
    INTO v_enrolled_at, v_batch_id
    FROM "Enrollments"
    WHERE "UserId" = p_user_id AND "CourseId" = v_course_id AND "EnrollmentStatus" = 0;

    -- Get CourseAccessType
    SELECT "CourseAccessType"
    INTO v_course_access_type
    FROM "Courses"
    WHERE "Id" = v_course_id;

    -- Calculate based on Access Type
    IF v_course_access_type = 0 THEN -- 0: SelfPaced
        v_deadline := v_enrolled_at + (v_deadline_days || ' days')::INTERVAL;
    ELSIF v_course_access_type = 1 THEN -- 1: CohortBased
        IF v_batch_id IS NOT NULL THEN
            SELECT "StartDate" INTO v_batch_start
            FROM "CourseBatches"
            WHERE "Id" = v_batch_id;
            
            v_deadline := v_batch_start + (v_deadline_days || ' days')::INTERVAL;
        END IF;
    ELSE -- Hybrid (Assume SelfPaced fallback)
        v_deadline := v_enrolled_at + (v_deadline_days || ' days')::INTERVAL;
    END IF;

    RETURN v_deadline;
END;
$$ LANGUAGE plpgsql;


-- calculate_course_progress
CREATE OR REPLACE FUNCTION calculate_course_progress(p_user_id INT, p_course_id INT)
RETURNS DECIMAL AS $$
DECLARE
    v_total_lessons INT;
    v_total_quizzes INT;
    v_total_assignments INT;
    v_total_items INT;
    v_completed_lessons INT;
    v_passed_quizzes INT;
    v_passed_assignments INT;
    v_completed_items INT;
    v_progress DECIMAL;
BEGIN
    -- Removed check course access type, apply same logic to both

    -- Count total published lessons
    SELECT COUNT(l."Id") INTO v_total_lessons
    FROM "Lessons" l
    JOIN "CourseSections" cs ON l."CourseSectionId" = cs."Id"
    WHERE cs."CourseId" = p_course_id AND l."IsPublished" = true;

    -- Count total published quizzes
    SELECT COUNT(q."Id") INTO v_total_quizzes
    FROM "Quizzes" q
    JOIN "CourseSections" cs ON q."CourseSectionId" = cs."Id"
    WHERE cs."CourseId" = p_course_id AND q."IsPublished" = true;

    -- Count total published assignments
    SELECT COUNT(a."Id") INTO v_total_assignments
    FROM "Assignments" a
    JOIN "CourseSections" cs ON a."CourseSectionId" = cs."Id"
    WHERE cs."CourseId" = p_course_id AND a."IsPublished" = true;

    v_total_items := v_total_lessons + v_total_quizzes + v_total_assignments;

    IF v_total_items = 0 THEN
        RETURN 0;
    END IF;

    -- Count completed lessons for user (only published)
    SELECT COUNT(*) INTO v_completed_lessons
    FROM "StudentProgresses" sp
    JOIN "Lessons" l ON sp."LessonId" = l."Id"
    JOIN "CourseSections" cs ON l."CourseSectionId" = cs."Id"
    WHERE cs."CourseId" = p_course_id 
      AND sp."UserId" = p_user_id 
      AND sp."IsCompleted" = true
      AND l."IsPublished" = true;

    -- Count passed quizzes for user (only published)
    SELECT COUNT(DISTINCT qa."QuizId") INTO v_passed_quizzes
    FROM "QuizAttempts" qa
    JOIN "Quizzes" q ON qa."QuizId" = q."Id"
    JOIN "CourseSections" cs ON q."CourseSectionId" = cs."Id"
    WHERE cs."CourseId" = p_course_id 
      AND qa."StudentId" = p_user_id 
      AND qa."IsPassed" = true
      AND q."IsPublished" = true;

    -- Count passed assignments for user (only published)
    SELECT COUNT(DISTINCT asub."AssignmentId") INTO v_passed_assignments
    FROM "AssignmentSubmissions" asub
    JOIN "Assignments" a ON asub."AssignmentId" = a."Id"
    JOIN "CourseSections" cs ON a."CourseSectionId" = cs."Id"
    WHERE cs."CourseId" = p_course_id 
      AND asub."StudentId" = p_user_id 
      AND asub."IsPassed" = true
      AND a."IsPublished" = true;

    v_completed_items := v_completed_lessons + v_passed_quizzes + v_passed_assignments;
    v_progress := (v_completed_items::DECIMAL / v_total_items) * 100;
    
    -- Update Enrollment Progress
    UPDATE "Enrollments"
    SET "ProgressPercentage" = v_progress
    WHERE "UserId" = p_user_id AND "CourseId" = p_course_id;

    RETURN v_progress;
END;
$$ LANGUAGE plpgsql;


-- is_course_completed
CREATE OR REPLACE FUNCTION is_course_completed(p_user_id INT, p_course_id INT)
RETURNS BOOLEAN AS $$
DECLARE
    v_progress DECIMAL;
BEGIN
    v_progress := calculate_course_progress(p_user_id, p_course_id);
    
    IF v_progress = 100 THEN
        UPDATE "Enrollments"
        SET "IsCompleted" = true,
            "CompletedAt" = COALESCE("CompletedAt", CURRENT_TIMESTAMP)
        WHERE "UserId" = p_user_id AND "CourseId" = p_course_id;
        
        RETURN true;
    ELSE
        UPDATE "Enrollments"
        SET "IsCompleted" = false,
            "CompletedAt" = NULL
        WHERE "UserId" = p_user_id AND "CourseId" = p_course_id;
    END IF;
    
    RETURN false;
END;
$$ LANGUAGE plpgsql;
