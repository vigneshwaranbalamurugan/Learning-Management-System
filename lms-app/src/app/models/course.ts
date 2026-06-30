import { QuizResponse } from './quiz';
import { AssignmentResponse } from './assignment';

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
  instructorEmail: string;
  instructorAvatarUrl?: string;
  lessonsCount: number;
  enrolledCount: number;
  completionRate: number;
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

export interface CourseSummaryStatsResponse {
  totalCourses: number;
  publishedCourses: number;
  pendingApproval: number;
  archivedCourses: number;
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

export interface ResourceResponse {
  id: number;
  lessonId: number;
  resourceType: number | string;
  resourceTitle: string;
  resourceUrl: string;
  description?: string;
  status: number | string;
  sortOrder: number;
  uploadedAt: string;
}

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
  content?: string;                // available for article lessons
  resources?: ResourceResponse[];
}

export interface CreateResourceRequest {
  lessonId: number;
  resourceType: number | string;
  resourceTitle: string;
  resourceUrl?: string;
  description?: string;
  status: number | string;
  file?: File;
}

export interface UpdateResourceRequest {
  resourceType?: number | string;
  resourceTitle?: string;
  resourceUrl?: string;
  description?: string;
  status?: number | string;
  file?: File;
}

export interface ReorderResourcesRequest {
  resources: { resourceId: number; sortOrder: number }[];
}

export interface CourseSectionDetail {
  id: number;
  courseId: number;
  title: string;
  description?: string;
  estimatedDuration: string;       // TimeSpan serialized as "HH:MM:SS"
  sortOrder: number;
  status: number | string;         // PublishStatus enum
  lessons?: LessonSummary[];       // Optional: populated if backend maps lessons in sections
  quizzes?: QuizResponse[];
  assignments?: AssignmentResponse[];
  resources?: ResourceResponse[];
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
