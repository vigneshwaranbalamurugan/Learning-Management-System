import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '@environments/environment';
import { AssignmentSubmissionResponse } from '@models/assignment';

import { HttpParams } from '@angular/common/http';
import { PagedInstructorAssignmentResponse, InstructorAssignmentSummaryDto } from '@models/assignment';

@Injectable({
  providedIn: 'root'
})
export class InstructorAssignmentService {
  private http = inject(HttpClient);
  private readonly baseUrl = environment.apiUrl;

  getInstructorAssignments(page: number = 1, pageSize: number = 10, search?: string, status?: number): Observable<PagedInstructorAssignmentResponse> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    if (search) params = params.set('search', search);
    if (status != null) params = params.set('status', status.toString());

    return this.http.get<PagedInstructorAssignmentResponse>(`${this.baseUrl}/Assignments/my-created`, { params });
  }

  getPendingSubmissions(assignmentId: number): Observable<AssignmentSubmissionResponse[]> {
    return this.http.get<AssignmentSubmissionResponse[]>(`${this.baseUrl}/AssignmentSubmissions/assignment/${assignmentId}/pending`);
  }

  getGradedSubmissions(assignmentId: number): Observable<AssignmentSubmissionResponse[]> {
    return this.http.get<AssignmentSubmissionResponse[]>(`${this.baseUrl}/AssignmentSubmissions/assignment/${assignmentId}/graded`);
  }

  getGradedSubmissionsWithDetails(assignmentId: number): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/AssignmentSubmissions/assignment/${assignmentId}/graded-with-details`);
  }

  gradeSubmission(submissionId: number, marksAwarded: number, feedback: string): Observable<AssignmentSubmissionResponse> {
    return this.http.put<AssignmentSubmissionResponse>(`${this.baseUrl}/AssignmentSubmissions/${submissionId}/grade`, {
      marksAwarded,
      feedback
    });
  }
}
