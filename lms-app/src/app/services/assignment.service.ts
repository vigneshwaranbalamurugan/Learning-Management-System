import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '@environments/environment';

@Injectable({
  providedIn: 'root',
})
export class AssignmentService {
  private http = inject(HttpClient);
  private readonly baseUrl = environment.apiUrl;

  getAssignmentsBySection(sectionId: number): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/Assignments/section/${sectionId}`);
  }

  getAssignmentById(id: number): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/Assignments/${id}`);
  }

  getAssignment(id: number): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/Assignments/${id}`);
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

  getAssignmentUploadLimits(): Observable<{ maxFileSizeMB: number }> {
    return this.http.get<{ maxFileSizeMB: number }>(`${this.baseUrl}/Assignments/upload-limits`);
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

  reorderAssignments(assignmentOrders: { assignmentId: number; sortOrder: number }[]): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/Assignments/reorder`, { assignmentOrders });
  }
}
