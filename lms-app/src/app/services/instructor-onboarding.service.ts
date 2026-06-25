import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '@environments/environment';
import {
  OnboardingStatusResponse,
  LinkedAccountResponse,
  StakeholderResponse,
  PayoutProductResponse,
  CreateLinkedAccountRequest,
  UpdateLinkedAccountRequest,
  CreateStakeholderRequest,
  UpdateStakeholderRequest,
  ConfigureBankRequest
} from '@models/onboarding';

@Injectable({
  providedIn: 'root'
})
export class InstructorOnboardingService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/revenue/onboarding`;

  getStatus(): Observable<OnboardingStatusResponse> {
    return this.http.get<OnboardingStatusResponse>(`${this.apiUrl}/status`);
  }

  createAccount(request: CreateLinkedAccountRequest): Observable<LinkedAccountResponse> {
    return this.http.post<LinkedAccountResponse>(`${this.apiUrl}/account`, request);
  }

  updateAccount(request: UpdateLinkedAccountRequest): Observable<LinkedAccountResponse> {
    return this.http.put<LinkedAccountResponse>(`${this.apiUrl}/account`, request);
  }

  createStakeholder(request: CreateStakeholderRequest): Observable<StakeholderResponse> {
    return this.http.post<StakeholderResponse>(`${this.apiUrl}/stakeholder`, request);
  }

  updateStakeholder(request: UpdateStakeholderRequest): Observable<StakeholderResponse> {
    return this.http.put<StakeholderResponse>(`${this.apiUrl}/stakeholder`, request);
  }

  requestProduct(): Observable<PayoutProductResponse> {
    return this.http.post<PayoutProductResponse>(`${this.apiUrl}/product`, {});
  }

  configureBank(request: ConfigureBankRequest): Observable<PayoutProductResponse> {
    return this.http.patch<PayoutProductResponse>(`${this.apiUrl}/product/bank`, request);
  }
}
