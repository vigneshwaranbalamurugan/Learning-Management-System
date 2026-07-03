import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '@environments/environment';
import {
  AdminRevenueSummary,
  PagedAdminTransactionResponse,
  PagedAdminPayoutResponse,
  RevenueFilters
} from '@models/admin-revenue';

@Injectable({ providedIn: 'root' })
export class AdminRevenueService {
  private http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/revenue`;

  getSummary(): Observable<AdminRevenueSummary> {
    return this.http.get<AdminRevenueSummary>(`${this.baseUrl}/admin`);
  }

  getTransactions(filters: Partial<RevenueFilters>): Observable<PagedAdminTransactionResponse> {
    let params = new HttpParams()
      .set('page', String(filters.page ?? 1))
      .set('pageSize', String(filters.pageSize ?? 15));

    if (filters.search) params = params.set('search', filters.search);
    if (filters.status) params = params.set('status', filters.status);
    if (filters.dateFrom) params = params.set('dateFrom', filters.dateFrom);
    if (filters.dateTo) params = params.set('dateTo', filters.dateTo);

    return this.http.get<PagedAdminTransactionResponse>(`${this.baseUrl}/admin/transactions`, { params });
  }

  getPayouts(filters: Partial<RevenueFilters>): Observable<PagedAdminPayoutResponse> {
    let params = new HttpParams()
      .set('page', String(filters.page ?? 1))
      .set('pageSize', String(filters.pageSize ?? 15));

    if (filters.search) params = params.set('search', filters.search);
    if (filters.status) params = params.set('status', filters.status);
    if (filters.dateFrom) params = params.set('dateFrom', filters.dateFrom);
    if (filters.dateTo) params = params.set('dateTo', filters.dateTo);

    return this.http.get<PagedAdminPayoutResponse>(`${this.baseUrl}/admin/payouts`, { params });
  }
}
