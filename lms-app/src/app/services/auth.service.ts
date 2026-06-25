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
  // True once we know for certain whether the user is logged in or not.
  // Prevents guards from re-triggering initializeAuth() after a failed session check.
  sessionChecked = signal(false);
  private initAuth$?: Observable<UserProfile | null>;

  constructor() {
  }

  loginApiCall(credentials: LoginModel, rememberMe: boolean = false): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/auth/login`, {
      email: credentials.email,
      password: credentials.password,
      rememberMe: rememberMe
    });
  }

  initializeAuth(): Observable<UserProfile | null> {
    if (this.initAuth$) return this.initAuth$;

    // If we have already confirmed the session is dead, don't hit the backend again.
    if (this.sessionChecked() && !this.currentUser()) {
      return of(null);
    }

    this.isAuthenticating.set(true);

    // Try to load the profile using the existing access_token cookie.
    // If the access token is expired (401), the auth interceptor will automatically:
    //   1. Call POST /auth/refresh-token (using the refresh_token cookie)
    //   2. Receive a new access_token cookie from the server
    //   3. Retry this /profile request
    // If the refresh token is also expired, the interceptor clears the session + redirects to /login.
    this.initAuth$ = this.profileService.getProfile().pipe(
      tap((profile) => {
        const actualRole = profile.role || 'Learner';
        const updatedProfile = { ...profile, role: actualRole };
        this.currentUser.set(updatedProfile);
        this.userRole.set(actualRole);

        // Auto redirect to appropriate dashboard if on login or root landing page
        const currentPath = window.location.pathname;
        if (currentPath === '/' || currentPath.startsWith('/login')) {
          this.redirectToDashboard(actualRole);
        }
      }),
      catchError(() => {
        this.clearSession();
        return of(null);
      }),
      finalize(() => {
        this.isAuthenticating.set(false);
        this.sessionChecked.set(true); // Mark session as definitively checked
        this.initAuth$ = undefined;
      }),
      shareReplay(1)
    );
    return this.initAuth$;
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
    this.currentUser.set(null);
    this.userRole.set(null);
    this.sessionChecked.set(true); // Session is confirmed dead — stop all retry attempts
  }

  logout() {
    // Clear the session FIRST so guards know the session is dead immediately.
    // This prevents any redirect loop when the revoke call triggers the interceptor.
    this.clearSession();
    return this.http.post(`${this.baseUrl}/auth/revoke`, {}, { withCredentials: true });
  }

  refreshToken(): Observable<any> {
    return this.http.post(`${this.baseUrl}/auth/refresh-token`, {}, { withCredentials: true });
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
