import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '@environments/environment';
import { EnrollmentResponse } from '@models/enrollment';

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

  getMyEnrollments(): Observable<EnrollmentResponse[]> {
    return this.http.get<EnrollmentResponse[]>(`${this.baseUrl}/enrollments/my`);
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
