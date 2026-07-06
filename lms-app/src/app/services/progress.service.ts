import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '@environments/environment';
import { CourseProgressResponse, PagedStudentProgressResponse } from '@models/progress';
import { HttpParams } from '@angular/common/http';

@Injectable({
  providedIn: 'root',
})
export class ProgressService {
  private http = inject(HttpClient);
  private readonly baseUrl = environment.apiUrl;

  getCourseProgress(courseId: number): Observable<CourseProgressResponse> {
    return this.http.get<CourseProgressResponse>(`${this.baseUrl}/Progress/course/${courseId}`);
  }

  getStudentsProgress(courseId: number, page: number = 1, pageSize: number = 10, search?: string, isCompleted?: boolean): Observable<PagedStudentProgressResponse> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    if (search) params = params.set('search', search);
    if (isCompleted != null) params = params.set('isCompleted', isCompleted.toString());

    return this.http.get<PagedStudentProgressResponse>(`${this.baseUrl}/Progress/course/${courseId}/students`, { params });
  }

  getCourseAnalytics(courseId: number): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/Progress/course/${courseId}/analytics`);
  }
}
