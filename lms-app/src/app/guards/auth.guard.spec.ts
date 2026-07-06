import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { AuthService } from '@services/auth.service';
import { authGuard } from './auth.guard';
import { of } from 'rxjs';
import { vi } from 'vitest';

describe('AuthGuard', () => {
  let routerSpy: any;
  let authServiceSpy: any;

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

  const runGuard = (allowedRoles?: string[]) => {
    return TestBed.runInInjectionContext(() => {
      return authGuard(allowedRoles)({} as any, {} as any);
    });
  };

  it('should allow access if user is logged in and no roles specified', () => {
    authServiceSpy.currentUser.mockReturnValue({ id: 1 });
    authServiceSpy.userRole.mockReturnValue('Learner');
    
    expect(runGuard()).toBe(true);
  });

  it('should allow access if user role matches allowed roles', () => {
    authServiceSpy.currentUser.mockReturnValue({ id: 1 });
    authServiceSpy.userRole.mockReturnValue('Admin');
    
    expect(runGuard(['Admin'])).toBe(true);
  });

  it('should redirect if user role does not match allowed roles', () => {
    authServiceSpy.currentUser.mockReturnValue({ id: 1 });
    authServiceSpy.userRole.mockReturnValue('Learner');
    
    expect(runGuard(['Admin'])).toBe(false);
    expect(authServiceSpy.redirectToDashboard).toHaveBeenCalledWith('Learner');
  });

  it('should redirect to login if session confirmed dead', () => {
    authServiceSpy.currentUser.mockReturnValue(null);
    authServiceSpy.sessionChecked.mockReturnValue(true);
    
    expect(runGuard()).toBe(false);
    expect(routerSpy.navigate).toHaveBeenCalledWith(['/login']);
  });

  it('should wait for initializeAuth if session not checked', () => {
    return new Promise<void>(resolve => {
      authServiceSpy.currentUser.mockReturnValue(null);
      authServiceSpy.sessionChecked.mockReturnValue(false);
      
      authServiceSpy.initializeAuth.mockReturnValue(of({ role: 'Instructor' }));
      
      const obs$ = runGuard(['Instructor']) as any;
      obs$.subscribe((result: boolean) => {
        expect(result).toBe(true);
        resolve();
      });
    });
  });

  it('should redirect to login if initializeAuth returns null', () => {
    return new Promise<void>(resolve => {
      authServiceSpy.currentUser.mockReturnValue(null);
      authServiceSpy.sessionChecked.mockReturnValue(false);
      
      authServiceSpy.initializeAuth.mockReturnValue(of(null));
      
      const obs$ = runGuard() as any;
      obs$.subscribe((result: boolean) => {
        expect(result).toBe(false);
        expect(routerSpy.navigate).toHaveBeenCalledWith(['/login']);
        resolve();
      });
    });
  });
});
