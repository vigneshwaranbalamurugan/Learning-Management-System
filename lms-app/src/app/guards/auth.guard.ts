import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '@services/auth.service';

export const authGuard = (allowedRoles?: string[]): CanActivateFn => {
  return () => {
    const authService = inject(AuthService);
    const router = inject(Router);

    const role = authService.userRole() || localStorage.getItem('user_role');
    const email = localStorage.getItem('user_email');

    if (!role || !email) {
      router.navigate(['/login']);
      return false;
    }

    if (allowedRoles && allowedRoles.length > 0) {
      const hasRole = allowedRoles.some(r => r.toLowerCase() === role.toLowerCase());
      if (!hasRole) {
        authService.redirectToDashboard(role);
        return false;
      }
    }

    return true;
  };
};
