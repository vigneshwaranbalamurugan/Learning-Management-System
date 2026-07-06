import { TestBed } from '@angular/core/testing';
import { HttpClient, provideHttpClient, withInterceptors, HttpErrorResponse } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { Router } from '@angular/router';
import { authInterceptor } from './auth.interceptor';
import { AuthService } from '@services/auth.service';
import { ToastService } from '@services/toast.service';
import { of, throwError } from 'rxjs';
import { vi } from 'vitest';

describe('AuthInterceptor', () => {
  let httpMock: HttpTestingController;
  let httpClient: HttpClient;
  let routerSpy: any;
  let authServiceSpy: any;
  let toastServiceSpy: any;

  beforeEach(() => {
    routerSpy = { navigate: vi.fn() };
    authServiceSpy = { refreshToken: vi.fn(), logout: vi.fn() };
    toastServiceSpy = { showApiError: vi.fn() };

    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([authInterceptor])),
        provideHttpClientTesting(),
        { provide: Router, useValue: routerSpy },
        { provide: AuthService, useValue: authServiceSpy },
        { provide: ToastService, useValue: toastServiceSpy }
      ]
    });

    httpMock = TestBed.inject(HttpTestingController);
    httpClient = TestBed.inject(HttpClient);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should add withCredentials: true to requests', () => {
    httpClient.get('/api/test').subscribe();
    
    const req = httpMock.expectOne('/api/test');
    expect(req.request.withCredentials).toBe(true);
    req.flush({});
  });

  it('should not refresh token on 401 for auth endpoints', () => {
    httpClient.get('/auth/login').subscribe({ error: () => {} });
    
    const req = httpMock.expectOne('/auth/login');
    req.flush('Unauthorized', { status: 401, statusText: 'Unauthorized' });
    
    expect(authServiceSpy.refreshToken).not.toHaveBeenCalled();
  });

  it('should refresh token on 401 for non-auth endpoints and retry', () => {
    return new Promise<void>(resolve => {
      authServiceSpy.refreshToken.mockReturnValue(of({}));
      
      httpClient.get('/api/test').subscribe(() => {
        expect(authServiceSpy.refreshToken).toHaveBeenCalled();
        resolve();
      });
      
      const req1 = httpMock.expectOne('/api/test');
      req1.flush('Unauthorized', { status: 401, statusText: 'Unauthorized' });
      
      const req2 = httpMock.expectOne('/api/test');
      req2.flush({});
    });
  });

  it('should logout and redirect on refresh token failure', () => {
    return new Promise<void>(resolve => {
      authServiceSpy.refreshToken.mockReturnValue(throwError(() => new Error('Refresh failed')));
      authServiceSpy.logout.mockReturnValue(of({}));
      
      Object.defineProperty(window, 'location', {
        value: { pathname: '/dashboard' },
        writable: true
      });
      
      httpClient.get('/api/test').subscribe({
        error: () => {
          expect(authServiceSpy.logout).toHaveBeenCalled();
          expect(routerSpy.navigate).toHaveBeenCalledWith(['/login']);
          expect(toastServiceSpy.showApiError).toHaveBeenCalled();
          resolve();
        }
      });
      
      const req1 = httpMock.expectOne('/api/test');
      req1.flush('Unauthorized', { status: 401, statusText: 'Unauthorized' });
    });
  });
});
