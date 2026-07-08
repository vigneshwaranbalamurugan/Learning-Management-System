CREATE OR REPLACE FUNCTION public.calculate_assignment_pass_status(p_submission_id integer)
 RETURNS boolean
 LANGUAGE plpgsql
AS $function$
DECLARE
    v_marks_awarded INTEGER;
    v_passing_marks INTEGER;
BEGIN
    -- Fetch the marks awarded for the given submission and the passing marks for the related assignment
    SELECT 
        s."MarksAwarded", 
        a."PassingMarks"
    INTO 
        v_marks_awarded, 
        v_passing_marks
    FROM "AssignmentSubmissions" s
    JOIN "Assignments" a ON s."AssignmentId" = a."Id"
    WHERE s."Id" = p_submission_id;

    -- If the submission has not been graded yet (marks awarded is null), return false
    IF v_marks_awarded IS NULL THEN
        RETURN FALSE;
    END IF;

    -- Return true if marks awarded is greater than or equal to passing marks
    RETURN v_marks_awarded >= v_passing_marks;
END;
$function$

CREATE OR REPLACE FUNCTION public.calculate_pass_status(p_attempt_id integer)
 RETURNS boolean
 LANGUAGE plpgsql
AS $function$
DECLARE
    v_score DOUBLE PRECISION;
    v_passing_percentage INT;
    v_quiz_id INT;
    v_total_marks DOUBLE PRECISION;
BEGIN
    SELECT "Score", "QuizId"
    INTO v_score, v_quiz_id
    FROM "QuizAttempts"
    WHERE "Id" = p_attempt_id;

    SELECT "PassingPercentage"
    INTO v_passing_percentage
    FROM "Quizzes"
    WHERE "Id" = v_quiz_id;

    SELECT COALESCE(SUM("Mark"), 0.0)
    INTO v_total_marks
    FROM "QuizQuestions"
    WHERE "QuizId" = v_quiz_id;

    IF v_total_marks = 0 THEN
        RETURN FALSE;
    END IF;

    RETURN (v_score / v_total_marks) * 100 >= v_passing_percentage;
END;
$function$

CREATE OR REPLACE FUNCTION public.calculate_quiz_score(p_attempt_id integer)
 RETURNS double precision
 LANGUAGE plpgsql
AS $function$
DECLARE
    v_score DOUBLE PRECISION;
BEGIN
    SELECT COALESCE(SUM(qq."Mark"), 0.0)
    INTO v_score
    FROM "QuizAnswers" qa
    INNER JOIN "QuizQuestions" qq
        ON qq."Id" = qa."QuestionId"
    INNER JOIN "QuizOptions" qo
        ON qo."Id" = qa."SelectedOptionId"
    WHERE qa."AttemptId" = p_attempt_id
      AND qo."IsCorrect" = TRUE;

    RETURN v_score;
END;
$function$

CREATE OR REPLACE FUNCTION public.get_batch_available_seats(p_batch_id integer)
 RETURNS integer
 LANGUAGE plpgsql
AS $function$
DECLARE
    v_max_students INT;
    v_enrolled_count INT;
BEGIN
    SELECT "MaxStudents"
    INTO v_max_students
    FROM "CourseBatches"
    WHERE "Id" = p_batch_id;

    IF NOT FOUND THEN
        RAISE EXCEPTION 'Batch with id % not found', p_batch_id;
    END IF;

    SELECT COUNT(*)
    INTO v_enrolled_count
    FROM "Enrollments"
    WHERE "BatchId" = p_batch_id;

    RETURN GREATEST(0, v_max_students - v_enrolled_count);
END;
$function$

CREATE OR REPLACE FUNCTION public.get_course_rating_stats(p_course_id integer)
 RETURNS TABLE(averagerating double precision, totalreviews integer)
 LANGUAGE plpgsql
AS $function$
BEGIN
    RETURN QUERY
    SELECT 
        COALESCE(AVG("Rating")::DOUBLE PRECISION, 0.0::DOUBLE PRECISION) AS AverageRating,
        COUNT(*)::INTEGER AS TotalReviews
    FROM "Reviews"
    WHERE "CourseId" = p_course_id;
END;
$function$

CREATE OR REPLACE FUNCTION public.get_remaining_attempts(p_quiz_id integer, p_user_id integer)
 RETURNS integer
 LANGUAGE plpgsql
AS $function$
DECLARE
    v_max_attempts INT;
    v_attempt_count INT;
BEGIN
    SELECT "MaxAttempts"
    INTO v_max_attempts
    FROM "Quizzes"
    WHERE "Id" = p_quiz_id;

    SELECT COUNT(*)
    INTO v_attempt_count
    FROM "QuizAttempts"
    WHERE "QuizId" = p_quiz_id
      AND "UserId" = p_user_id
      AND "Status" != 'Expired';

    RETURN GREATEST(0, v_max_attempts - v_attempt_count);
END;
$function$

CREATE OR REPLACE FUNCTION public.get_submission_attempt_count(p_assignment_id integer, p_student_id integer)
 RETURNS integer
 LANGUAGE plpgsql
AS $function$
DECLARE
    v_attempt_count INT;
BEGIN
    SELECT COUNT(*)
    INTO v_attempt_count
    FROM "AssignmentSubmissions"
    WHERE "AssignmentId" = p_assignment_id
      AND "StudentId" = p_student_id;

    RETURN v_attempt_count;
END;
$function$

CREATE OR REPLACE FUNCTION public.get_upcoming_deadlines(target_date date)
 RETURNS TABLE(userid integer, useremail character varying, username character varying, coursename character varying, itemtype character varying, itemtitle character varying, deadlinedate date)
 LANGUAGE plpgsql
AS $function$
BEGIN
    RETURN QUERY
    -- For Assignments
    SELECT 
        u."Id" AS UserId,
        u."Email" AS UserEmail,
        (up."FirstName" || ' ' || COALESCE(up."LastName", ''))::VARCHAR AS UserName,
        c."Title" AS CourseName,
        'Assignment'::VARCHAR AS ItemType,
        a."Title" AS ItemTitle,
        (e."EnrolledAt" + (a."DeadlineInDays" * INTERVAL '1 day'))::DATE AS DeadlineDate
    FROM "Enrollments" e
JOIN "Users" u
ON e."UserId" = u."Id"
JOIN "UserProfiles" up
ON up."UserId" = u."Id"
    JOIN "Courses" c ON e."CourseId" = c."Id"
    JOIN "CourseSections" cs ON c."Id" = cs."CourseId"
    JOIN "Assignments" a ON cs."Id" = a."CourseSectionId"
    WHERE a."DeadlineInDays" > 0
      AND (e."EnrolledAt" + (a."DeadlineInDays" * INTERVAL '1 day'))::DATE = target_date
      AND e."EnrollmentStatus" = 1 -- Assuming 0 is Active
      AND NOT EXISTS (
          SELECT 1 FROM "AssignmentSubmissions" sub 
          WHERE sub."AssignmentId" = a."Id" AND sub."StudentId" = u."Id"
      )

    UNION ALL

    -- For Quizzes
    SELECT 
        u."Id" AS UserId,
        u."Email" AS UserEmail,
        (up."FirstName" || ' ' || COALESCE(up."LastName", ''))::VARCHAR AS UserName,
        c."Title" AS CourseName,
        'Quiz'::VARCHAR AS ItemType,
        q."Title" AS ItemTitle,
        (e."EnrolledAt" + (q."DeadlineInDays" * INTERVAL '1 day'))::DATE AS DeadlineDate
    FROM "Enrollments" e
    JOIN "Users" u ON e."UserId" = u."Id"
   JOIN "UserProfiles" up
ON up."UserId" = u."Id" 
    JOIN "Courses" c ON e."CourseId" = c."Id"
    JOIN "CourseSections" cs ON c."Id" = cs."CourseId"
    JOIN "Quizzes" q ON cs."Id" = q."CourseSectionId"
    WHERE q."DeadlineInDays" > 0
      AND (e."EnrolledAt" + (q."DeadlineInDays" * INTERVAL '1 day'))::DATE = target_date
      AND e."EnrollmentStatus" = 0
      AND NOT EXISTS (
          SELECT 1 FROM "QuizAttempts" qa 
          WHERE qa."QuizId" = q."Id" AND qa."UserId" = u."Id" AND (qa."Status" = 'Submitted' OR qa."IsPassed" = true) -- Assuming 2 is Completed
      );
END;
$function$

