import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '@environments/environment';

export interface StudentProgressSummaryDto {
  enrollmentId: number;
  studentId: number;
  studentName: string;
  studentEmail: string;
  enrolledAt: string;
  enrollmentStatus: number; // Enum: Active = 1, Completed = 2, Suspended = 3, etc.
  progressPercentage: number;
  isCompleted: boolean;
  completedAt?: string;
  batchName?: string;
}

export interface LessonProgressDetails {
  id: number;
  userId: number;
  lessonId: number;
  lessonTitle: string;
  isCompleted: boolean;
  completedAt?: string;
  lastViewedAt: string;
  watchPercentage: number;
}

export interface QuizProgressDetails {
  quizId: number;
  quizTitle: string;
  isPassed: boolean;
  attemptsMade: number;
}

export interface AssignmentProgressDetails {
  assignmentId: number;
  assignmentTitle: string;
  isPassed: boolean;
  status: string; // e.g. "Submitted", "UnderReview", "Graded", "NotSubmitted"
}

export interface SectionProgressDetails {
  sectionId: number;
  title: string;
  progressPercentage: number;
  lessons: LessonProgressDetails[];
  quizzes: QuizProgressDetails[];
  assignments: AssignmentProgressDetails[];
}

export interface StudentCourseProgressResponse {
  courseId: number;
  progressPercentage: number;
  completedLessonsCount: number;
  totalLessonsCount: number;
  sections: SectionProgressDetails[];
}

@Injectable({
  providedIn: 'root'
})
export class InstructorProgressService {
  private http = inject(HttpClient);
  private readonly baseUrl = environment.apiUrl;

  getStudentsProgress(courseId: number): Observable<StudentProgressSummaryDto[]> {
    return this.http.get<StudentProgressSummaryDto[]>(`${this.baseUrl}/Progress/course/${courseId}/students`);
  }

  getStudentDetailedProgress(courseId: number, studentId: number): Observable<StudentCourseProgressResponse> {
    return this.http.get<StudentCourseProgressResponse>(`${this.baseUrl}/Progress/course/${courseId}/students/${studentId}/detail`);
  }
}
