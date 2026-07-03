import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '@environments/environment';
import { InstructorRevenueSummaryResponse, PagedInstructorRevenueSummaryResponse } from '@models/revenue';

@Injectable({
  providedIn: 'root'
})
export class RevenueService {
  private http = inject(HttpClient);
  private readonly baseUrl = environment.apiUrl;

  getInstructorRevenue(page: number = 1, pageSize: number = 10, search?: string, status?: string): Observable<PagedInstructorRevenueSummaryResponse> {
    let url = `${this.baseUrl}/revenue/instructor?page=${page}&pageSize=${pageSize}`;
    if (search) url += `&search=${encodeURIComponent(search)}`;
    if (status) url += `&status=${encodeURIComponent(status)}`;
    return this.http.get<PagedInstructorRevenueSummaryResponse>(url);
  }
}
