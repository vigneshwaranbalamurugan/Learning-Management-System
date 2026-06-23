import { Component, inject, OnInit, signal, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { InstructorCourseLayout } from '../instructor-course-layout/instructor-course-layout';
import { DashboardService } from '@services/dashboard.service';
import { untilDestroyed } from '../../rxjs/until-destroyed';

@Component({
  selector: 'app-instructor-course-learners',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './instructor-course-learners.html'
})
export class InstructorCourseLearners implements OnInit {
  protected layout = inject(InstructorCourseLayout);
  private dashboardService = inject(DashboardService);
  private route = inject(ActivatedRoute);
  private destroyRef = inject(DestroyRef);

  protected learners = signal<any[]>([]);
  protected isLoading = signal(true);

  protected get course() {
    return this.layout.course();
  }

  ngOnInit() {
    const interval = setInterval(() => {
      const courseId = this.layout.courseId();
      if (courseId) {
        this.loadLearners(courseId);
        clearInterval(interval);
      }
    }, 50);
    this.destroyRef.onDestroy(() => clearInterval(interval));
  }

  private loadLearners(courseId: number) {
    this.isLoading.set(true);
    this.dashboardService.getStudentsProgress(courseId)
      .pipe(untilDestroyed(this.destroyRef))
      .subscribe({
        next: (data) => {
          this.learners.set(data || []);
          this.isLoading.set(false);
        },
        error: (err) => {
          console.error('Failed to load course learners:', err);
          this.isLoading.set(false);
        }
      });
  }

  protected getInitials(name: string): string {
    if (!name) return 'U';
    const parts = name.trim().split(/\s+/);
    if (parts.length >= 2) {
      return (parts[0][0] + parts[1][0]).toUpperCase();
    }
    return parts[0].substring(0, 2).toUpperCase();
  }
}
