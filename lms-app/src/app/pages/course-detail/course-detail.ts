import { Component, OnInit, signal, inject, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { DashboardService } from '@services/dashboard.service';
import { ToastService } from '@services/toast.service';
import {
  CourseDetailResponse,
  EnrollmentResponse
} from '@models/dashboard';
import { untilDestroyed } from '../../rxjs/until-destroyed';
import { forkJoin } from 'rxjs';

@Component({
  selector: 'app-course-detail',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './course-detail.html'
})
export class CourseDetail implements OnInit {
  private dashboardService = inject(DashboardService);
  private toastService     = inject(ToastService);
  private route            = inject(ActivatedRoute);
  private router           = inject(Router);
  private destroyRef       = inject(DestroyRef);

  // ── Data ─────────────────────────────────────────────────────────────────
  protected course            = signal<CourseDetailResponse | null>(null);
  protected enrollment        = signal<EnrollmentResponse | null>(null);
  protected isEnrolled        = signal(false);
  protected enrollmentProgress = signal(0);

  // ── UI State ─────────────────────────────────────────────────────────────
  protected isLoading   = signal(true);
  protected isEnrolling = signal(false);
  protected expandedSections = signal<Set<number>>(new Set());

  // ── Stored course ID from router state ────────────────────────────────────
  private courseId: number | null = null;

  ngOnInit(): void {
    // Router state can be read via history.state (Angular router sets it there)
    // getCurrentNavigation() is only valid during the navigation itself, not in ngOnInit
    const historyState = history.state;
    if (historyState?.['courseId']) {
      this.courseId = Number(historyState['courseId']);
    }

    this.route.paramMap.pipe(untilDestroyed(this.destroyRef)).subscribe(params => {
      const slug = params.get('slug');
      if (!slug) {
        this.goBack();
        return;
      }

      if (this.courseId) {
        this.loadCourse(this.courseId);
      } else {
        // State lost (e.g. direct URL navigation / refresh) — go back with message
        this.toastService.showError('Course not found. Please navigate from the course catalog.');
        this.goBack();
      }
    });
  }

  private loadCourse(courseId: number): void {
    this.isLoading.set(true);

    forkJoin({
      course: this.dashboardService.getCourseById(courseId),
      enrollments: this.dashboardService.getMyEnrollments()
    }).pipe(untilDestroyed(this.destroyRef)).subscribe({
      next: ({ course, enrollments }) => {
        this.course.set(course);

        // Expand first section by default
        if (course.sections?.length > 0) {
          this.expandedSections.update(s => {
            const next = new Set(s);
            next.add(course.sections[0].id);
            return next;
          });
        }

        // Find enrollment for this course
        const found = enrollments.find(e => e.courseId === courseId);
        if (found) {
          this.isEnrolled.set(true);
          this.enrollmentProgress.set(found.progressPercentage ?? 0);
          this.enrollment.set(found);
        }

        this.isLoading.set(false);
      },
      error: (err) => {
        this.toastService.showApiError(err, 'Failed to load course details.');
        this.isLoading.set(false);
        this.goBack();
      }
    });
  }

  protected enroll(): void {
    const c = this.course();
    if (!c) return;
    if (this.isEnrolled()) {
      this.toastService.showInfo('You are already enrolled in this course.');
      return;
    }
    if (c.isPremium) {
      this.toastService.showWarning('Premium courses require payment to enroll.');
      return;
    }
    this.isEnrolling.set(true);
    this.dashboardService.enrollFreeCourse(c.id).pipe(untilDestroyed(this.destroyRef)).subscribe({
      next: () => {
        this.isEnrolled.set(true);
        this.isEnrolling.set(false);
        this.toastService.showSuccess(`Successfully enrolled in "${c.title}"!`);
      },
      error: (err) => {
        this.toastService.showApiError(err, 'Enrollment failed.');
        this.isEnrolling.set(false);
      }
    });
  }

  protected toggleSection(sectionId: number): void {
    this.expandedSections.update(s => {
      const next = new Set(s);
      if (next.has(sectionId)) {
        next.delete(sectionId);
      } else {
        next.add(sectionId);
      }
      return next;
    });
  }

  protected isSectionExpanded(sectionId: number): boolean {
    return this.expandedSections().has(sectionId);
  }

  protected expandAll(): void {
    const c = this.course();
    if (!c) return;
    this.expandedSections.set(new Set(c.sections.map(s => s.id)));
  }

  protected collapseAll(): void {
    this.expandedSections.set(new Set());
  }

  protected goBack(): void {
    this.router.navigate(['/learner/explore']);
  }

  protected navigateToCourses(): void {
    this.router.navigate(['/learner/courses']);
  }

  // ── Helpers ───────────────────────────────────────────────────────────────

  protected getLevelName(level: number | string): string {
    switch (Number(level)) {
      case 1: return 'Beginner';
      case 2: return 'Intermediate';
      case 3: return 'Advanced';
      default: return 'All Levels';
    }
  }

  protected getLevelColor(level: number | string): string {
    switch (Number(level)) {
      case 1: return 'emerald';
      case 2: return 'amber';
      case 3: return 'red';
      default: return 'gray';
    }
  }

  protected formatDuration(isoStr: string | null | undefined): string {
    if (!isoStr) return '';
    const parts = isoStr.split(':');
    const h = parseInt(parts[0] ?? '0', 10);
    const m = parseInt(parts[1] ?? '0', 10);
    if (h > 0 && m > 0) return `${h}h ${m}m`;
    if (h > 0) return `${h}h`;
    if (m > 0) return `${m}m`;
    const s = parseInt(parts[2] ?? '0', 10);
    return s > 0 ? `${s}s` : '';
  }

  protected getLessonTypeIcon(type: number | string): string {
    switch (String(type).toLowerCase()) {
      case '0': case 'video':        return '🎬';
      case '1': case 'article':      return '📄';
      case '2': case 'pdf':          return '📑';
      case '3': case 'externallink': return '🔗';
      case '4': case 'quiz':         return '📝';
      default:                       return '📖';
    }
  }

  protected getLessonTypeName(type: number | string): string {
    switch (String(type).toLowerCase()) {
      case '0': case 'video':        return 'Video';
      case '1': case 'article':      return 'Article';
      case '2': case 'pdf':          return 'PDF';
      case '3': case 'externallink': return 'Link';
      case '4': case 'quiz':         return 'Quiz';
      default:                       return 'Lesson';
    }
  }

  protected getTotalLessonsCount(): number {
    return this.course()?.sections?.reduce((acc, s) => acc + (s.lessons?.length ?? 0), 0) ?? 0;
  }

  protected parseOutcomes(raw: string | null | undefined): string[] {
    if (!raw) return [];
    return raw.split(/\n|;/).map(l => l.trim()).filter(l => l.length > 0);
  }

  protected parseRequirements(raw: string | null | undefined): string[] {
    if (!raw) return [];
    return raw.split(/\n|;/).map(l => l.trim()).filter(l => l.length > 0);
  }

  protected starsArray(rating: number): boolean[] {
    return [1, 2, 3, 4, 5].map(s => s <= Math.round(rating));
  }
}
