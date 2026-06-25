export interface LearnerAnalytics {
  totalEnrolledCourses: number;
  completedCourses: number;
  inProgressCourses: number;
  averageProgressPercentage: number;
  averageQuizScore?: number;
  averageAssignmentScore?: number;
}

export interface RecentEnrollment {
  studentName: string;
  courseTitle: string;
  enrolledAt: string;
}

export interface InstructorAnalytics {
  totalCoursesCreated: number;
  totalStudentsEnrolled: number;
  totalRevenueGenerated: number;
  averageQuizScore?: number;
  averageAssignmentScore?: number;
  recentEnrollments: RecentEnrollment[];
}
