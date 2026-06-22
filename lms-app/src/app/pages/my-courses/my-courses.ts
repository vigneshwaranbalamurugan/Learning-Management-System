import { Component, OnInit, signal, computed, inject, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DashboardService } from '@services/dashboard.service';
import { ToastService } from '@services/toast.service';
import { EnrollmentResponse } from '@models/dashboard';
import { untilDestroyed } from '../../rxjs/until-destroyed';
import { Router, RouterModule } from '@angular/router';

import { Loader } from '@components/loader/loader';

@Component({
  selector: 'app-my-courses',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, Loader],
  templateUrl: './my-courses.html'
})
export class MyCourses implements OnInit {
  private dashboardService = inject(DashboardService);
  private toastService = inject(ToastService);
  private router = inject(Router);
  private destroyRef = inject(DestroyRef);

  protected enrollments = signal<EnrollmentResponse[]>([]);
  protected isLoading = signal(true);
  protected searchQuery = signal('');
  protected statusFilter = signal<string>('all'); // 'all', 'in-progress', 'completed'
  protected typeFilter = signal<string>('all');     // 'all', 'self-paced', 'batch'

  // Dynamic statistics computed from all enrollments
  protected totalCount = computed(() => this.enrollments().length);
  protected completedCount = computed(() => this.enrollments().filter(e => e.isCompleted).length);
  protected inProgressCount = computed(() => this.enrollments().filter(e => !e.isCompleted).length);

  // Client-side filtering logic
  protected filteredEnrollments = computed(() => {
    let list = this.enrollments();
    const query = this.searchQuery().toLowerCase().trim();
    const status = this.statusFilter();
    const type = this.typeFilter();

    if (query) {
      list = list.filter(e => 
        e.courseTitle.toLowerCase().includes(query) || 
        (e.instructorName && e.instructorName.toLowerCase().includes(query))
      );
    }

    if (status === 'in-progress') {
      list = list.filter(e => !e.isCompleted);
    } else if (status === 'completed') {
      list = list.filter(e => e.isCompleted);
    }

    if (type === 'self-paced') {
      list = list.filter(e => this.isSelfPaced(e.courseAccessType));
    } else if (type === 'batch') {
      list = list.filter(e => !this.isSelfPaced(e.courseAccessType));
    }

    return list;
  });

  ngOnInit(): void {
    this.loadMyEnrollments();
  }

  private loadMyEnrollments(): void {
    this.isLoading.set(true);
    this.dashboardService.getMyEnrollments()
      .pipe(untilDestroyed(this.destroyRef))
      .subscribe({
        next: (data) => {
          this.enrollments.set(data ?? []);
          this.isLoading.set(false);
        },
        error: (err) => {
          this.toastService.showApiError(err, 'Failed to load your enrolled courses.');
          this.isLoading.set(false);
        }
      });
  }

  protected getLevelName(level: number | string): string {
    const lvl = String(level).trim().toLowerCase();
    if (lvl === '1' || lvl === 'beginner') return 'Beginner';
    if (lvl === '2' || lvl === 'intermediate') return 'Intermediate';
    if (lvl === '3' || lvl === 'advanced') return 'Advanced';
    return 'All Levels';
  }

  protected getLevelColor(level: number | string): string {
    const lvl = String(level).trim().toLowerCase();
    if (lvl === '1' || lvl === 'beginner') return 'bg-emerald-50 text-emerald-700';
    if (lvl === '2' || lvl === 'intermediate') return 'bg-amber-50 text-amber-700';
    if (lvl === '3' || lvl === 'advanced') return 'bg-red-50 text-red-700';
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
