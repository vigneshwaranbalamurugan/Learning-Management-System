import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '@environments/environment';
import { ReviewResponse, CreateReviewRequest, UpdateReviewRequest, PagedInstructorReviewResponse } from '@models/review';
import { HttpParams } from '@angular/common/http';

@Injectable({
  providedIn: 'root',
})
export class ReviewService {
  private http = inject(HttpClient);
  private readonly baseUrl = environment.apiUrl;

  getCourseReviews(courseId: number): Observable<ReviewResponse[]> {
    return this.http.get<ReviewResponse[]>(`${this.baseUrl}/Reviews/course/${courseId}`);
  }

  submitReview(request: CreateReviewRequest): Observable<ReviewResponse> {
    return this.http.post<ReviewResponse>(`${this.baseUrl}/Reviews`, request);
  }

  updateReview(reviewId: number, request: UpdateReviewRequest): Observable<ReviewResponse> {
    return this.http.put<ReviewResponse>(`${this.baseUrl}/Reviews/${reviewId}`, request);
  }

  deleteReview(reviewId: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/Reviews/${reviewId}`);
  }

  getInstructorReviews(page: number, pageSize: number, rating?: number, courseId?: number, search?: string): Observable<PagedInstructorReviewResponse> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());
      
    if (rating) params = params.set('rating', rating.toString());
    if (courseId) params = params.set('courseId', courseId.toString());
    if (search) params = params.set('search', search);

    return this.http.get<PagedInstructorReviewResponse>(`${this.baseUrl}/Reviews/instructor`, { params });
  }
}
