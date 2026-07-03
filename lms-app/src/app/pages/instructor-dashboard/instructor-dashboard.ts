import { Component, inject, OnInit, signal, DestroyRef, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AuthService } from '@services/auth.service';
import { untilDestroyed } from '../../rxjs/until-destroyed';
import { forkJoin } from 'rxjs';
import { AnalyticsService } from '@services/analytics.service';
import { CourseService } from '@services/course.service';
import { Router } from '@angular/router';
import { CourseStatus } from '@enums/course-status.enum';

@Component({
  selector: 'app-instructor-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './instructor-dashboard.html'
})
export class InstructorDashboard implements OnInit {
  protected CourseStatus = CourseStatus;
  protected authService = inject(AuthService);
  private courseService = inject(CourseService);
  private analyticsService = inject(AnalyticsService);
  private destroyRef = inject(DestroyRef);
  private router=inject(Router);

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
  protected showAllCourses = signal(false);
  protected displayedCourses = computed(() => this.showAllCourses() ? this.courses() : this.courses().slice(0, 4));

  protected recentEnrollments = signal<any[]>([]);
  protected showAllEnrollments = signal(false);
  protected displayedEnrollments = computed(() => this.showAllEnrollments() ? this.recentEnrollments() : this.recentEnrollments().slice(0, 6));
  
  protected isLoading = signal(true);

  protected getInitials(name: string): string {
    if (!name) return 'U';
    const parts = name.trim().split(/\s+/);
    if (parts.length >= 2) {
      return (parts[0][0] + parts[1][0]).toUpperCase();
    }
    return parts[0].substring(0, 2).toUpperCase();
  }
  
  protected openCreateCourse(){
    this.router.navigate(["/instructor/courses/new"]);
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
      case CourseStatus.Draft: return 'Draft';
      case CourseStatus.Published: return 'Published';
      case CourseStatus.Archived: return 'Archived';
      case CourseStatus.PendingApproval: return 'Pending Approval';
      case CourseStatus.Rejected: return 'Rejected';
      default: return String(status);
    }
  }

  protected getStatusClasses(statusCode: number): string {
    switch (statusCode) {
      case CourseStatus.Draft:
        return 'bg-gray-100 text-gray-600';
      case CourseStatus.Published:
        return 'bg-emerald-50 text-emerald-600';
      case CourseStatus.Archived:
        return 'bg-slate-100 text-slate-600';
      case CourseStatus.PendingApproval:
        return 'bg-amber-50 text-amber-600';
      case CourseStatus.Rejected:
        return 'bg-rose-50 text-rose-600';
      default:
        return 'bg-gray-100 text-gray-600';
    }
  }

  ngOnInit(): void {
    forkJoin({
      analytics: this.analyticsService.getInstructorAnalytics(),
      courses: this.courseService.getMyCourses()
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

        const mapped = (courses.courses || []).map(c => ({
          title: c.title,
          students: 0,
          status: this.getStatusLabel(c.status),
          statusCode: typeof c.status === 'string' ? parseInt(c.status, 10) : c.status,
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
