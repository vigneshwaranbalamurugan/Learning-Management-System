import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '@environments/environment';

@Injectable({
  providedIn: 'root',
})
export class CourseBuilderService {
  private http = inject(HttpClient);
  private readonly baseUrl = environment.apiUrl;

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

  reorderSections(sectionOrders: { sectionId: number; sortOrder: number }[]): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/CourseSections/reorder`, { sectionOrders });
  }

  reorderLessons(lessonOrders: { lessonId: number; sortOrder: number }[]): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/Lessons/reorder`, { lessonOrders });
  }
}
