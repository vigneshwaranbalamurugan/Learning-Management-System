import { Component, inject, OnInit, signal, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AuthService } from '@services/auth.service';
import { DashboardService } from '@services/dashboard.service';
import { untilDestroyed } from '../../rxjs/until-destroyed';
import { forkJoin } from 'rxjs';

@Component({
  selector: 'app-instructor-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './instructor-dashboard.html'
})
export class InstructorDashboard implements OnInit {
  protected authService = inject(AuthService);
  private dashboardService = inject(DashboardService);
  private destroyRef = inject(DestroyRef);

  protected get user() {
    return this.authService.currentUser();
  }

  protected stats = signal<any[]>([
    { label: 'Total Students', value: '0', iconBg: '#eff6ff', iconColor: '#2563eb' },
    { label: 'Courses Created', value: '0', iconBg: '#ecfdf5', iconColor: '#059669' },
    { label: 'Monthly Earnings', value: '₹0', iconBg: '#fdf2f8', iconColor: '#db2777' },
    { label: 'Avg Quiz Score', value: 'N/A', iconBg: '#fff7ed', iconColor: '#ea580c' }
  ]);

  protected courses = signal<any[]>([]);
  protected recentEnrollments = signal<any[]>([]);
  protected isLoading = signal(true);

  protected getInitials(name: string): string {
    if (!name) return 'U';
    const parts = name.trim().split(/\s+/);
    if (parts.length >= 2) {
      return (parts[0][0] + parts[1][0]).toUpperCase();
    }
    return parts[0].substring(0, 2).toUpperCase();
  }

  protected getFriendlyTime(dateString: string): string {
    if (!dateString) return '';
    try {
      const date = new Date(dateString);
      const diffMs = new Date().getTime() - date.getTime();
      const diffHrs = Math.floor(diffMs / (1000 * 60 * 60));
      if (diffHrs < 1) {
        const diffMins = Math.floor(diffMs / (1000 * 60));
        return `${diffMins || 1}m ago`;
      }
      if (diffHrs < 24) {
        return `${diffHrs}h ago`;
      }
      const diffDays = Math.floor(diffHrs / 24);
      return `${diffDays}d ago`;
    } catch {
      return '';
    }
  }

  protected getStatusLabel(status: number | string): string {
    const statusNum = typeof status === 'string' ? parseInt(status, 10) : status;
    switch (statusNum) {
      case 1: return 'Draft';
      case 2: return 'Published';
      case 3: return 'Archived';
      case 4: return 'Pending Approval';
      case 5: return 'Rejected';
      default: return String(status);
    }
  }

  ngOnInit(): void {
    forkJoin({
      analytics: this.dashboardService.getInstructorAnalytics(),
      courses: this.dashboardService.getMyCourses()
    }).pipe(
      untilDestroyed(this.destroyRef)
    ).subscribe({
      next: ({ analytics, courses }) => {
        this.stats.set([
          { label: 'Total Students', value: analytics.totalStudentsEnrolled.toLocaleString(), iconBg: '#eff6ff', iconColor: '#2563eb' },
          { label: 'Courses Created', value: analytics.totalCoursesCreated.toString(), iconBg: '#ecfdf5', iconColor: '#059669' },
          { label: 'Total Revenue', value: `₹${analytics.totalRevenueGenerated.toLocaleString()}`, iconBg: '#fdf2f8', iconColor: '#db2777' },
          { label: 'Avg Quiz Score', value: analytics.averageQuizScore != null ? `${Math.round(analytics.averageQuizScore)}%` : 'N/A', iconBg: '#fff7ed', iconColor: '#ea580c' }
        ]);

        if (analytics.recentEnrollments) {
          this.recentEnrollments.set(analytics.recentEnrollments);
        }

        const mapped = courses.map(c => ({
          title: c.title,
          students: 0,
          status: this.getStatusLabel(c.status),
          rating: c.averageRating
        }));
        this.courses.set(mapped);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Failed to load instructor dashboard analytics:', err);
        this.isLoading.set(false);
      }
    });
  }
}
