import { QuizResponse } from './quiz';
import { ReviewResponse } from './review';
import { AssignmentResponse } from './assignment';
import { CourseAccessType } from '../enums/course-access-type.enum';
import { CourseStatus } from '../enums/course-status.enum';
import { LessonType } from '../enums/lesson-types.enum';
import { PublishStatus } from '../enums/publish-status.enum';
import { ResourceType } from '../enums/resource-type.enum';
import { CourseLevel } from '../enums/course-level.enum';

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
  level: CourseLevel | string;
  languageId: number;
  languageName: string;
  status: CourseStatus | string;
  publishedAt?: string;
  createdAt: string;
  updatedAt: string;
  courseAccessType: CourseAccessType | string;
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
  // User specific data
  isEnrolled: boolean;
  enrollmentProgress: number;
  enrollmentId?: number;
  reviews: ReviewResponse[];
  hasNonExpiredEnrollments?: boolean;
  hasActiveEnrollments?: boolean;
  isDeleted?: boolean;
  versionNumber?: string;
  isBeingUpdated?: boolean;
  hasNewerVersion?: boolean;
  hasDraftChanges?: boolean;
}

export interface CategoryResponse {
  id: number;
  name: string;
  description: string;
}

export interface CourseListItemResponse {
  id: number;
  title: string;
  slug: string;
  thumbnailUrl?: string;
  categoryName: string;
  instructorName: string;
  languageName: string;
  level: CourseLevel | string;
  status: CourseStatus | string;
  price?: number;
  isPremium: boolean;
  courseAccessType: CourseAccessType | string;
  averageRating: number;
  totalReviews: number;
  lessonsCount: number;
  enrolledCount: number;
  estimatedDuration: string;
  hasCertificate: boolean;
  createdAt: string;
}

export interface InstructorCourseCardResponse {
  id: number;
  title: string;
  slug: string;
  thumbnailUrl?: string;
  status: CourseStatus | string;
  averageRating: number;
  totalReviews: number;
  enrolledCount: number;
  createdAt: string;
  updatedAt: string;
  hasNonExpiredEnrollments?: boolean;
  hasActiveEnrollments?: boolean;
  isDeleted?: boolean;
  versionNumber?: string;
  isBeingUpdated?: boolean;
}

export interface PagedCourseResponse {
  courses: CourseResponse[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export interface PagedCourseListResponse {
  courses: CourseListItemResponse[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export interface PagedInstructorCourseResponse {
  courses: InstructorCourseCardResponse[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export interface CourseSummaryStatsResponse {
  totalCourses: number;
  publishedCourses: number;
  pendingApproval: number;
  updatesPending: number;
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
  resourceType: ResourceType | string;
  resourceTitle: string;
  resourceUrl: string;
  description?: string;
  status: PublishStatus | string;
  sortOrder: number;
  uploadedAt: string;
}

export interface LessonSummary {
  id: number;
  courseSectionId: number;
  title: string;
  description?: string;
  type: LessonType | string;           // LessonType enum: Video, Article, Pdf, ExternalLink, Quiz
  durationInMinutes?: string;      // TimeSpan serialized as "HH:MM:SS"
  sortOrder: number;
  isPreview: boolean;
  status: PublishStatus | string;         // PublishStatus enum
  contentUrl?: string;             // available for preview lessons
  content?: string;                // available for article lessons
  resources?: ResourceResponse[];
}

export interface CreateResourceRequest {
  lessonId: number;
  resourceType: ResourceType | string;
  resourceTitle: string;
  resourceUrl?: string;
  description?: string;
  status: PublishStatus | string;
  file?: File;
}

export interface UpdateResourceRequest {
  resourceType?: ResourceType | string;
  resourceTitle?: string;
  resourceUrl?: string;
  description?: string;
  status?: PublishStatus | string;
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
  status: PublishStatus | string;         // PublishStatus enum
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

export interface CourseLessonPreviewResponse {
  id: number;
  courseSectionId: number;
  title: string;
  description?: string;
  type: LessonType | string;
  durationInMinutes?: string;
  sortOrder: number;
  isPreview: boolean;
  status: PublishStatus | string;
  contentUrl?: string;
  content?: string;
  resources?: ResourceResponse[];
}

export interface CourseSectionPreviewResponse {
  id: number;
  courseId: number;
  title: string;
  description?: string;
  estimatedDuration: string;
  sortOrder: number;
  status: PublishStatus | string;
  lessons: CourseLessonPreviewResponse[];
  quizzes: QuizResponse[];
  assignments: AssignmentResponse[];
}

export interface CoursePreviewResponse extends CourseResponse {
  introVideoUrl?: string;
  requirements?: string;
  learningOutcomes?: string;
  estimatedDuration: string;
  sections: CourseSectionPreviewResponse[];
  availableBatches: BatchSummary[];
  isWishlisted: boolean;
}
