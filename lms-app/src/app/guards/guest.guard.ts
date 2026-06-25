import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '@services/auth.service';
import { map, take } from 'rxjs/operators';
import { of } from 'rxjs';

export const guestGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);
  
  if (authService.currentUser()) {
    authService.redirectToDashboard(authService.userRole()!);
    return false;
  }

  // Session already confirmed dead — user is not logged in, allow access to login page
  if (authService.sessionChecked()) {
    return true;
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
