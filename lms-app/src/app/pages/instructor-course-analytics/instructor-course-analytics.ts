import { Component, inject, OnInit, signal, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { InstructorCourseLayout } from '../instructor-course-layout/instructor-course-layout';
import { DashboardService } from '@services/dashboard.service';
import { untilDestroyed } from '../../rxjs/until-destroyed';

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
  private dashboardService = inject(DashboardService);
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
    this.dashboardService.getStudentsProgress(courseId)
      .pipe(untilDestroyed(this.destroyRef))
      .subscribe({
        next: (students) => {
          this.computeStats(students || []);
          this.isLoading.set(false);
        },
        error: (err) => {
          console.error('Failed to load course learners for analytics:', err);
          this.isLoading.set(false);
        }
      });
  }

  private computeStats(students: any[]) {
    const now = new Date();
    const months: { month: string; year: number; monthNum: number; count: number; isCurrent: boolean }[] = [];

    // Initialize last 6 months
    for (let i = 5; i >= 0; i--) {
      const d = new Date(now.getFullYear(), now.getMonth() - i, 1);
      months.push({
        month: d.toLocaleString('default', { month: 'short' }),
        year: d.getFullYear(),
        monthNum: d.getMonth(),
        count: 0,
        isCurrent: i === 0
      });
    }

    // Group students by month
    students.forEach(student => {
      if (!student.enrolledAt) return;
      const enrolledDate = new Date(student.enrolledAt);
      const enrolledMonth = enrolledDate.getMonth();
      const enrolledYear = enrolledDate.getFullYear();

      const match = months.find(m => m.monthNum === enrolledMonth && m.year === enrolledYear);
      if (match) {
        match.count++;
      }
    });

    // Determine max count for scaling
    const maxCount = Math.max(...months.map(m => m.count));

    // Map to monthly stats with scaled height percentage
    this.monthlyStats = months.map(m => {
      let heightPercentage = 10; // min height so bar is visible
      if (maxCount > 0) {
        heightPercentage = Math.max(10, Math.round((m.count / maxCount) * 100));
      }
      return {
        month: m.month,
        count: m.count,
        heightPercentage,
        isCurrent: m.isCurrent
      };
    });

    // Compute MoM Growth
    const prevMonth = months[4]?.count || 0;
    const currentMonth = months[5]?.count || 0;

    if (prevMonth === 0) {
      if (currentMonth === 0) {
        this.momGrowthText = '0% Month-over-Month';
        this.isGrowthPositive = true;
        this.isGrowthZero = true;
      } else {
        this.momGrowthText = `↑ ${currentMonth * 100}% Month-over-Month`;
        this.isGrowthPositive = true;
        this.isGrowthZero = false;
      }
    } else {
      const diff = currentMonth - prevMonth;
      const percentage = Math.round((diff / prevMonth) * 100);
      if (percentage > 0) {
        this.momGrowthText = `↑ ${percentage}% Month-over-Month`;
        this.isGrowthPositive = true;
        this.isGrowthZero = false;
      } else if (percentage < 0) {
        this.momGrowthText = `↓ ${Math.abs(percentage)}% Month-over-Month`;
        this.isGrowthPositive = false;
        this.isGrowthZero = false;
      } else {
        this.momGrowthText = '0% Month-over-Month';
        this.isGrowthPositive = true;
        this.isGrowthZero = true;
      }
    }
  }
}
