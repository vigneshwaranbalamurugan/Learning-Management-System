import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '@environments/environment';
import { CourseProgressResponse } from '@models/progress';

@Injectable({
  providedIn: 'root',
})
export class ProgressService {
  private http = inject(HttpClient);
  private readonly baseUrl = environment.apiUrl;

  getCourseProgress(courseId: number): Observable<CourseProgressResponse> {
    return this.http.get<CourseProgressResponse>(`${this.baseUrl}/Progress/course/${courseId}`);
  }

  getStudentsProgress(courseId: number): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/Progress/course/${courseId}/students`);
  }

  getCourseAnalytics(courseId: number): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/Progress/course/${courseId}/analytics`);
  }
}
