import { Component, inject, OnInit, signal, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { DashboardService } from '@services/dashboard.service';
import { ToastService } from '@services/toast.service';
import { untilDestroyed } from '../../rxjs/until-destroyed';
import { Loader } from '@components/loader/loader';

@Component({
  selector: 'app-instructor-progress-list',
  standalone: true,
  imports: [CommonModule, RouterModule, Loader],
  templateUrl: './instructor-progress-list.html'
})
export class InstructorProgressList implements OnInit {
  private dashboardService = inject(DashboardService);
  private toastService = inject(ToastService);
  private router = inject(Router);
  private destroyRef = inject(DestroyRef);

  protected courses = signal<any[]>([]);
  protected isLoading = signal(true);

  ngOnInit(): void {
    this.loadCourses();
  }

  private loadCourses(): void {
    this.isLoading.set(true);
    this.dashboardService.getMyCourses()
      .pipe(untilDestroyed(this.destroyRef))
      .subscribe({
        next: (data) => {
          this.courses.set(data?.courses || []);
          this.isLoading.set(false);
        },
        error: (err) => {
          this.toastService.showApiError(err, 'Failed to load courses.');
          this.isLoading.set(false);
        }
      });
  }

  protected viewProgress(courseId: number): void {
    this.router.navigate(['/instructor/progress/course', courseId]);
  }
}
