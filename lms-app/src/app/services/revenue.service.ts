import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '@environments/environment';
import { InstructorRevenueSummaryResponse } from '@models/revenue';

@Injectable({
  providedIn: 'root'
})
export class RevenueService {
  private http = inject(HttpClient);
  private readonly baseUrl = environment.apiUrl;

  getInstructorRevenue(): Observable<InstructorRevenueSummaryResponse> {
    return this.http.get<InstructorRevenueSummaryResponse>(`${this.baseUrl}/revenue/instructor`);
  }
}
