import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '@environments/environment';
import { PlatformFeeResponse, SetPlatformFeeRequest, FeeCategory } from '@models/platform-fee';

@Injectable({
  providedIn: 'root'
})
export class SettingsService {
  private http = inject(HttpClient);
  private readonly baseUrl = environment.apiUrl;

  getPlatformFee(category: FeeCategory = FeeCategory.CourseFee): Observable<PlatformFeeResponse> {
    return this.http.get<PlatformFeeResponse>(`${this.baseUrl}/platform-fees/current?category=${category}`);
  }

  setPlatformFee(request: SetPlatformFeeRequest): Observable<PlatformFeeResponse> {
    return this.http.post<PlatformFeeResponse>(`${this.baseUrl}/platform-fees`, request);
  }

  updatePlatformFee(request: SetPlatformFeeRequest): Observable<PlatformFeeResponse> {
    return this.http.put<PlatformFeeResponse>(`${this.baseUrl}/platform-fees`, request);
  }

  deletePlatformFee(category: FeeCategory): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/platform-fees/${category}`);
  }

  getFeeHistory(category?: FeeCategory): Observable<PlatformFeeResponse[]> {
    const url = category != null 
      ? `${this.baseUrl}/platform-fees/history?category=${category}`
      : `${this.baseUrl}/platform-fees/history`;
    return this.http.get<PlatformFeeResponse[]>(url);
  }
}
