import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '@environments/environment';
import { 
  LearnerAnalytics, 
  EnrollmentResponse, 
  InstructorAnalytics, 
  CourseResponse,
  CategoryResponse,
  PagedCourseResponse,
  FiltersMetadataResponse,
  CourseSearchQuery,
  CertificateResponse,
  CourseDetailResponse,
  ReviewResponse,
  CreateReviewRequest,
  UpdateReviewRequest,
  CourseProgressResponse
} from '@models/dashboard';
import { QuizAttemptResponse, QuizAttemptDetailResponse } from '@models/quiz';

@Injectable({
  providedIn: 'root',
})
export class DashboardService {
  private http = inject(HttpClient);
  private readonly baseUrl = environment.apiUrl;

  getLearnerAnalytics(): Observable<LearnerAnalytics> {
    return this.http.get<LearnerAnalytics>(`${this.baseUrl}/analytics/learner`);
  }

  getMyEnrollments(): Observable<EnrollmentResponse[]> {
    return this.http.get<EnrollmentResponse[]>(`${this.baseUrl}/enrollments/my`);
  }

  getInstructorAnalytics(): Observable<InstructorAnalytics> {
    return this.http.get<InstructorAnalytics>(`${this.baseUrl}/analytics/instructor`);
  }

  getMyCourses(): Observable<CourseResponse[]> {
    return this.http.get<CourseResponse[]>(`${this.baseUrl}/Courses/my-courses`);
  }

  getAllCourses(query: CourseSearchQuery): Observable<PagedCourseResponse> {
    let params = new HttpParams();
    if (query.categoryIds)      params = params.set('categoryIds', query.categoryIds);
    if (query.levels)           params = params.set('levels', query.levels);
    if (query.languageIds)      params = params.set('languageIds', query.languageIds);
    if (query.isPremium != null) params = params.set('isPremium', query.isPremium.toString());
    if (query.minRating != null) params = params.set('minRating', query.minRating.toString());
    if (query.durations)        params = params.set('durations', query.durations);
    if (query.instructorIds)    params = params.set('instructorIds', query.instructorIds);
    if (query.courseAccessTypes) params = params.set('courseAccessTypes', query.courseAccessTypes);
    if (query.sortBy)           params = params.set('sortBy', query.sortBy);
    if (query.search)           params = params.set('search', query.search);
    if (query.excludeCourseIds) params = params.set('excludeCourseIds', query.excludeCourseIds);
    params = params.set('pageNumber', query.pageNumber.toString());
    params = params.set('pageSize', query.pageSize.toString());
    return this.http.get<PagedCourseResponse>(`${this.baseUrl}/Courses`, { params });
  }

  getAllCategories(): Observable<CategoryResponse[]> {
    return this.http.get<CategoryResponse[]>(`${this.baseUrl}/CourseCategories`);
  }

  getFiltersMetadata(): Observable<FiltersMetadataResponse> {
    return this.http.get<FiltersMetadataResponse>(`${this.baseUrl}/Courses/filters-metadata`);
  }

  enrollFreeCourse(courseId: number): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/courses/${courseId}/enroll/free`, {});
  }

  getCourseById(courseId: number): Observable<CourseDetailResponse> {
    return this.http.get<CourseDetailResponse>(`${this.baseUrl}/Courses/${courseId}`);
  }

  // ── Reviews ───────────────────────────────────────────────────────────────

  getCourseReviews(courseId: number): Observable<ReviewResponse[]> {
    return this.http.get<ReviewResponse[]>(`${this.baseUrl}/Reviews/course/${courseId}`);
  }

  submitReview(request: CreateReviewRequest): Observable<ReviewResponse> {
    return this.http.post<ReviewResponse>(`${this.baseUrl}/Reviews`, request);
  }

  updateReview(reviewId: number, request: UpdateReviewRequest): Observable<ReviewResponse> {
    return this.http.put<ReviewResponse>(`${this.baseUrl}/Reviews/${reviewId}`, request);
  }

  deleteReview(reviewId: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/Reviews/${reviewId}`);
  }

  getMyCertificates(): Observable<CertificateResponse[]> {
    return this.http.get<CertificateResponse[]>(`${this.baseUrl}/certificates/my`);
  }

  verifyCertificate(certificateId: string): Observable<CertificateResponse> {
    return this.http.get<CertificateResponse>(`${this.baseUrl}/certificates/verify/${certificateId}`);
  }

  getMyQuizAttempts(): Observable<QuizAttemptResponse[]> {
    return this.http.get<QuizAttemptResponse[]>(`${this.baseUrl}/QuizAttempts/my`);
  }

  getQuizAttemptDetail(attemptId: number): Observable<QuizAttemptDetailResponse> {
    return this.http.get<QuizAttemptDetailResponse>(`${this.baseUrl}/QuizAttempts/${attemptId}`);
  }

  getCourseProgress(courseId: number): Observable<CourseProgressResponse> {
    return this.http.get<CourseProgressResponse>(`${this.baseUrl}/Progress/course/${courseId}`);
  }

  // ── Assignments & Submissions ─────────────────────────────────────────────

  getAssignmentsBySection(sectionId: number): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/Assignments/section/${sectionId}`);
  }

  getAssignmentById(id: number): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/Assignments/${id}`);
  }

  submitAssignment(formData: FormData): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/AssignmentSubmissions`, formData);
  }

  getAssignmentSubmissions(assignmentId: number): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/AssignmentSubmissions/assignment/${assignmentId}/my-submissions`);
  }

  getAssignmentStatus(assignmentId: number): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/AssignmentSubmissions/assignment/${assignmentId}/status`);
  }
}
