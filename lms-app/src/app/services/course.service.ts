import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '@environments/environment';
import { CourseResponse, CategoryResponse, PagedCourseResponse, FiltersMetadataResponse, CourseSearchQuery, CourseDetailResponse, PagedInstructorCourseResponse, PagedCourseListResponse, CoursePreviewResponse } from '@models/course';

@Injectable({
  providedIn: 'root',
})
export class CourseService {
  private http = inject(HttpClient);
  private readonly baseUrl = environment.apiUrl;

  getMyCourses(query?: any): Observable<PagedInstructorCourseResponse> {
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
    return this.http.get<PagedInstructorCourseResponse>(`${this.baseUrl}/Courses/my-courses`, { params });
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

  softDeleteCourse(courseId: number): Observable<void> {
    return this.http.patch<void>(`${this.baseUrl}/Courses/${courseId}/soft-delete`, {});
  }

  getAllCourses(query: CourseSearchQuery): Observable<PagedCourseListResponse> {
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
    return this.http.get<PagedCourseListResponse>(`${this.baseUrl}/Courses`, { params });
  }

  getAllCategories(): Observable<CategoryResponse[]> {
    return this.http.get<CategoryResponse[]>(`${this.baseUrl}/CourseCategories`);
  }

  getFiltersMetadata(): Observable<FiltersMetadataResponse> {
    return this.http.get<FiltersMetadataResponse>(`${this.baseUrl}/Courses/filters-metadata`);
  }

  getCourseById(courseId: number): Observable<CourseDetailResponse | CoursePreviewResponse> {
    return this.http.get<CourseDetailResponse | CoursePreviewResponse>(`${this.baseUrl}/Courses/${courseId}`);
  }

  getCourseBySlug(slug: string): Observable<CourseDetailResponse | CoursePreviewResponse> {
    return this.http.get<CourseDetailResponse | CoursePreviewResponse>(`${this.baseUrl}/Courses/slug/${slug}`);
  }

  getInstructorCourseBySlug(slug: string): Observable<CourseDetailResponse> {
    return this.http.get<CourseDetailResponse>(`${this.baseUrl}/Courses/instructor/slug/${slug}`);
  }

  updateCourse(courseId: number, formData: FormData): Observable<CourseResponse> {
    return this.http.put<CourseResponse>(`${this.baseUrl}/Courses/${courseId}`, formData);
  }
}
