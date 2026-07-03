import { Component, inject, OnInit, signal, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { InstructorCourseLayout } from '../instructor-course-layout/instructor-course-layout';
import { untilDestroyed } from '../../rxjs/until-destroyed';
import { ProgressService } from '@services/progress.service';

interface MonthlyStat {
  month: string;
  count: number;
  heightPercentage: number;
  isCurrent: boolean;
}

@Component({
  selector: 'app-instructor-course-analytics',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './instructor-course-analytics.html'
})
export class InstructorCourseAnalytics implements OnInit {
  protected layout = inject(InstructorCourseLayout);
  private progressService = inject(ProgressService);
  private route = inject(ActivatedRoute);
  private destroyRef = inject(DestroyRef);

  protected monthlyStats: MonthlyStat[] = [];
  protected momGrowthText = '0% Month-over-Month';
  protected isGrowthPositive = true;
  protected isGrowthZero = true;
  protected isLoading = signal(true);

  protected get course() {
    return this.layout.course();
  }

  ngOnInit() {
    const interval = setInterval(() => {
      const courseId = this.layout.courseId();
      if (courseId) {
        this.loadLearnersData(courseId);
        clearInterval(interval);
      }
    }, 50);
    this.destroyRef.onDestroy(() => clearInterval(interval));
  }

  private loadLearnersData(courseId: number) {
    this.isLoading.set(true);
    this.progressService.getCourseAnalytics(courseId)
      .pipe(untilDestroyed(this.destroyRef))
      .subscribe({
        next: (data) => {
          this.monthlyStats = data.monthlyStats || [];
          this.momGrowthText = data.momGrowthText || '0% Month-over-Month';
          this.isGrowthPositive = data.isGrowthPositive;
          this.isGrowthZero = data.isGrowthZero;
          this.isLoading.set(false);
        },
        error: (err) => {
          console.error('Failed to load course analytics:', err);
          this.isLoading.set(false);
        }
      });
  }
}
