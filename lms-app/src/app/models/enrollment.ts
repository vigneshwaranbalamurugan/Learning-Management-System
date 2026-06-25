export interface EnrollmentResponse {
  id: number;
  userId: number;
  courseId: number;
  courseTitle: string;
  batchId?: number;
  batchName?: string;
  enrolledAt: string;
  accessExpiresAt?: string;
  enrollmentStatus: number | string;
  progressPercentage: number;
  isCompleted: boolean;
  completedAt?: string;
  // Enriched course metadata
  thumbnailUrl?: string;
  instructorName: string;
  categoryName: string;
  languageName: string;
  level: number | string;
  lessonsCount: number;
  estimatedDuration: string;
  hasCertificate: boolean;
  courseAccessType: number | string;
}
