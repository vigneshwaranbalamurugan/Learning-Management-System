import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '@environments/environment';
import { EnrollmentResponse, PagedEnrollmentResponse } from '@models/enrollment';

@Injectable({
  providedIn: 'root',
})
export class EnrollmentService {
  private http = inject(HttpClient);
  private readonly baseUrl = environment.apiUrl;

  private getIdempotencyHeaders(courseId: number): { headers: HttpHeaders } {
    const storageKey = `idem_key_${courseId}`;
    let key = sessionStorage.getItem(storageKey);
    if (!key) {
      key = crypto.randomUUID();
      sessionStorage.setItem(storageKey, key);
    }
    return { headers: new HttpHeaders({ 'Idempotency-Key': key }) };
  }

  getMyEnrollments(
    page: number = 1,
    pageSize: number = 10,
    search?: string,
    status?: string,
    accessType?: string
  ): Observable<PagedEnrollmentResponse> {
    let url = `${this.baseUrl}/enrollments/my?page=${page}&pageSize=${pageSize}`;
    if (search) url += `&search=${encodeURIComponent(search)}`;
    if (status) url += `&status=${encodeURIComponent(status)}`;
    if (accessType) url += `&accessType=${encodeURIComponent(accessType)}`;
    return this.http.get<PagedEnrollmentResponse>(url);
  }

  getAllMyEnrollments(): Observable<EnrollmentResponse[]> {
    return this.http.get<PagedEnrollmentResponse>(`${this.baseUrl}/enrollments/my?page=1&pageSize=1000`)
      .pipe(map(res => res?.enrollments || []));
  }

  enrollFreeCourse(courseId: number): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/courses/${courseId}/enroll/free`, {}, this.getIdempotencyHeaders(courseId));
  }

  enrollPremiumCourse(courseId: number, providerName: string = 'Razorpay'): Observable<{ providerOrderId: string, providerName: string }> {
    return this.http.post<{ providerOrderId: string, providerName: string }>(`${this.baseUrl}/courses/${courseId}/enroll/premium`, { providerName }, this.getIdempotencyHeaders(courseId));
  }

  verifyPayment(courseId: number, paymentData: any): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/courses/${courseId}/enroll/verify`, paymentData, this.getIdempotencyHeaders(courseId));
  }
}
