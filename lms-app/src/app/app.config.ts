import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { DATE_PIPE_DEFAULT_TIMEZONE } from '@angular/common';
import { authInterceptor } from '@interceptors/auth.interceptor';
import { errorInterceptor } from '@interceptors/error.interceptor';

import { routes } from './app.routes';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    provideHttpClient(withInterceptors([errorInterceptor, authInterceptor])),
    { provide: DATE_PIPE_DEFAULT_TIMEZONE, useValue: 'Asia/Kolkata' }
  ]
};
