import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '@environments/environment';
import { LessonProgressResponse } from '@models/progress';
import {
  QuizStudentDetailResponse,
  StartAttemptResponse,
  SubmitQuizRequest,
  QuizAttemptResponse,
  GetRemainingAttemptsResponse
} from '@models/quiz';

@Injectable({
  providedIn: 'root'
})
export class LearningService {
  private http = inject(HttpClient);
  private readonly baseUrl = environment.apiUrl;

  // ── Lessons ──────────────────────────────────────────────────────────────

  markLessonComplete(lessonId: number, watchPercentage?: number): Observable<LessonProgressResponse> {
    return this.http.post<LessonProgressResponse>(`${this.baseUrl}/Lessons/${lessonId}/complete`, { watchPercentage });
  }

  // ── Quizzes ──────────────────────────────────────────────────────────────

  getQuizForStudent(quizId: number): Observable<QuizStudentDetailResponse> {
    return this.http.get<QuizStudentDetailResponse>(`${this.baseUrl}/QuizAttempts/${quizId}/take`);
  }

  startQuizAttempt(quizId: number): Observable<StartAttemptResponse> {
    return this.http.post<StartAttemptResponse>(`${this.baseUrl}/QuizAttempts/${quizId}/start`, {});
  }

  submitQuiz(quizId: number, request: SubmitQuizRequest): Observable<QuizAttemptResponse> {
    return this.http.post<QuizAttemptResponse>(`${this.baseUrl}/QuizAttempts/${quizId}/submit`, request);
  }

  getRemainingAttempts(quizId: number): Observable<GetRemainingAttemptsResponse> {
    return this.http.get<GetRemainingAttemptsResponse>(`${this.baseUrl}/QuizAttempts/${quizId}/remaining-attempts`);
  }

  getPreviousAttempts(quizId: number): Observable<QuizAttemptResponse[]> {
    return this.http.get<QuizAttemptResponse[]>(`${this.baseUrl}/QuizAttempts/quiz/${quizId}`);
  }
}
