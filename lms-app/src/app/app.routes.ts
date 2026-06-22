import { Routes } from '@angular/router';
import { publicRoutes } from './routes/public.routes';
import { learnerRoutes } from './routes/learner.routes';
import { instructorRoutes } from './routes/instructor.routes';

export const routes: Routes = [
  ...publicRoutes,
  ...learnerRoutes,
  ...instructorRoutes,
  { path: '**', redirectTo: '' }
];
