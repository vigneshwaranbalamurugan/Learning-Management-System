import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '@services/auth.service';
import { map, take } from 'rxjs/operators';
import { of } from 'rxjs';

export const authGuard = (allowedRoles?: string[]): CanActivateFn => {
  return () => {
    const authService = inject(AuthService);
    const router = inject(Router);

    if (authService.currentUser()) {
      const role = authService.userRole()!;
      if (allowedRoles && allowedRoles.length > 0) {
        const hasRole = allowedRoles.some(r => r.toLowerCase() === role.toLowerCase());
        if (!hasRole) {
          authService.redirectToDashboard(role);
          return false;
        }
      }
      return true;
    }

    // Session already confirmed dead — no need to hit the backend
    if (authService.sessionChecked()) {
      router.navigate(['/login']);
      return false;
    }

    return authService.initializeAuth().pipe(
      take(1),
      map(profile => {
        if (!profile) {
          router.navigate(['/login']);
          return false;
        }
        const role = profile.role || 'Learner';
        if (allowedRoles && allowedRoles.length > 0) {
          const hasRole = allowedRoles.some(r => r.toLowerCase() === role.toLowerCase());
          if (!hasRole) {
            authService.redirectToDashboard(role);
            return false;
          }
        }
        return true;
      })
    );
  };
};
