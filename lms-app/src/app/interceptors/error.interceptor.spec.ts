import { TestBed } from '@angular/core/testing';
import { HttpClient, provideHttpClient, withInterceptors, HttpErrorResponse } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { Router } from '@angular/router';
import { errorInterceptor } from './error.interceptor';
import { vi } from 'vitest';

describe('ErrorInterceptor', () => {
  let httpMock: HttpTestingController;
  let httpClient: HttpClient;
  let routerSpy: any;

  beforeEach(() => {
    routerSpy = { navigate: vi.fn() };

    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([errorInterceptor])),
        provideHttpClientTesting(),
        { provide: Router, useValue: routerSpy }
      ]
    });

    httpMock = TestBed.inject(HttpTestingController);
    httpClient = TestBed.inject(HttpClient);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should redirect to network error on status 0', () => {
    return new Promise<void>(resolve => {
      httpClient.get('/api/test').subscribe({
        error: () => {
          expect(routerSpy.navigate).toHaveBeenCalledWith(['/network-error'], { skipLocationChange: true });
          resolve();
        }
      });
      
      const req = httpMock.expectOne('/api/test');
      req.flush('Error', { status: 0, statusText: 'Unknown Error' });
    });
  });

  it('should redirect to timeout on status 504', () => {
    return new Promise<void>(resolve => {
      httpClient.get('/api/test').subscribe({
        error: () => {
          expect(routerSpy.navigate).toHaveBeenCalledWith(['/timeout'], { skipLocationChange: true });
          resolve();
        }
      });
      
      const req = httpMock.expectOne('/api/test');
      req.flush('Gateway Timeout', { status: 504, statusText: 'Gateway Timeout' });
    });
  });

  it('should pass through other errors (e.g. 400)', () => {
    return new Promise<void>(resolve => {
      httpClient.get('/api/test').subscribe({
        error: (err: HttpErrorResponse) => {
          expect(err.status).toBe(400);
          expect(routerSpy.navigate).not.toHaveBeenCalled();
          resolve();
        }
      });
      
      const req = httpMock.expectOne('/api/test');
      req.flush('Bad Request', { status: 400, statusText: 'Bad Request' });
    });
  });
});
