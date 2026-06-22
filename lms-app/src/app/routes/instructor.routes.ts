import { Routes } from '@angular/router';
import { authGuard } from '../guards/auth.guard';
import { DashboardLayout } from '@components/dashboard-layout/dashboard-layout';

export const instructorRoutes: Routes = [
  {
    path: 'instructor',
    component: DashboardLayout,
    canActivate: [authGuard(['Instructor'])],
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      {
        path: 'dashboard',
        loadComponent: () => import('@pages/instructor-dashboard/instructor-dashboard').then(m => m.InstructorDashboard)
      },
      {
        path: 'assignments',
        loadComponent: () => import('@pages/instructor-assignments/instructor-assignments').then(m => m.InstructorAssignments)
      },
      {
        path: 'assignments/:assignmentId/evaluate',
        loadComponent: () => import('@pages/instructor-evaluate/instructor-evaluate').then(m => m.InstructorEvaluate)
      },
      {
        path: 'progress',
        loadComponent: () => import('@pages/progress/instructor-progress-list').then(m => m.InstructorProgressList)
      },
      {
        path: 'progress/course/:courseId',
        loadComponent: () => import('@pages/progress/instructor-course-progress').then(m => m.InstructorCourseProgress)
      },
      {
        path: 'profile',
        loadComponent: () => import('@pages/profile/profile').then(m => m.Profile)
      }
    ]
  }
];
