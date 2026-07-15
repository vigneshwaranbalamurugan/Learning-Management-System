import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '@environments/environment';

// ── Models ──────────────────────────────────────────────────────────────────

export interface AiChatMessage {
  role: 'user' | 'assistant';
  content: string;
}

export interface AiTutorChatRequest {
  question: string;
  history?: AiChatMessage[];
}

export interface AiTutorChatResponse {
  answer: string;
  lessonId: number;
}

export interface AiSummaryResponse {
  lessonId: number;
  summary: string;
  keyPoints: string[];
  notes: string;
  status: 'generated' | 'generating' | 'not_supported' | 'error';
  generatedAt?: string;
}

// ── Service ─────────────────────────────────────────────────────────────────

@Injectable({
  providedIn: 'root'
})
export class AiService {
  private http = inject(HttpClient);
  private readonly baseUrl = environment.apiUrl;

  /**
   * Ask the AI tutor a question about a lesson.
   * Returns the AI answer based on lesson content via RAG.
   */
  chatWithTutor(
    lessonId: number,
    question: string,
    history: AiChatMessage[] = []
  ): Observable<AiTutorChatResponse> {
    const body: AiTutorChatRequest = { question, history };
    return this.http.post<AiTutorChatResponse>(
      `${this.baseUrl}/Lessons/${lessonId}/ai/chat`,
      body
    );
  }

  /**
   * Get the AI-generated summary for a lesson.
   * May return status="generating" while the background job is running.
   */
  getLessonSummary(lessonId: number): Observable<AiSummaryResponse> {
    return this.http.get<AiSummaryResponse>(
      `${this.baseUrl}/Lessons/${lessonId}/ai/summary`
    );
  }

  /**
   * Trigger summary regeneration (Instructor/Admin only).
   */
  regenerateSummary(lessonId: number): Observable<any> {
    return this.http.post<any>(
      `${this.baseUrl}/Lessons/${lessonId}/ai/summary/regenerate`,
      {}
    );
  }
}
