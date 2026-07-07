import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '@environments/environment';
import { CertificateResponse, PagedCertificateResponse, CertificateRegenerationStatus, RegenerateCertificatesResponse } from '@models/certificate';

@Injectable({
  providedIn: 'root',
})
export class CertificateService {
  private http = inject(HttpClient);
  private readonly baseUrl = environment.apiUrl;

  getMyCertificates(page: number = 1, pageSize: number = 10): Observable<PagedCertificateResponse> {
    return this.http.get<PagedCertificateResponse>(`${this.baseUrl}/certificates/my?page=${page}&pageSize=${pageSize}`);
  }

  verifyCertificate(certificateId: string): Observable<CertificateResponse> {
    return this.http.get<CertificateResponse>(`${this.baseUrl}/certificates/verify/${certificateId}`);
  }

  getCertificateRegenerationStatus(): Observable<CertificateRegenerationStatus> {
    return this.http.get<CertificateRegenerationStatus>(`${this.baseUrl}/certificates/regeneration-status`);
  }

  regenerateAllCertificates(): Observable<RegenerateCertificatesResponse> {
    return this.http.post<RegenerateCertificatesResponse>(`${this.baseUrl}/certificates/regenerate-all`, {});
  }
}
