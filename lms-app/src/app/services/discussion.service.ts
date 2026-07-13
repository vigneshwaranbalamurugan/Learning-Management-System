import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '@environments/environment';
import {
  DiscussionResponse,
  DiscussionDetailResponse,
  ReplyResponse,
  CreateDiscussionRequest,
  UpdateDiscussionRequest,
  CreateReplyRequest,
  UpdateReplyRequest
} from '@models/discussion';

@Injectable({
  providedIn: 'root'
})
export class DiscussionService {
  private http = inject(HttpClient);
  private readonly baseUrl = environment.apiUrl;

  getLessonDiscussions(lessonId: number): Observable<DiscussionResponse[]> {
    return this.http.get<DiscussionResponse[]>(`${this.baseUrl}/Discussions/lesson/${lessonId}`);
  }

  getDiscussionDetail(id: number): Observable<DiscussionDetailResponse> {
    return this.http.get<DiscussionDetailResponse>(`${this.baseUrl}/Discussions/${id}`);
  }

  createDiscussion(request: CreateDiscussionRequest): Observable<DiscussionResponse> {
    return this.http.post<DiscussionResponse>(`${this.baseUrl}/Discussions`, request);
  }

  updateDiscussion(id: number, request: UpdateDiscussionRequest): Observable<DiscussionResponse> {
    return this.http.put<DiscussionResponse>(`${this.baseUrl}/Discussions/${id}`, request);
  }

  deleteDiscussion(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/Discussions/${id}`);
  }

  addReply(discussionId: number, request: CreateReplyRequest): Observable<ReplyResponse> {
    return this.http.post<ReplyResponse>(`${this.baseUrl}/Discussions/${discussionId}/replies`, request);
  }

  updateReply(replyId: number, request: UpdateReplyRequest): Observable<ReplyResponse> {
    return this.http.put<ReplyResponse>(`${this.baseUrl}/Discussions/replies/${replyId}`, request);
  }

  deleteReply(replyId: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/Discussions/replies/${replyId}`);
  }

  toggleLike(discussionId: number): Observable<number> {
    return this.http.post<number>(`${this.baseUrl}/Discussions/${discussionId}/like`, {});
  }
}
