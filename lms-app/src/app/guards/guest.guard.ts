import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '@services/auth.service';

export const guestGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const role = authService.userRole() || localStorage.getItem('user_role');
  const email = localStorage.getItem('user_email');

  if (role && email) {
    authService.redirectToDashboard(role);
    return false;
  }

  return true;
};
