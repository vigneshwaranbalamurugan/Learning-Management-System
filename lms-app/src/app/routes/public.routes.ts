import { Routes } from '@angular/router';
import { Home } from '@pages/home/home';
import { Login } from '@pages/login/login';
import { VerifyCertificatePage } from '@pages/verify-certificate/verify-certificate';
import { guestGuard } from '../guards/guest.guard';

export const publicRoutes: Routes = [
  { path: '', component: Home, canActivate: [guestGuard] },
  { path: 'login', component: Login, canActivate: [guestGuard] },
  { path: 'verify-certificate', component: VerifyCertificatePage },
  { path: 'verify-certificate/:id', component: VerifyCertificatePage }
];
