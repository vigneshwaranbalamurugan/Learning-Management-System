import { Routes } from '@angular/router';
import { authGuard } from '../guards/auth.guard';
import { DashboardLayout } from '@components/dashboard-layout/dashboard-layout';

export const learnerRoutes: Routes = [
  {
    path: 'learner',
    component: DashboardLayout,
    canActivate: [authGuard(['Learner'])],
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      {
        path: 'dashboard',
        loadComponent: () => import('@pages/learner-dashboard/learner-dashboard').then(m => m.LearnerDashboard)
      },
      {
        path: 'explore',
        loadComponent: () => import('@pages/explore-courses/explore-courses').then(m => m.ExploreCourses)
      },
      {
        path: 'explore/:slug',
        loadComponent: () => import('@pages/course-detail/course-detail').then(m => m.CourseDetail)
      },
      {
        path: 'courses',
        loadComponent: () => import('@pages/my-courses/my-courses').then(m => m.MyCourses)
      },
      {
        path: 'certificates',
        loadComponent: () => import('@pages/certificates/certificates').then(m => m.CertificatesPage)
      },
      {
        path: 'certificates/:id',
        loadComponent: () => import('@pages/certificate-detail/certificate-detail').then(m => m.CertificateDetailPage)
      },
      {
        path: 'quizzes',
        loadComponent: () => import('@pages/quizzes/quizzes').then(m => m.QuizzesPage)
      },
      {
        path: 'quizzes/:id',
        loadComponent: () => import('@pages/quizzes-detail/quizzes-detail').then(m => m.QuizDetailPage)
      },
      {
        path: 'profile',
        loadComponent: () => import('@pages/profile/profile').then(m => m.Profile),
        canDeactivate: [(component: any) => component.canDeactivate ? component.canDeactivate() : true]
      },
      {
        path: 'reviews',
        loadComponent: () => import('@pages/reviews/reviews').then(m => m.ReviewsPage)
      },
      {
        path: 'progress',
        loadComponent: () => import('@pages/progress/progress').then(m => m.ProgressPage)
      },
      {
        path: 'progress/:id',
        loadComponent: () => import('@pages/progress/progress-detail').then(m => m.ProgressDetailPage)
      },
      {
        path: 'assignments',
        loadComponent: () => import('@pages/assignments/assignments').then(m => m.AssignmentsPage)
      },
      {
        path: 'assignments/:id',
        loadComponent: () => import('@pages/assignments/assignment-detail').then(m => m.AssignmentDetailPage)
      }
    ]
  },
  {
    path: 'learner/learn/:courseId',
    loadComponent: () => import('@pages/course-learning/course-learning').then(m => m.CourseLearning),
    canActivate: [authGuard(['Learner'])]
  },
  {
    path: 'learner/learn/:courseId/lesson/:lessonId',
    loadComponent: () => import('@pages/course-learning/course-learning').then(m => m.CourseLearning),
    canActivate: [authGuard(['Learner'])]
  },
  {
    path: 'learner/learn/:courseId/quiz/:quizId',
    loadComponent: () => import('@pages/course-learning/course-learning').then(m => m.CourseLearning),
    canActivate: [authGuard(['Learner'])]
  },
  {
    path: 'learner/learn/:courseId/assignment/:assignmentId',
    loadComponent: () => import('@pages/course-learning/course-learning').then(m => m.CourseLearning),
    canActivate: [authGuard(['Learner'])]
  }
];
