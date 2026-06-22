import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '@environments/environment';
import { AssignmentSubmissionResponse } from '@models/assignment';

export interface InstructorAssignmentSummaryDto {
  id: number;
  courseSectionId: number;
  title: string;
  courseTitle: string;
  sectionTitle: string;
  totalMarks: number;
  deadlineInDays: number;
  deadlineDate?: string;
  pendingSubmissionsCount: number;
  status: number;
  createdAt: string;
}

@Injectable({
  providedIn: 'root'
})
export class InstructorAssignmentService {
  private http = inject(HttpClient);
  private readonly baseUrl = environment.apiUrl;

  getInstructorAssignments(): Observable<InstructorAssignmentSummaryDto[]> {
    return this.http.get<InstructorAssignmentSummaryDto[]>(`${this.baseUrl}/Assignments/my-created`);
  }

  getPendingSubmissions(assignmentId: number): Observable<AssignmentSubmissionResponse[]> {
    return this.http.get<AssignmentSubmissionResponse[]>(`${this.baseUrl}/AssignmentSubmissions/assignment/${assignmentId}/pending`);
  }

  gradeSubmission(submissionId: number, marksAwarded: number, feedback: string): Observable<AssignmentSubmissionResponse> {
    return this.http.put<AssignmentSubmissionResponse>(`${this.baseUrl}/AssignmentSubmissions/${submissionId}/grade`, {
      marksAwarded,
      feedback
    });
  }
}
