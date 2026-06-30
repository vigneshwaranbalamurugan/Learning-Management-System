import { Routes } from '@angular/router';
import { publicRoutes } from './routes/public.routes';
import { learnerRoutes } from './routes/learner.routes';
import { instructorRoutes } from './routes/instructor.routes';
import { adminRoutes } from './routes/admin.routes';

export const routes: Routes = [
  ...publicRoutes,
  ...learnerRoutes,
  ...instructorRoutes,
  ...adminRoutes,
  { path: '**', redirectTo: '' }
];
