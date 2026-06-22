import { Component, OnInit, signal, inject, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { DashboardService } from '@services/dashboard.service';
import { ToastService } from '@services/toast.service';
import { CourseProgressResponse } from '@models/dashboard';
import { untilDestroyed } from '../../rxjs/until-destroyed';
import { Loader } from '@components/loader/loader';

@Component({
  selector: 'app-progress-detail-page',
  standalone: true,
  imports: [CommonModule, RouterModule, Loader],
  templateUrl: './progress-detail.html'
})
export class ProgressDetailPage implements OnInit {
  private dashboardService = inject(DashboardService);
  private toastService = inject(ToastService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private destroyRef = inject(DestroyRef);

  protected progress = signal<CourseProgressResponse | null>(null);
  protected isLoading = signal(true);

  ngOnInit(): void {
    const courseId = this.route.snapshot.paramMap.get('id');
    if (courseId) {
      this.loadProgress(Number(courseId));
    } else {
      this.goBack();
    }
  }

  private loadProgress(courseId: number): void {
    this.isLoading.set(true);
    this.dashboardService.getCourseProgress(courseId)
      .pipe(untilDestroyed(this.destroyRef))
      .subscribe({
        next: (data) => {
          this.progress.set(data);
          this.isLoading.set(false);
        },
        error: (err) => {
          this.toastService.showApiError(err, 'Failed to load detailed progress.');
          this.isLoading.set(false);
          this.goBack();
        }
      });
  }

  protected goBack(): void {
    this.router.navigate(['/learner/progress']);
  }
}
