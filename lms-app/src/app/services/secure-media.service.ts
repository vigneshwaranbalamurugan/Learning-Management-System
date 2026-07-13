import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '@environments/environment';

export interface SecureUrlResponse {
  url: string;
}

@Injectable({
  providedIn: 'root'
})
export class SecureMediaService {
  private http = inject(HttpClient);
  private readonly baseUrl = environment.apiUrl;

  getSecureUrl(blobPath: string, courseId: number): Observable<SecureUrlResponse> {
    const params = new HttpParams()
      .set('blobPath', blobPath)
      .set('courseId', courseId.toString());

    return this.http.get<SecureUrlResponse>(`${this.baseUrl}/media/secure-url`, { params });
  }
}
