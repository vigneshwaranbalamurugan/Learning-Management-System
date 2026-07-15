import { Routes } from '@angular/router';
import { Home } from '@pages/home/home';
import { Login } from '@pages/login/login';
import { VerifyCertificatePage } from '@pages/verify-certificate/verify-certificate';
import { NetworkErrorPage } from '@pages/network-error/network-error';
import { ServerTimeoutPage } from '@pages/server-timeout/server-timeout';
import { ResetPasswordPage } from '@pages/reset-password/reset-password';
import { VerifyEmailPage } from '@pages/login/verify-email/verify-email';
import { SharedCertificatePage } from '@pages/shared-certificate/shared-certificate';
import { guestGuard } from '../guards/guest.guard';

export const publicRoutes: Routes = [
  { path: '', component: Home, canActivate: [guestGuard] },
  { path: 'login', component: Login, canActivate: [guestGuard] },
  { path: 'auth/verify', component: VerifyEmailPage },
  { path: 'verify-certificate', component: VerifyCertificatePage },
  { path: 'verify-certificate/:id', component: VerifyCertificatePage },
  { path: 'network-error', component: NetworkErrorPage },
  { path: 'timeout', component: ServerTimeoutPage },
  { path: 'reset-password', component: ResetPasswordPage },
  { path: 'shared-certificate/:token', component: SharedCertificatePage }
];
