import { Routes } from '@angular/router';
import { authGuard } from '../guards/auth.guard';
import { DashboardLayout } from '@components/dashboard-layout/dashboard-layout';

export const adminRoutes: Routes = [
  {
    path: 'admin',
    component: DashboardLayout,
    canActivate: [authGuard(['Admin'])],
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      {
        path: 'dashboard',
        loadComponent: () => import('@pages/admin/revenue/revenue').then(m => m.AdminRevenue)
      },
      {
        path: 'logs',
        loadComponent: () => import('@pages/admin/logs/logs').then(m => m.AdminLogs)
      },
      {
        path: 'courses',
        loadComponent: () => import('@pages/admin/courses/courses').then(m => m.AdminCoursesComponent)
      },
      {
        path: 'settings',
        loadComponent: () => import('@pages/admin/settings/settings').then(m => m.AdminSettings)
      },
      {
        path: 'courses/new',
        loadComponent: () => import('@pages/instructor-course-form/instructor-course-form').then(m => m.InstructorCourseForm),
        canDeactivate: [(component: any) => component.canDeactivate ? component.canDeactivate() : true]
      },
      {
        path: 'courses/:slug',
        loadComponent: () => import('@pages/instructor-course-layout/instructor-course-layout').then(m => m.InstructorCourseLayout),
        children: [
          { path: '', redirectTo: 'overview', pathMatch: 'full' },
          {
            path: 'overview',
            loadComponent: () => import('@pages/instructor-course-overview/instructor-course-overview').then(m => m.InstructorCourseOverview),
            canDeactivate: [(component: any) => component.canDeactivate ? component.canDeactivate() : true]
          },
          {
            path: 'builder',
            loadComponent: () => import('@pages/instructor-course-builder/instructor-course-builder').then(m => m.InstructorCourseBuilder)
          },
          {
            path: 'quizzes',
            loadComponent: () => import('@pages/instructor-course-quizzes/instructor-course-quizzes').then(m => m.InstructorCourseQuizzes)
          },
          {
            path: 'assignments',
            loadComponent: () => import('@pages/instructor-course-assignments/instructor-course-assignments').then(m => m.InstructorCourseAssignments)
          },
          {
            path: 'assignments/new',
            loadComponent: () => import('@pages/instructor-assignment-form/instructor-assignment-form').then(m => m.InstructorAssignmentForm),
            canDeactivate: [(component: any) => component.canDeactivate ? component.canDeactivate() : true]
          },
          {
            path: 'assignments/:assignmentId/edit',
            loadComponent: () => import('@pages/instructor-assignment-form/instructor-assignment-form').then(m => m.InstructorAssignmentForm),
            canDeactivate: [(component: any) => component.canDeactivate ? component.canDeactivate() : true]
          },
          {
            path: 'sections/:sectionId/lessons/new',
            loadComponent: () => import('@pages/instructor-lesson-form/instructor-lesson-form').then(m => m.InstructorLessonForm),
            canDeactivate: [(component: any) => component.canDeactivate ? component.canDeactivate() : true]
          },
          {
            path: 'lessons/:lessonId/edit',
            loadComponent: () => import('@pages/instructor-lesson-form/instructor-lesson-form').then(m => m.InstructorLessonForm),
            canDeactivate: [(component: any) => component.canDeactivate ? component.canDeactivate() : true]
          },
          {
            path: 'lessons/:lessonId/detail',
            loadComponent: () => import('@pages/instructor-lesson-detail/instructor-lesson-detail').then(m => m.InstructorLessonDetail)
          },
          {
            path: 'lessons/:lessonId/resources/new',
            loadComponent: () => import('@pages/instructor-resource-form/instructor-resource-form').then(m => m.InstructorResourceForm),
            canDeactivate: [(component: any) => component.canDeactivate ? component.canDeactivate() : true]
          },
          {
            path: 'resources/:resourceId/edit',
            loadComponent: () => import('@pages/instructor-resource-form/instructor-resource-form').then(m => m.InstructorResourceForm),
            canDeactivate: [(component: any) => component.canDeactivate ? component.canDeactivate() : true]
          },
          {
            path: 'quizzes/:quizId/questions',
            loadComponent: () => import('@pages/instructor-quiz-questions/instructor-quiz-questions').then(m => m.InstructorQuizQuestions),
            canDeactivate: [(component: any) => component.canDeactivate ? component.canDeactivate() : true]
          },
          {
            path: 'analytics',
            loadComponent: () => import('@pages/instructor-course-analytics/instructor-course-analytics').then(m => m.InstructorCourseAnalytics)
          },
          {
            path: 'learners',
            loadComponent: () => import('@pages/instructor-course-learners/instructor-course-learners').then(m => m.InstructorCourseLearners)
          }
        ]
      },
      {
        path: 'courses/preview/:slug',
        loadComponent: () => import('@pages/course-detail/course-detail').then(m => m.CourseDetail)
      },
      {
        path: 'reviews',
        loadComponent: () => import('@pages/admin/reviews/reviews').then(m => m.AdminReviewsComponent)
      },
      {
        path: 'assignments',
        loadComponent: () => import('@pages/admin/assignments/assignments').then(m => m.AdminAssignmentsComponent)
      },
      {
        path: 'certificates',
        loadComponent: () => import('@pages/admin/certificates/certificates').then(m => m.AdminCertificatesComponent)
      },
      {
        path: 'revenue',
        redirectTo: 'dashboard',
        pathMatch: 'full'
      }
    ]
  }
];
