import { Component, OnInit, signal, inject, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { ToastService } from '@services/toast.service';
import { EnrollmentResponse } from '@models/enrollment';
import { CourseLevel } from '../../enums/course-level.enum';
import { untilDestroyed } from '../../rxjs/until-destroyed';
import { Loader } from '@components/loader/loader';
import { PaginationComponent } from '@components/pagination/pagination.component';
import { EnrollmentService } from '@services/enrollment.service';

@Component({
  selector: 'app-progress-page',
  standalone: true,
  imports: [CommonModule, RouterModule, Loader, PaginationComponent],
  templateUrl: './progress.html'
})
export class ProgressPage implements OnInit {
  private enrollmentService = inject(EnrollmentService);
  private toastService = inject(ToastService);
  private router = inject(Router);
  private destroyRef = inject(DestroyRef);

  protected enrollments = signal<EnrollmentResponse[]>([]);
  protected isLoading = signal(true);
  
  // Pagination State
  protected currentPage = signal(1);
  protected pageSize = signal(10);
  protected totalItems = signal(0);
  protected totalPages = signal(0);

  ngOnInit(): void {
    this.loadEnrollments();
  }

  protected loadEnrollments(page: number = 1): void {
    this.isLoading.set(true);
    this.enrollmentService.getMyEnrollments(page, this.pageSize())
      .pipe(untilDestroyed(this.destroyRef))
      .subscribe({
        next: (data) => {
          this.enrollments.set(data?.enrollments ?? []);
          this.currentPage.set(data?.pageNumber ?? 1);
          this.pageSize.set(data?.pageSize ?? 10);
          this.totalItems.set(data?.totalCount ?? 0);
          this.totalPages.set(data?.totalPages ?? 0);
          this.isLoading.set(false);
        },
        error: (err) => {
          this.toastService.showApiError(err, 'Failed to load progress list.');
          this.isLoading.set(false);
        }
      });
  }

  protected onPageChange(page: number): void {
    this.loadEnrollments(page);
  }

  protected viewDetailedProgress(courseId: number): void {
    this.router.navigate(['/learner/progress', courseId]);
  }

  protected getLevelName(level: number | string): string {
    const lvl = String(level).trim().toLowerCase();
    if (lvl === String(CourseLevel.Beginner) || lvl === 'beginner') return 'Beginner';
    if (lvl === String(CourseLevel.Intermediate) || lvl === 'intermediate') return 'Intermediate';
    if (lvl === String(CourseLevel.Advanced) || lvl === 'advanced') return 'Advanced';
    return 'All Levels';
  }

  protected getLevelColor(level: number | string): string {
    const lvl = String(level).trim().toLowerCase();
    if (lvl === String(CourseLevel.Beginner) || lvl === 'beginner') return 'bg-emerald-50 text-emerald-700';
    if (lvl === String(CourseLevel.Intermediate) || lvl === 'intermediate') return 'bg-amber-50 text-amber-700';
    if (lvl === String(CourseLevel.Advanced) || lvl === 'advanced') return 'bg-red-50 text-red-700';
    return 'bg-gray-100 text-gray-600';
  }

  protected isSelfPaced(accessType: number | string): boolean {
    const at = String(accessType).trim().toLowerCase();
    return at === '1' || at === 'selfpaced';
  }

  protected formatDuration(isoStr: string): string {
    if (!isoStr) return '';
    const parts = isoStr.split(':');
    const h = parseInt(parts[0] ?? '0', 10);
    const m = parseInt(parts[1] ?? '0', 10);
    if (h > 0 && m > 0) return `${h}h ${m}m`;
    if (h > 0) return `${h}h`;
    return `${m}m`;
  }
}
