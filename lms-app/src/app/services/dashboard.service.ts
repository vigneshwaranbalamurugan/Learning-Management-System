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

  getMyCourses(query?: any): Observable<PagedCourseResponse> {
    let params = new HttpParams();
    if (query) {
      if (query.categoryIds)        params = params.set('categoryIds', query.categoryIds);
      if (query.levels)             params = params.set('levels', query.levels);
      if (query.languageIds)        params = params.set('languageIds', query.languageIds);
      if (query.sortBy)             params = params.set('sortBy', query.sortBy);
      if (query.search)             params = params.set('search', query.search);
      if (query.statuses)           params = params.set('statuses', query.statuses);
      if (query.pageNumber != null) params = params.set('pageNumber', query.pageNumber.toString());
      if (query.pageSize != null)   params = params.set('pageSize', query.pageSize.toString());
    } else {
      params = params.set('pageNumber', '1');
      params = params.set('pageSize', '1000');
    }
    return this.http.get<PagedCourseResponse>(`${this.baseUrl}/Courses/my-courses`, { params });
  }

  createCourse(formData: FormData): Observable<CourseResponse> {
    return this.http.post<CourseResponse>(`${this.baseUrl}/Courses`, formData);
  }

  publishCourse(courseId: number, publish: boolean): Observable<CourseResponse> {
    return this.http.patch<CourseResponse>(`${this.baseUrl}/Courses/${courseId}/publish`, { publish });
  }

  archiveCourse(courseId: number, archive: boolean): Observable<CourseResponse> {
    return this.http.patch<CourseResponse>(`${this.baseUrl}/Courses/${courseId}/archive`, { archive });
  }

  deleteCourse(courseId: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/Courses/${courseId}`);
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

  getCourseBySlug(slug: string): Observable<CourseDetailResponse> {
    return this.http.get<CourseDetailResponse>(`${this.baseUrl}/Courses/slug/${slug}`);
  }

  getInstructorCourseBySlug(slug: string): Observable<CourseDetailResponse> {
    return this.http.get<CourseDetailResponse>(`${this.baseUrl}/Courses/instructor/slug/${slug}`);
  }

  updateCourse(courseId: number, formData: FormData): Observable<CourseResponse> {
    return this.http.put<CourseResponse>(`${this.baseUrl}/Courses/${courseId}`, formData);
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

  // ── Course Builder (Sections & Lessons) ───────────────────────────────────

  createSection(data: { courseId: number; title: string; description?: string; estimatedDuration: string; sortOrder?: number }): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/CourseSections`, data);
  }

  updateSection(id: number, data: { title?: string; description?: string; estimatedDuration?: string; sortOrder?: number; status?: number }): Observable<any> {
    return this.http.put<any>(`${this.baseUrl}/CourseSections/${id}`, data);
  }

  deleteSection(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/CourseSections/${id}`);
  }

  getLesson(id: number): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/Lessons/${id}/detail`);
  }

  createLesson(formData: FormData): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/Lessons`, formData);
  }

  updateLesson(id: number, formData: FormData): Observable<any> {
    return this.http.put<any>(`${this.baseUrl}/Lessons/${id}`, formData);
  }

  deleteLesson(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/Lessons/${id}`);
  }

  getLessonUploadLimits(): Observable<{ videoMaxFileSizeMB: number, pdfMaxFileSizeMB: number }> {
    return this.http.get<{ videoMaxFileSizeMB: number, pdfMaxFileSizeMB: number }>(`${this.baseUrl}/Lessons/upload-limits`);
  }

  // ── Quizzes ───────────────────────────────────────────────────────────────

  getQuiz(id: number): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/Quizzes/${id}`);
  }

  createQuiz(data: any): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/Quizzes`, data);
  }

  updateQuiz(id: number, data: any): Observable<any> {
    return this.http.put<any>(`${this.baseUrl}/Quizzes/${id}`, data);
  }

  deleteQuiz(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/Quizzes/${id}`);
  }

  // ── Quiz Questions ────────────────────────────────────────────────────────

  addQuizQuestion(quizId: number, data: any): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/QuizQuestions/quiz/${quizId}`, data);
  }

  updateQuizQuestion(questionId: number, data: any): Observable<any> {
    return this.http.put<any>(`${this.baseUrl}/QuizQuestions/${questionId}`, data);
  }

  deleteQuizQuestion(questionId: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/QuizQuestions/${questionId}`);
  }

  reorderQuizQuestions(quizId: number, items: { questionId: number; sortOrder: number }[]): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/QuizQuestions/quiz/${quizId}/reorder`, { items });
  }

  // ── Reordering ────────────────────────────────────────────────────────────

  reorderSections(sectionOrders: { sectionId: number; sortOrder: number }[]): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/CourseSections/reorder`, { sectionOrders });
  }
  reorderLessons(lessonOrders: { lessonId: number; sortOrder: number }[]): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/Lessons/reorder`, { lessonOrders });
  }
  reorderQuizzes(quizOrders: { quizId: number; sortOrder: number }[]): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/Quizzes/reorder`, { quizOrders });
  }
  reorderAssignments(assignmentOrders: { assignmentId: number; sortOrder: number }[]): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/Assignments/reorder`, { assignmentOrders });
  }

  // ── Assignments ───────────────────────────────────────────────────────────

  getAssignment(id: number): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/Assignments/${id}`);
  }

  getAssignmentUploadLimits(): Observable<{ maxFileSizeMB: number }> {
    return this.http.get<{ maxFileSizeMB: number }>(`${this.baseUrl}/Assignments/upload-limits`);
  }

  createAssignment(formData: FormData): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/Assignments`, formData);
  }

  updateAssignment(id: number, formData: FormData): Observable<any> {
    return this.http.put<any>(`${this.baseUrl}/Assignments/${id}`, formData);
  }

  deleteAssignment(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/Assignments/${id}`);
  }

  getStudentsProgress(courseId: number): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/Progress/course/${courseId}/students`);
  }
}
