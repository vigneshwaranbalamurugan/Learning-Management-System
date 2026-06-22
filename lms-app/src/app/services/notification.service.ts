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
  getNotifications(page = 1): Observable<Notification[]> {
    return this.http.get<Notification[]>(this.baseUrl);
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

  /** Client-side paging helper — returns a slice of the full list. */
  static pageSlice(all: Notification[], page: number): { items: Notification[]; hasMore: boolean } {
    const end = page * PAGE_SIZE;
    return {
      items: all.slice(0, end),
      hasMore: all.length > end,
    };
  }
}
