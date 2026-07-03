import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '@environments/environment';
import { LearnerAnalytics, InstructorAnalytics, AdminAnalytics } from '@models/analytics';

@Injectable({
  providedIn: 'root',
})
export class AnalyticsService {
  private http = inject(HttpClient);
  private readonly baseUrl = environment.apiUrl;

  getLearnerAnalytics(): Observable<LearnerAnalytics> {
    return this.http.get<LearnerAnalytics>(`${this.baseUrl}/analytics/learner`);
  }

  getInstructorAnalytics(): Observable<InstructorAnalytics> {
    return this.http.get<InstructorAnalytics>(`${this.baseUrl}/analytics/instructor`);
  }

  getAdminAnalytics(): Observable<AdminAnalytics> {
    return this.http.get<AdminAnalytics>(`${this.baseUrl}/analytics/admin`);
  }
}
