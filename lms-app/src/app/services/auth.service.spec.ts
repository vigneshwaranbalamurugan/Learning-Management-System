import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { AuthService } from './auth.service';
import { ProfileService, UserProfile } from './profile.service';
import { environment } from '@environments/environment';
import { of, throwError, firstValueFrom } from 'rxjs';
import { vi } from 'vitest';

describe('AuthService', () => {
  let service: AuthService;
  let httpMock: HttpTestingController;
  let routerSpy: any;
  let profileServiceSpy: any;

  beforeEach(() => {
    const routerMock = { navigate: vi.fn() };
    const profileMock = { getProfile: vi.fn() };

    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: Router, useValue: routerMock },
        { provide: ProfileService, useValue: profileMock }
      ]
    });

    service = TestBed.inject(AuthService);
    httpMock = TestBed.inject(HttpTestingController);
    routerSpy = TestBed.inject(Router);
    profileServiceSpy = TestBed.inject(ProfileService);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('getRoleFromToken', () => {
    it('should return null for empty token', () => {
      expect(service.getRoleFromToken('')).toBeNull();
    });

    it('should return null for invalid token structure', () => {
      expect(service.getRoleFromToken('invalid-token')).toBeNull();
    });

    it('should extract role from standard claim type', () => {
      const payload = { 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role': 'Admin' };
      const token = `header.${btoa(JSON.stringify(payload))}.signature`;
      expect(service.getRoleFromToken(token)).toBe('Admin');
    });

    it('should extract role from simple role field', () => {
      const payload = { 'role': 'Instructor' };
      const token = `header.${btoa(JSON.stringify(payload))}.signature`;
      expect(service.getRoleFromToken(token)).toBe('Instructor');
    });
  });

  describe('clearSession', () => {
    it('should clear signals and mark sessionChecked', () => {
      service.currentUser.set({ id: 1 } as any);
      service.userRole.set('Admin');
      service.sessionChecked.set(false);

      service.clearSession();

      expect(service.currentUser()).toBeNull();
      expect(service.userRole()).toBeNull();
      expect(service.sessionChecked()).toBe(true);
    });
  });

  describe('redirectToDashboard', () => {
    it('should route to /login for null role', () => {
      service.redirectToDashboard('');
      expect(routerSpy.navigate).toHaveBeenCalledWith(['/login']);
    });

    it('should route to /instructor/dashboard for instructor role', () => {
      service.redirectToDashboard('Instructor');
      expect(routerSpy.navigate).toHaveBeenCalledWith(['/instructor/dashboard']);
    });

    it('should route to /admin/dashboard for admin role', () => {
      service.redirectToDashboard('Admin');
      expect(routerSpy.navigate).toHaveBeenCalledWith(['/admin/dashboard']);
    });

    it('should route to /learner/dashboard for any other role', () => {
      service.redirectToDashboard('Learner');
      expect(routerSpy.navigate).toHaveBeenCalledWith(['/learner/dashboard']);
    });
  });

  describe('logout', () => {
    it('should clear session and call revoke endpoint', () => {
      service.logout().subscribe();
      
      expect(service.currentUser()).toBeNull();
      
      const req = httpMock.expectOne(`${environment.apiUrl}/auth/revoke`);
      expect(req.request.method).toBe('POST');
      req.flush({});
    });
  });

  describe('initializeAuth', () => {
    it('should return null without HTTP call if session is checked and dead', () => {
      return new Promise<void>(resolve => {
        service.sessionChecked.set(true);
        service.currentUser.set(null);
        
        service.initializeAuth().subscribe(profile => {
          expect(profile).toBeNull();
          expect(profileServiceSpy.getProfile).not.toHaveBeenCalled();
          resolve();
        });
      });
    });

    it('should return profile and set signals on success', async () => {
      service.sessionChecked.set(false);
      const mockProfile = { firstName: 'Test', role: 'Learner' } as any;
      profileServiceSpy.getProfile.mockReturnValue(of(mockProfile));

      const profile = await firstValueFrom(service.initializeAuth());
      
      expect(profile).toEqual(mockProfile);
      expect(service.currentUser()).toEqual(mockProfile);
      expect(service.userRole()).toBe('Learner');
      expect(service.sessionChecked()).toBe(true);
    });

    it('should clear session on error', async () => {
      service.sessionChecked.set(false);
      profileServiceSpy.getProfile.mockReturnValue(throwError(() => new Error('Unauth')));

      const profile = await firstValueFrom(service.initializeAuth());
      
      expect(profile).toBeNull();
      expect(service.currentUser()).toBeNull();
      expect(service.sessionChecked()).toBe(true);
    });
  });
});
