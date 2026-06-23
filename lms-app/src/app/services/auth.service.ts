import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { catchError, tap, finalize, shareReplay } from 'rxjs/operators';
import { Router } from '@angular/router';
import { environment } from '@environments/environment';
import { LoginModel } from '@models/auth';
import { ProfileService, UserProfile } from './profile.service';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private http = inject(HttpClient);
  private profileService = inject(ProfileService);
  private router = inject(Router);
  private readonly baseUrl = environment.apiUrl;

  currentUser = signal<UserProfile | null>(null);
  userRole = signal<string | null>(null);
  isAuthenticating = signal(false);
  private initAuth$?: Observable<UserProfile | null>;

  constructor() {
    const cachedProfile = localStorage.getItem('user_profile') || sessionStorage.getItem('user_profile');
    if (cachedProfile) {
      try {
        const profile = JSON.parse(cachedProfile) as UserProfile;
        this.currentUser.set(profile);
        this.userRole.set(profile.role || 'Learner');
      } catch (e) {
        console.error('Error parsing cached profile:', e);
      }
    }
  }

  loginApiCall(credentials: LoginModel): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/auth/login`, {
      email: credentials.email,
      password: credentials.password
    });
  }

  initializeAuth(): Observable<UserProfile | null> {
    if (this.initAuth$) return this.initAuth$;

    const email = localStorage.getItem('user_email') || sessionStorage.getItem('user_email');
    
    if (email) {
      this.isAuthenticating.set(true);
      this.initAuth$ = this.profileService.getProfile().pipe(
        tap((profile) => {
          const actualRole = profile.role || 'Learner';
          const updatedProfile = { ...profile, email, role: actualRole };
          this.currentUser.set(updatedProfile);
          this.userRole.set(actualRole);
          const storage = localStorage.getItem('user_email') ? localStorage : sessionStorage;
          storage.setItem('user_profile', JSON.stringify(updatedProfile));
          
          // Auto redirect to appropriate dashboard if on login or root landing page
          const currentPath = window.location.pathname;
          if (currentPath === '/' || currentPath.startsWith('/login')) {
            this.redirectToDashboard(actualRole);
          }
        }),
        catchError((err) => {
          this.clearSession();
          return of(null);
        }),
        finalize(() => {
          this.isAuthenticating.set(false);
          this.initAuth$ = undefined;
        }),
        shareReplay(1)
      );
      return this.initAuth$;
    }
    return of(null);
  }

  redirectToDashboard(role: string) {
    if (!role) {
      this.router.navigate(['/login']);
    } else if (role.toLowerCase() === 'instructor') {
      this.router.navigate(['/instructor/dashboard']);
    } else {
      this.router.navigate(['/learner/dashboard']);
    }
  }

  clearSession() {
    localStorage.removeItem('user_email');
    localStorage.removeItem('user_profile');
    sessionStorage.removeItem('user_email');
    sessionStorage.removeItem('user_profile');
    this.currentUser.set(null);
    this.userRole.set(null);
  }

  logout() {
    this.clearSession();
    // Hit backend revoke/logout endpoint to clear HttpOnly cookies
    return this.http.post(`${this.baseUrl}/auth/revoke`, {});
  }

  getRoleFromToken(token: string): string | null {
    if (!token) return null;
    try {
      const parts = token.split('.');
      if (parts.length !== 3) return null;
      const payload = JSON.parse(atob(parts[1]));
      
      const roleClaimType = 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role';
      return payload[roleClaimType] || payload['role'] || null;
    } catch (e) {
      console.error('Error decoding JWT token:', e);
      return null;
    }
  }
}
