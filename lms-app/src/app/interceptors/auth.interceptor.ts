import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { ToastService } from '@services/toast.service';
import { catchError, throwError } from 'rxjs';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);
  const toastService = inject(ToastService);

  req = req.clone({
    withCredentials: true,
  });

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401) {
        // Show toaster notification
        toastService.showApiError(error, 'Unauthorized. Please login again.');
        
        // Redirect to login page
        router.navigate(['/login']);
      }
      return throwError(() => error);
    })
  );
};
