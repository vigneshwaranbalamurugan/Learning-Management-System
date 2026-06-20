import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '@environments/environment';
import { LoginModel } from '@models/auth';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private http = inject(HttpClient);
  private readonly baseUrl = environment.apiUrl;

  loginApiCall(credentials: LoginModel): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/auth/login`, {
      email: credentials.email,
      password: credentials.password
    });
  }


  getRoleFromToken(token: string): string | null {
    if (!token) return null;
    try {
      const parts = token.split('.');
      if (parts.length !== 3) return null;
      const payload = JSON.parse(atob(parts[1]));
      
      const roleClaimType = 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role';
      alert(payload[roleClaimType] || payload['role']);
      return payload[roleClaimType] || payload['role'] || null;
    } catch (e) {
      console.error('Error decoding JWT token:', e);
      return null;
    }
  }
}
