import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '@environments/environment';
import { CertificateResponse } from '@models/certificate';

@Injectable({
  providedIn: 'root',
})
export class CertificateService {
  private http = inject(HttpClient);
  private readonly baseUrl = environment.apiUrl;

  getMyCertificates(): Observable<CertificateResponse[]> {
    return this.http.get<CertificateResponse[]>(`${this.baseUrl}/certificates/my`);
  }

  verifyCertificate(certificateId: string): Observable<CertificateResponse> {
    return this.http.get<CertificateResponse>(`${this.baseUrl}/certificates/verify/${certificateId}`);
  }
}
