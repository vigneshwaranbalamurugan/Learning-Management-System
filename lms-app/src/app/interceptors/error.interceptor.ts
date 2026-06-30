import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError, TimeoutError } from 'rxjs';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);

  return next(req).pipe(
    catchError((error: any) => {
      // Offline or network unreachable
      if (error instanceof HttpErrorResponse && error.status === 0) {
        router.navigate(['/network-error'], { skipLocationChange: true });
        return throwError(() => error);
      }
      
      // Gateway Timeout from backend [RequestTimeout]
      if (error instanceof HttpErrorResponse && error.status === 504) {
        router.navigate(['/timeout'], { skipLocationChange: true });
        return throwError(() => error);
      }

      // RxJS TimeoutError
      if (error instanceof TimeoutError) {
        router.navigate(['/timeout'], { skipLocationChange: true });
        return throwError(() => error);
      }

      // Pass through all other errors (401, 403, 400, etc.)
      return throwError(() => error);
    })
  );
};
