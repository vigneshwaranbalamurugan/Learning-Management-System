export interface LessonProgressResponse {
  id: number;
  lessonId: number;
  isCompleted: boolean;
  lastWatchedSecond?: number;
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
  courseTitle: string;
  progressPercentage: number;
  completedLessonsCount: number;
  totalLessonsCount: number;
  sections: SectionProgressResponse[];
}
