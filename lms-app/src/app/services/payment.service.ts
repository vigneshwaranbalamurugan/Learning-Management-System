import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { environment } from '@env/environment';
import { Observable } from 'rxjs';

export interface LearnerPayment {
  id: number;
  courseTitle: string;
  courseThumbnailUrl?: string;
  amount: number;
  currency: string;
  status: string;
  paidAt?: string;
  createdAt: string;
  providerPaymentId?: string;
  invoiceNumber: string;
}

export interface LearnerPaymentPagedResponse {
  items: LearnerPayment[];
  totalCount: number;
  totalPages: number;
  currentPage: number;
}

@Injectable({
  providedIn: 'root'
})
export class PaymentService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/payments`;

  getMyPayments(search: string = '', status: string = '', page: number = 1, pageSize: number = 10): Observable<LearnerPaymentPagedResponse> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    if (search) params = params.set('search', search);
    if (status) params = params.set('status', status);

    return this.http.get<LearnerPaymentPagedResponse>(`${this.apiUrl}/my`, { params });
  }

  downloadInvoice(id: number): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/my/${id}/invoice`, {
      responseType: 'blob'
    });
  }
}
