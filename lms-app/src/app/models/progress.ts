export interface LessonProgressResponse {
  id: number;
  lessonId: number;
  isCompleted: boolean;
  lastWatchedSecond?: number;
  maxWatchedSecond?: number;
  watchPercentage?: number;
  completedAt?: string;
  lessonTitle?: string;
}

export interface QuizProgressResponse {
  quizId: number;
  quizTitle?: string;
  isPassed: boolean;
  attemptsMade: number;
}

export interface AssignmentProgressResponse {
  assignmentId: number;
  assignmentTitle?: string;
  isPassed: boolean;
  status: string;
}

export interface SectionProgressResponse {
  sectionId: number;
  sectionTitle: string;
  progressPercentage: number;
  lessons: LessonProgressResponse[];
  quizzes: QuizProgressResponse[];
  assignments: AssignmentProgressResponse[];
}

export interface CourseProgressResponse {
  courseId: number;
  courseTitle?: string;
  progressPercentage: number;
  completedLessonsCount: number;
  totalLessonsCount: number;
  sections: SectionProgressResponse[];
}

export interface StudentProgressSummaryDto {
  enrollmentId: number;
  studentId: number;
  studentName: string;
  studentEmail: string;
  enrolledAt: string;
  enrollmentStatus: string;
  progressPercentage: number;
  isCompleted: boolean;
  completedAt?: string;
  batchName?: string;
}

export interface InstructorCourseProgressResponse {
  courseId: number;
  courseTitle: string;
  courseStatus: string;
  students: StudentProgressSummaryDto[];
}

export interface PagedStudentProgressResponse {
  students: StudentProgressSummaryDto[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  totalStudents: number;
  completedCount: number;
  averageProgress: number;
  courseId: number;
  courseTitle: string;
  courseStatus: string;
}
