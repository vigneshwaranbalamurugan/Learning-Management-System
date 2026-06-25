import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '@environments/environment';
import { QuizAttemptResponse, QuizAttemptDetailResponse } from '@models/quiz';

@Injectable({
  providedIn: 'root',
})
export class QuizService {
  private http = inject(HttpClient);
  private readonly baseUrl = environment.apiUrl;

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

  reorderQuizzes(quizOrders: { quizId: number; sortOrder: number }[]): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/Quizzes/reorder`, { quizOrders });
  }

  getMyQuizAttempts(): Observable<QuizAttemptResponse[]> {
    return this.http.get<QuizAttemptResponse[]>(`${this.baseUrl}/QuizAttempts/my`);
  }

  getQuizAttemptDetail(attemptId: number): Observable<QuizAttemptDetailResponse> {
    return this.http.get<QuizAttemptDetailResponse>(`${this.baseUrl}/QuizAttempts/${attemptId}`);
  }
}
