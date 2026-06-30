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

export interface MonthlyRevenue {
  month: string;
  revenue: number;
}

export interface MonthlyTrend {
  month: string;
  count: number;
}

export interface RecentActivity {
  activityType: string;
  description: string;
  timestamp: string;
  userName: string;
}

export interface AdminAnalytics {
  totalUsers: number;
  totalLearners: number;
  totalInstructors: number;
  totalCourses: number;
  activeCourses: number;
  totalEnrollments: number;
  totalRevenue: number;
  totalCertificatesIssued: number;
  monthlyRevenue: MonthlyRevenue[];
  userGrowth: MonthlyTrend[];
  enrollmentTrend: MonthlyTrend[];
  recentActivities: RecentActivity[];
}

export interface InstructorAnalytics {
  totalCoursesCreated: number;
  totalStudentsEnrolled: number;
  totalRevenueGenerated: number;
  averageQuizScore?: number;
  averageAssignmentScore?: number;
  recentEnrollments: RecentEnrollment[];
}
