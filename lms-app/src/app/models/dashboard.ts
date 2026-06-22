export interface LearnerAnalytics {
  totalEnrolledCourses: number;
  completedCourses: number;
  inProgressCourses: number;
  averageProgressPercentage: number;
  averageQuizScore?: number;
  averageAssignmentScore?: number;
}

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

export interface CourseResponse {
  id: number;
  instructorId: number;
  categoryId: number;
  categoryName: string;
  title: string;
  slug: string;
  description?: string;
  price?: number;
  isPremium: boolean;
  thumbnailUrl?: string;
  level: number | string;
  languageId: number;
  languageName: string;
  status: number | string;
  publishedAt?: string;
  createdAt: string;
  updatedAt: string;
  courseAccessType: number | string;
  averageRating: number;
  totalReviews: number;
  // Enriched fields from backend
  instructorName: string;
  lessonsCount: number;
  enrolledCount: number;
  estimatedDuration: string; // TimeSpan serialized as "HH:MM:SS"
  hasCertificate: boolean;
}

export interface CategoryResponse {
  id: number;
  name: string;
  description: string;
}

export interface PagedCourseResponse {
  courses: CourseResponse[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export interface InstructorMetadata {
  id: number;
  fullName: string;
}

export interface LanguageMetadata {
  id: number;
  name: string;
}

export interface FiltersMetadataResponse {
  categories: CategoryResponse[];
  languages: LanguageMetadata[];
  instructors: InstructorMetadata[];
}

export interface CourseSearchQuery {
  categoryIds?: string;
  levels?: string;
  languageIds?: string;
  isPremium?: boolean | null;
  minRating?: number | null;
  durations?: string;
  instructorIds?: string;
  courseAccessTypes?: string;
  sortBy?: string;
  search?: string;
  pageNumber: number;
  pageSize: number;
  excludeCourseIds?: string;
}

// ─── Course Detail (GET /Courses/{id}) ──────────────────────────────────────

export interface LessonSummary {
  id: number;
  courseSectionId: number;
  title: string;
  description?: string;
  type: number | string;           // LessonType enum: Video, Article, Pdf, ExternalLink, Quiz
  durationInMinutes?: string;      // TimeSpan serialized as "HH:MM:SS"
  sortOrder: number;
  isPreview: boolean;
  status: number | string;         // PublishStatus enum
  contentUrl?: string;             // available for preview lessons
}

// CourseSectionDetail mirrors the backend SectionResponse DTO.
// Note: The backend CourseDetailsResponse.Sections is IEnumerable<SectionResponse> which
// maps CourseSection via AutoMapper convention. Lessons are included IF the mapper maps them
// (AutoMapper convention will map Lessons since both SectionResponse and the mapper map Lessons).
// We include lessons as optional to handle both cases.
export interface CourseSectionDetail {
  id: number;
  courseId: number;
  title: string;
  description?: string;
  estimatedDuration: string;       // TimeSpan serialized as "HH:MM:SS"
  sortOrder: number;
  status: number | string;         // PublishStatus enum
  lessons?: LessonSummary[];       // Optional: populated if backend maps lessons in sections
}

export interface BatchSummary {
  id: number;
  name: string;
  startDate?: string;
  endDate?: string;
  maxStudents?: number;
  enrolledCount?: number;
}

export interface CourseDetailResponse extends CourseResponse {
  introVideoUrl?: string;
  requirements?: string;           // raw text / multi-line string
  learningOutcomes?: string;       // raw text / multi-line string
  sections: CourseSectionDetail[];
  availableBatches: BatchSummary[];
  isWishlisted: boolean;
}

export interface CertificateResponse {
  id: number;
  certificateId: string;
  courseId: number;
  courseName: string;
  userId: number;
  learnerName: string;
  instructorName: string;
  certificateImageUrl: string;
  issuedAt: string;
  courseDescription: string;
  courseThumbnailUrl: string;
  courseLevel: string;
  courseDurationHours: number;
  categoryName: string;
}

// ─── Reviews ──────────────────────────────────────────────────────────────────

export interface ReviewResponse {
  id: number;
  courseId: number;
  userId: number;
  userName: string;
  rating: number;
  reviewText: string;
  createdAt: string;
  updatedAt: string;
}

export interface CreateReviewRequest {
  courseId: number;
  rating: number;
  reviewText: string;
}

export interface UpdateReviewRequest {
  rating?: number;
  reviewText?: string;
}

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
