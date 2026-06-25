import { Component, OnInit, signal, inject, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { ToastService } from '@services/toast.service';
import { EnrollmentResponse } from '@models/enrollment';
import { untilDestroyed } from '../../rxjs/until-destroyed';
import { Loader } from '@components/loader/loader';
import { EnrollmentService } from '@services/enrollment.service';

@Component({
  selector: 'app-progress-page',
  standalone: true,
  imports: [CommonModule, RouterModule, Loader],
  templateUrl: './progress.html'
})
export class ProgressPage implements OnInit {
  private enrollmentService = inject(EnrollmentService);
  private toastService = inject(ToastService);
  private router = inject(Router);
  private destroyRef = inject(DestroyRef);

  protected enrollments = signal<EnrollmentResponse[]>([]);
  protected isLoading = signal(true);

  ngOnInit(): void {
    this.loadEnrollments();
  }

  private loadEnrollments(): void {
    this.isLoading.set(true);
    this.enrollmentService.getMyEnrollments()
      .pipe(untilDestroyed(this.destroyRef))
      .subscribe({
        next: (data) => {
          this.enrollments.set(data ?? []);
          this.isLoading.set(false);
        },
        error: (err) => {
          this.toastService.showApiError(err, 'Failed to load progress list.');
          this.isLoading.set(false);
        }
      });
  }

  protected viewDetailedProgress(courseId: number): void {
    this.router.navigate(['/learner/progress', courseId]);
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
}
