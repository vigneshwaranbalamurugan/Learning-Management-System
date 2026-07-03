import { Component, inject, OnInit, signal, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AuthService } from '@services/auth.service';
import { EnrollmentResponse } from '@models/enrollment';
import { untilDestroyed } from '../../rxjs/until-destroyed';
import { forkJoin } from 'rxjs';

import { Loader } from '@components/loader/loader';
import { AnalyticsService } from '@services/analytics.service';
import { EnrollmentService } from '@services/enrollment.service';

@Component({
  selector: 'app-learner-dashboard',
  standalone: true,
  imports: [CommonModule, Loader],
  templateUrl: './learner-dashboard.html'
})
export class LearnerDashboard implements OnInit {
  protected authService = inject(AuthService);
  private enrollmentService = inject(EnrollmentService);
  private analyticsService = inject(AnalyticsService);
  private destroyRef = inject(DestroyRef);

  protected get user() {
    return this.authService.currentUser();
  }

  protected stats = signal<any[]>([
    { label: 'Active Courses', value: '0', iconBg: '#eff6ff', iconColor: '#2563eb' },
    { label: 'Completed Courses', value: '0', iconBg: '#ecfdf5', iconColor: '#059669' },
    { label: 'Average Progress', value: '0%', iconBg: '#fff7ed', iconColor: '#ea580c' },
    { label: 'Avg Quiz Score', value: 'N/A', iconBg: '#fdf2f8', iconColor: '#db2777' }
  ]);

  protected courses = signal<any[]>([]);
  protected showAllCourses = signal<boolean>(false);
  protected isLoading = signal<boolean>(true);

  protected get displayedCourses() {
    const list = this.courses();
    return this.showAllCourses() ? list : list.slice(0, 4);
  }

  protected toggleViewAll() {
    this.showAllCourses.update(val => !val);
  }

  ngOnInit(): void {
    forkJoin({
      analytics: this.analyticsService.getLearnerAnalytics(),
      enrollments: this.enrollmentService.getAllMyEnrollments()
    }).pipe(
      untilDestroyed(this.destroyRef)
    ).subscribe({
      next: ({ analytics, enrollments }) => {
        this.stats.set([
          { label: 'Active Courses', value: analytics.inProgressCourses.toString(), iconBg: '#eff6ff', iconColor: '#2563eb' },
          { label: 'Completed Courses', value: analytics.completedCourses.toString(), iconBg: '#ecfdf5', iconColor: '#059669' },
          { label: 'Average Progress', value: `${Math.round(analytics.averageProgressPercentage)}%`, iconBg: '#fff7ed', iconColor: '#ea580c' },
          { label: 'Avg Quiz Score', value: analytics.averageQuizScore != null ? `${Math.round(analytics.averageQuizScore)}%` : 'N/A', iconBg: '#fdf2f8', iconColor: '#db2777' }
        ]);

        const mapped = enrollments.map(e => ({
          title: e.courseTitle,
          progress: Math.round(e.progressPercentage),
          category: 'Course',
          level: e.isCompleted ? 'Completed' : 'In Progress'
        }));
        this.courses.set(mapped);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Failed to load learner dashboard analytics:', err);
        this.isLoading.set(false);
      }
    });
  }
}
