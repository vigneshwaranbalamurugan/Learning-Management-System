import { Component, OnInit, signal, computed, inject, DestroyRef, effect, untracked } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ToastService } from '@services/toast.service';
import { EnrollmentResponse } from '@models/enrollment';
import { CourseLevel } from '../../enums/course-level.enum';
import { untilDestroyed } from '../../rxjs/until-destroyed';
import { Router, RouterModule } from '@angular/router';

import { Loader } from '@components/loader/loader';
import { EnrollmentService } from '@services/enrollment.service';

@Component({
  selector: 'app-my-courses',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, Loader],
  templateUrl: './my-courses.html'
})
export class MyCourses implements OnInit {
  private enrollmentService = inject(EnrollmentService);
  private toastService = inject(ToastService);
  private router = inject(Router);
  private destroyRef = inject(DestroyRef);

  protected enrollments = signal<EnrollmentResponse[]>([]);
  protected isLoading = signal(true);
  protected searchQuery = signal('');
  protected statusFilter = signal<string>('all'); // 'all', 'in-progress', 'completed'
  protected typeFilter = signal<string>('all');     // 'all', 'self-paced', 'batch'

  // Pagination State
  protected currentPage = signal(1);
  protected pageSize = signal(6);
  protected totalPages = signal(1);
  protected totalCount = signal(0);
  protected completedCount = signal(0);
  protected inProgressCount = signal(0);
  protected filteredEnrollments = computed(() => this.enrollments());

  constructor() {
    let isInitializing = true;
    effect(() => {
      this.searchQuery();
      this.statusFilter();
      this.typeFilter();

      untracked(() => {
        if (!isInitializing) {
          this.currentPage.set(1);
        }
      });
    }, { allowSignalWrites: true });

    effect(() => {
      this.searchQuery();
      this.statusFilter();
      this.typeFilter();
      this.currentPage();

      untracked(() => {
        this.loadMyEnrollments();
      });
    });
    isInitializing = false;
  }

  ngOnInit(): void {
    // We need the overall counts for the stats cards.
    // It is best to call getAllMyEnrollments to compute stats, or just use paginated response totalCount
    this.loadStats();
  }

  private loadStats(): void {
    this.enrollmentService.getAllMyEnrollments()
      .pipe(untilDestroyed(this.destroyRef))
      .subscribe({
        next: (allData) => {
          this.totalCount.set(allData.length);
          this.completedCount.set(allData.filter(e => e.isCompleted).length);
          this.inProgressCount.set(allData.filter(e => !e.isCompleted).length);
        }
      });
  }

  private loadMyEnrollments(): void {
    this.isLoading.set(true);
    const search = this.searchQuery().trim() || undefined;
    const status = this.statusFilter() === 'all' ? undefined : this.statusFilter();
    let accessType: string | undefined = undefined;
    if (this.typeFilter() === 'self-paced') accessType = 'SelfPaced';
    if (this.typeFilter() === 'batch') accessType = 'CohortBased';

    this.enrollmentService.getMyEnrollments(this.currentPage(), this.pageSize(), search, status, accessType)
      .pipe(untilDestroyed(this.destroyRef))
      .subscribe({
        next: (data) => {
          this.enrollments.set(data?.enrollments ?? []);
          this.totalPages.set(data?.totalPages || 1);
          // If search/filter applied, we don't update global counts, they remain from loadStats
          this.isLoading.set(false);
        },
        error: (err) => {
          this.toastService.showApiError(err, 'Failed to load your enrolled courses.');
          this.isLoading.set(false);
        }
      });
  }

  protected onSearch(): void {
    this.currentPage.set(1);
    this.loadMyEnrollments();
  }

  protected onFilterChange(): void {
    this.currentPage.set(1);
    this.loadMyEnrollments();
  }

  protected prevPage(): void {
    if (this.currentPage() > 1) {
      this.currentPage.update(p => p - 1);
      this.loadMyEnrollments();
    }
  }

  protected nextPage(): void {
    if (this.currentPage() < this.totalPages()) {
      this.currentPage.update(p => p + 1);
      this.loadMyEnrollments();
    }
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

  protected resetFilters(): void {
    this.searchQuery.set('');
    this.statusFilter.set('all');
    this.typeFilter.set('all');
  }

  protected navigateToExplore(): void {
    this.router.navigate(['/learner/explore']);
  }
}
