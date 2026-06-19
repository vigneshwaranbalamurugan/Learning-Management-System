using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMSApi.DALLibrary.Migrations
{
    /// <inheritdoc />
    public partial class AddDeadlineDateToQuizAndAssignment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeadlineDate",
                table: "Quizzes",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeadlineDate",
                table: "Assignments",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 18, 7, 17, 17, 197, DateTimeKind.Utc).AddTicks(6510), new DateTime(2026, 6, 18, 7, 17, 17, 197, DateTimeKind.Utc).AddTicks(6510) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 18, 7, 17, 17, 197, DateTimeKind.Utc).AddTicks(6520), new DateTime(2026, 6, 18, 7, 17, 17, 197, DateTimeKind.Utc).AddTicks(6520) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 18, 7, 17, 17, 197, DateTimeKind.Utc).AddTicks(6520), new DateTime(2026, 6, 18, 7, 17, 17, 197, DateTimeKind.Utc).AddTicks(6520) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 18, 7, 17, 17, 197, DateTimeKind.Utc).AddTicks(3120), new DateTime(2026, 6, 18, 7, 17, 17, 197, DateTimeKind.Utc).AddTicks(3130) });

            migrationBuilder.Sql(@"
CREATE OR REPLACE FUNCTION public.get_upcoming_deadlines(target_date date)
 RETURNS TABLE(userid integer, useremail character varying, username character varying, coursename character varying, itemtype character varying, itemtitle character varying, deadlinedate date)
 LANGUAGE plpgsql
AS $function$
BEGIN
    RETURN QUERY
    -- For Assignments
    SELECT 
        u.""Id"" AS UserId,
        u.""Email"" AS UserEmail,
        (u.""FirstName"" || ' ' || COALESCE(u.""LastName"", ''))::VARCHAR AS UserName,
        c.""Title"" AS CourseName,
        'Assignment'::VARCHAR AS ItemType,
        a.""Title"" AS ItemTitle,
        CASE 
            WHEN c.""CourseAccessType"" = 1 THEN (e.""EnrolledAt"" + (a.""DeadlineInDays"" * INTERVAL '1 day'))::DATE
            WHEN c.""CourseAccessType"" = 2 THEN a.""DeadlineDate""::DATE
            ELSE NULL
        END AS DeadlineDate
    FROM ""Enrollments"" e
    JOIN ""Users"" u ON e.""UserId"" = u.""Id""
    JOIN ""Courses"" c ON e.""CourseId"" = c.""Id""
    JOIN ""CourseSection"" cs ON c.""Id"" = cs.""CourseId""
    JOIN ""Assignments"" a ON cs.""Id"" = a.""CourseSectionId""
    WHERE e.""EnrollmentStatus"" = 1
      AND (
          (c.""CourseAccessType"" = 1 AND a.""DeadlineInDays"" > 0 AND (e.""EnrolledAt"" + (a.""DeadlineInDays"" * INTERVAL '1 day'))::DATE = target_date)
          OR
          (c.""CourseAccessType"" = 2 AND a.""DeadlineDate"" IS NOT NULL AND a.""DeadlineDate""::DATE = target_date)
      )
      AND NOT EXISTS (
          SELECT 1 FROM ""AssignmentSubmissions"" sub 
          WHERE sub.""AssignmentId"" = a.""Id"" AND sub.""StudentId"" = u.""Id""
      )

    UNION ALL

    -- For Quizzes
    SELECT 
        u.""Id"" AS UserId,
        u.""Email"" AS UserEmail,
        (u.""FirstName"" || ' ' || COALESCE(u.""LastName"", ''))::VARCHAR AS UserName,
        c.""Title"" AS CourseName,
        'Quiz'::VARCHAR AS ItemType,
        q.""Title"" AS ItemTitle,
        CASE 
            WHEN c.""CourseAccessType"" = 1 THEN (e.""EnrolledAt"" + (q.""DeadlineInDays"" * INTERVAL '1 day'))::DATE
            WHEN c.""CourseAccessType"" = 2 THEN q.""DeadlineDate""::DATE
            ELSE NULL
        END AS DeadlineDate
    FROM ""Enrollments"" e
    JOIN ""Users"" u ON e.""UserId"" = u.""Id""
    JOIN ""Courses"" c ON e.""CourseId"" = c.""Id""
    JOIN ""CourseSection"" cs ON c.""Id"" = cs.""CourseId""
    JOIN ""Quzzes"" q ON cs.""Id"" = q.""CourseSectionId""
    WHERE e.""EnrollmentStatus"" = 1
      AND (
          (c.""CourseAccessType"" = 1 AND q.""DeadlineInDays"" > 0 AND (e.""EnrolledAt"" + (q.""DeadlineInDays"" * INTERVAL '1 day'))::DATE = target_date)
          OR
          (c.""CourseAccessType"" = 2 AND q.""DeadlineDate"" IS NOT NULL AND q.""DeadlineDate""::DATE = target_date)
      )
      AND NOT EXISTS (
          SELECT 1 FROM ""QuizAttempts"" qa 
          WHERE qa.""QuizId"" = q.""Id"" AND qa.""UserId"" = u.""Id"" AND (qa.""Status"" = 2 OR qa.""IsPassed"" = true)
      );
END;
$function$;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeadlineDate",
                table: "Quizzes");

            migrationBuilder.DropColumn(
                name: "DeadlineDate",
                table: "Assignments");

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 17, 17, 54, 3, 445, DateTimeKind.Utc).AddTicks(7940), new DateTime(2026, 6, 17, 17, 54, 3, 445, DateTimeKind.Utc).AddTicks(7940) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 17, 17, 54, 3, 445, DateTimeKind.Utc).AddTicks(7940), new DateTime(2026, 6, 17, 17, 54, 3, 445, DateTimeKind.Utc).AddTicks(7940) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 17, 17, 54, 3, 445, DateTimeKind.Utc).AddTicks(7950), new DateTime(2026, 6, 17, 17, 54, 3, 445, DateTimeKind.Utc).AddTicks(7950) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 17, 17, 54, 3, 445, DateTimeKind.Utc).AddTicks(4260), new DateTime(2026, 6, 17, 17, 54, 3, 445, DateTimeKind.Utc).AddTicks(4260) });

            migrationBuilder.Sql(@"
CREATE OR REPLACE FUNCTION public.get_upcoming_deadlines(target_date date)
 RETURNS TABLE(userid integer, useremail character varying, username character varying, coursename character varying, itemtype character varying, itemtitle character varying, deadlinedate date)
 LANGUAGE plpgsql
AS $function$
BEGIN
    RETURN QUERY
    -- For Assignments
    SELECT 
        u.""Id"" AS UserId,
        u.""Email"" AS UserEmail,
        (u.""FirstName"" || ' ' || COALESCE(u.""LastName"", ''))::VARCHAR AS UserName,
        c.""Title"" AS CourseName,
        'Assignment'::VARCHAR AS ItemType,
        a.""Title"" AS ItemTitle,
        (e.""EnrolledAt"" + (a.""DeadlineInDays"" * INTERVAL '1 day'))::DATE AS DeadlineDate
    FROM ""Enrollments"" e
    JOIN ""Users"" u ON e.""UserId"" = u.""Id""
    JOIN ""Courses"" c ON e.""CourseId"" = c.""Id""
    JOIN ""CourseSection"" cs ON c.""Id"" = cs.""CourseId""
    JOIN ""Assignments"" a ON cs.""Id"" = a.""CourseSectionId""
    WHERE a.""DeadlineInDays"" > 0
      AND (e.""EnrolledAt"" + (a.""DeadlineInDays"" * INTERVAL '1 day'))::DATE = target_date
      AND e.""EnrollmentStatus"" = 1
      AND NOT EXISTS (
          SELECT 1 FROM ""AssignmentSubmissions"" sub 
          WHERE sub.""AssignmentId"" = a.""Id"" AND sub.""StudentId"" = u.""Id""
      )

    UNION ALL

    -- For Quizzes
    SELECT 
        u.""Id"" AS UserId,
        u.""Email"" AS UserEmail,
        (u.""FirstName"" || ' ' || COALESCE(u.""LastName"", ''))::VARCHAR AS UserName,
        c.""Title"" AS CourseName,
        'Quiz'::VARCHAR AS ItemType,
        q.""Title"" AS ItemTitle,
        (e.""EnrolledAt"" + (q.""DeadlineInDays"" * INTERVAL '1 day'))::DATE AS DeadlineDate
    FROM ""Enrollments"" e
    JOIN ""Users"" u ON e.""UserId"" = u.""Id""
    JOIN ""Courses"" c ON e.""CourseId"" = c.""Id""
    JOIN ""CourseSection"" cs ON c.""Id"" = cs.""CourseId""
    JOIN ""Quzzes"" q ON cs.""Id"" = q.""CourseSectionId""
    WHERE q.""DeadlineInDays"" > 0
      AND (e.""EnrolledAt"" + (q.""DeadlineInDays"" * INTERVAL '1 day'))::DATE = target_date
      AND e.""EnrollmentStatus"" = 0
      AND NOT EXISTS (
          SELECT 1 FROM ""QuizAttempts"" qa 
          WHERE qa.""QuizId"" = q.""Id"" AND qa.""UserId"" = u.""Id"" AND (qa.""Status"" = 2 OR qa.""IsPassed"" = true)
      );
END;
$function$;
");
        }
    }
}
