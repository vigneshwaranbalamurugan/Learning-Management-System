import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '@services/auth.service';
import { map, take } from 'rxjs/operators';
import { of } from 'rxjs';

export const guestGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);
  
  const email = localStorage.getItem('user_email') || sessionStorage.getItem('user_email');
  if (!email) {
    return true;
  }

  if (authService.userRole()) {
    authService.redirectToDashboard(authService.userRole()!);
    return false;
  }

  return authService.initializeAuth().pipe(
    take(1),
    map(profile => {
      if (profile) {
        authService.redirectToDashboard(profile.role || 'Learner');
        return false;
      }
      return true;
    })
  );
};
