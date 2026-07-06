import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { AuthService } from '@services/auth.service';
import { guestGuard } from './guest.guard';
import { of } from 'rxjs';
import { vi } from 'vitest';

describe('GuestGuard', () => {
  let authServiceSpy: any;
  let routerSpy: any;

  beforeEach(() => {
    routerSpy = { navigate: vi.fn() };
    authServiceSpy = { 
      currentUser: vi.fn(), 
      userRole: vi.fn(), 
      sessionChecked: vi.fn(), 
      redirectToDashboard: vi.fn(), 
      initializeAuth: vi.fn() 
    };
    
    TestBed.configureTestingModule({
      providers: [
        { provide: Router, useValue: routerSpy },
        { provide: AuthService, useValue: authServiceSpy }
      ]
    });
  });

  const runGuard = () => {
    return TestBed.runInInjectionContext(() => {
      return guestGuard(null as any, null as any);
    });
  };

  it('should redirect to dashboard if user is logged in', () => {
    authServiceSpy.currentUser.mockReturnValue({ id: 1 });
    authServiceSpy.userRole.mockReturnValue('Learner');
    
    expect(runGuard()).toBe(false);
    expect(authServiceSpy.redirectToDashboard).toHaveBeenCalledWith('Learner');
  });

  it('should allow access if session confirmed dead', () => {
    authServiceSpy.currentUser.mockReturnValue(null);
    authServiceSpy.sessionChecked.mockReturnValue(true);
    
    expect(runGuard()).toBe(true);
  });

  it('should wait for initializeAuth and redirect if profile returned', () => {
    return new Promise<void>(resolve => {
      authServiceSpy.currentUser.mockReturnValue(null);
      authServiceSpy.sessionChecked.mockReturnValue(false);
      
      authServiceSpy.initializeAuth.mockReturnValue(of({ role: 'Instructor' }));
      
      const obs$ = runGuard() as any;
      obs$.subscribe((result: boolean) => {
        expect(result).toBe(false);
        expect(authServiceSpy.redirectToDashboard).toHaveBeenCalledWith('Instructor');
        resolve();
      });
    });
  });

  it('should allow access if initializeAuth returns null', () => {
    return new Promise<void>(resolve => {
      authServiceSpy.currentUser.mockReturnValue(null);
      authServiceSpy.sessionChecked.mockReturnValue(false);
      
      authServiceSpy.initializeAuth.mockReturnValue(of(null));
      
      const obs$ = runGuard() as any;
      obs$.subscribe((result: boolean) => {
        expect(result).toBe(true);
        resolve();
      });
    });
  });
});
