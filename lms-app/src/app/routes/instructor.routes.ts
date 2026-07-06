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
        path: 'courses',
        loadComponent: () => import('@pages/instructor-courses/instructor-courses').then(m => m.InstructorCourses)
      },
      {
        path: 'courses/new',
        loadComponent: () => import('@pages/instructor-course-form/instructor-course-form').then(m => m.InstructorCourseForm),
        canDeactivate: [(component: any) => component.canDeactivate ? component.canDeactivate() : true]
      },
      {
        path: 'courses/preview/:slug',
        loadComponent: () => import('@pages/course-detail/course-detail').then(m => m.CourseDetail)
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
        path: 'assignments',
        loadComponent: () => import('@pages/instructor-assignments/instructor-assignments').then(m => m.InstructorAssignments)
      },
      {
        path: 'assignments/:assignmentId/evaluate',
        loadComponent: () => import('@pages/instructor-evaluate/instructor-evaluate').then(m => m.InstructorEvaluate),
        canDeactivate: [(component: any) => component.canDeactivate ? component.canDeactivate() : true]
      },
      {
        path: 'assignments/:assignmentId/graded',
        loadComponent: () => import('@pages/instructor-graded-submissions/instructor-graded-submissions').then(m => m.InstructorGradedSubmissions)
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
        loadComponent: () => import('@pages/profile/profile').then(m => m.Profile),
        canDeactivate: [(component: any) => component.canDeactivate ? component.canDeactivate() : true]
      },
      {
        path: 'revenue',
        loadComponent: () => import('@pages/instructor-revenue/instructor-revenue').then(m => m.InstructorRevenue)
      },
      {
        path: 'settings',
        loadComponent: () => import('@pages/instructor-settings/instructor-settings').then(m => m.InstructorSettings)
      },
      {
        path: 'reviews',
        loadComponent: () => import('@pages/instructor-reviews/instructor-reviews').then(m => m.InstructorReviewsPage)
      }
    ]
  }
];
