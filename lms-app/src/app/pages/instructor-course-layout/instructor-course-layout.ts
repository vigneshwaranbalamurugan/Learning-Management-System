import { Component, OnInit, signal, inject, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, ActivatedRoute, Router } from '@angular/router';
import { DashboardService } from '@services/dashboard.service';
import { CourseDetailResponse } from '@models/dashboard';
import { untilDestroyed } from '../../rxjs/until-destroyed';
import { Loader } from '@components/loader/loader';
import { ToastService } from '@services/toast.service';

@Component({
  selector: 'app-instructor-course-layout',
  standalone: true,
  imports: [CommonModule, RouterModule, Loader],
  templateUrl: './instructor-course-layout.html'
})
export class InstructorCourseLayout implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private dashboardService = inject(DashboardService);
  private destroyRef = inject(DestroyRef);
  private toastService = inject(ToastService);

  public courseId = signal<number | null>(null);
  public slug = signal<string | null>(null);
  public course = signal<CourseDetailResponse | null>(null);
  protected isLoading = signal(true);
  protected activePath = signal<string>('');

  protected tabs = [
    { label: 'Overview', path: 'overview', icon: 'M12 6.253v13m0-13C10.832 5.477 9.246 5 7.5 5S4.168 5.477 3 6.253v13C4.168 18.477 5.754 18 7.5 18s3.332.477 4.5 1.253m0-13C13.168 5.477 14.754 5 16.5 5c1.747 0 3.332.477 4.5 1.253v13C19.832 18.477 18.247 18 16.5 18c-1.746 0-3.332.477-4.5 1.253' },
    { label: 'Content Builder', path: 'builder', icon: 'M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z' },
    { label: 'Quizzes', path: 'quizzes', icon: 'M9.663 17h4.673M12 3v1m6.364 1.636l-.707.707M21 12h-1M4 12H3m3.343-5.657l-.707-.707m2.828 9.9a5 5 0 117.072 0l-.548.547A3.374 3.374 0 0014 18.469V19a2 2 0 11-4 0v-.531c0-.895-.356-1.754-.988-2.386l-.548-.547z' },
    { label: 'Assignments', path: 'assignments', icon: 'M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2' },
    { label: 'Analytics', path: 'analytics', icon: 'M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z' },
    { label: 'Learners', path: 'learners', icon: 'M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197M13 7a3 3 0 11-6 0 3 3 0 016 0z' }
  ];

  ngOnInit() {
    // Set initial path
    this.activePath.set(this.router.url);

    // Track router URL updates
    this.router.events.pipe(untilDestroyed(this.destroyRef)).subscribe(() => {
      this.activePath.set(this.router.url);
    });

    this.route.paramMap.pipe(untilDestroyed(this.destroyRef)).subscribe(params => {
      const slugVal = params.get('slug');
      if (slugVal) {
        this.slug.set(slugVal);
        this.loadCourse(slugVal);
      }
    });
  }

  protected isTabActive(path: string): boolean {
    const url = this.activePath();
    if (path === 'overview') {
      return url.includes('/overview');
    }
    if (path === 'builder') {
      return url.includes('/builder') || url.includes('/lessons/') || url.includes('/sections/');
    }
    if (path === 'quizzes') {
      return url.includes('/quizzes');
    }
    if (path === 'assignments') {
      return url.includes('/assignments');
    }
    if (path === 'analytics') {
      return url.includes('/analytics');
    }
    if (path === 'learners') {
      return url.includes('/learners');
    }
    return false;
  }

  public loadCourse(slugOrId: string | number) {
    this.isLoading.set(true);
    const request$ = typeof slugOrId === 'number'
      ? this.dashboardService.getCourseById(slugOrId)
      : this.dashboardService.getInstructorCourseBySlug(slugOrId);

    request$.subscribe({
      next: (data) => {
        if (data.sections) {
          data.sections.sort((a: any, b: any) => (a.sortOrder || 0) - (b.sortOrder || 0));
          data.sections.forEach((section: any) => {
            if (section.lessons) {
              section.lessons.sort((a: any, b: any) => (a.sortOrder || 0) - (b.sortOrder || 0));
              section.lessons.forEach((lesson: any) => {
                if (lesson.lessonResources && !lesson.resources) {
                  lesson.resources = lesson.lessonResources;
                }
                if (lesson.resources) {
                  lesson.resources.sort((a: any, b: any) => (a.sortOrder || 0) - (b.sortOrder || 0));
                }
              });
            }
            if (section.quizzes) {
              section.quizzes.sort((a: any, b: any) => (a.order || 0) - (b.order || 0));
            }
            if (section.assignments) {
              section.assignments.sort((a: any, b: any) => (a.sortOrder || 0) - (b.sortOrder || 0));
            }
          });
        }
        this.course.set(data);
        this.courseId.set(data.id);
        this.slug.set(data.slug);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Failed to load course details:', err);
        this.toastService.showApiError(err, 'Failed to load course details.');
        this.isLoading.set(false);
        this.navigateBack();
      }
    });
  }

  protected navigateBack(): void {
    this.router.navigate(['/instructor/courses']);
  }
}
