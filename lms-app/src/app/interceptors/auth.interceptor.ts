import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { ToastService } from '@services/toast.service';
import { AuthService } from '@services/auth.service';
import { catchError, switchMap, filter, take, throwError, BehaviorSubject } from 'rxjs';

let isRefreshing = false;
let refreshTokenSubject = new BehaviorSubject<any>(null);

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);
  const toastService = inject(ToastService);
  const authService = inject(AuthService);

  req = req.clone({
    withCredentials: true,
  });

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      const isAuthEndpoint =
        req.url.includes('/auth/refresh-token') ||
        req.url.includes('/auth/login') ||
        req.url.includes('/auth/revoke');

      if (error.status === 401 && !isAuthEndpoint) {
        if (!isRefreshing) {
          isRefreshing = true;
          refreshTokenSubject.next(null);

          return authService.refreshToken().pipe(
            switchMap(() => {
              isRefreshing = false;
              refreshTokenSubject.next(true);
              return next(req);
            }),
            catchError((refreshErr) => {
              isRefreshing = false;
              // Clear session state and call revoke on the backend
              authService.logout().subscribe({ error: () => {} });
              toastService.showApiError(error, 'Session expired. Please login again.');
              // Only redirect if the user was on a protected route (has a currentUser)
              // At this point currentUser is null (just cleared), so we check the route.
              const currentPath = window.location.pathname;
              const isPublicPath =
                currentPath === '/' ||
                currentPath.startsWith('/login') ||
                currentPath.startsWith('/verify-certificate');
              if (!isPublicPath) {
                router.navigate(['/login']);
              }
              return throwError(() => refreshErr);
            })
          );
        } else {
          return refreshTokenSubject.pipe(
            filter((result) => result !== null),
            take(1),
            switchMap(() => next(req))
          );
        }
      }
      return throwError(() => error);
    })
  );
};
