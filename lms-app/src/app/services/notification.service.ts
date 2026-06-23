import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '@environments/environment';
import { Notification } from '@models/notification';

const PAGE_SIZE = 20;

@Injectable({
  providedIn: 'root',
})
export class NotificationService {
  private http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/notifications`;

  /**
   * Fetches a page of notifications for infinite-scroll.
   * The backend currently returns all records; we slice client-side for
   * smooth infinite scroll while keeping the REST contract unchanged.
   */
  getNotifications(page = 1, pageSize = 10): Observable<Notification[]> {
    const params = new HttpParams().set('page', page).set('pageSize', pageSize);
    return this.http.get<Notification[]>(this.baseUrl, { params });
  }

  getUnreadCount(): Observable<number> {
    return this.http.get<number>(`${this.baseUrl}/unread-count`);
  }

  markAsRead(id: number): Observable<void> {
    return this.http.patch<void>(`${this.baseUrl}/${id}/read`, {});
  }

  markAllAsRead(): Observable<void> {
    return this.http.patch<void>(`${this.baseUrl}/read-all`, {});
  }

  deleteNotification(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

  /** Client-side paging helper removed as we use server-side pagination now. */
}
