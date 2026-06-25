import { Component, OnInit, signal, computed, inject, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { InstructorProgressService, StudentProgressSummaryDto, StudentCourseProgressResponse } from '@services/instructor-progress.service';
import { ToastService } from '@services/toast.service';
import { untilDestroyed } from '../../rxjs/until-destroyed';
import { Loader } from '@components/loader/loader';
import { CourseService } from '@services/course.service';
import { ProgressService } from '@services/progress.service';

@Component({
  selector: 'app-instructor-course-progress',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, Loader],
  templateUrl: './instructor-course-progress.html'
})
export class InstructorCourseProgress implements OnInit {
  private instructorProgressService = inject(InstructorProgressService);
  private dashboardProgressService = inject(ProgressService);
  private courseService = inject(CourseService);
  private toastService = inject(ToastService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private destroyRef = inject(DestroyRef);

  protected courseId = 0;
  protected courseTitle = signal<string>('');
  
  protected students = signal<StudentProgressSummaryDto[]>([]);
  protected isLoading = signal(true);
  protected searchQuery = signal('');

  // Statistics
  protected totalStudents = computed(() => this.students().length);
  protected completedCount = computed(() => this.students().filter(s => s.isCompleted).length);
  protected averageProgress = computed(() => {
    const list = this.students();
    if (list.length === 0) return 0;
    const sum = list.reduce((total, s) => total + (s.progressPercentage || 0), 0);
    return Math.round(sum / list.length);
  });

  // Local Search filtering
  protected filteredStudents = computed(() => {
    let list = this.students();
    const query = this.searchQuery().toLowerCase().trim();

    if (query) {
      list = list.filter(s => 
        (s.studentName && s.studentName.toLowerCase().includes(query)) ||
        (s.studentEmail && s.studentEmail.toLowerCase().includes(query)) ||
        (s.batchName && s.batchName.toLowerCase().includes(query))
      );
    }
    return list;
  });

  // Selected student detailed progress drawer state
  protected selectedStudent = signal<StudentProgressSummaryDto | null>(null);
  protected detailedProgress = signal<StudentCourseProgressResponse | null>(null);
  protected isDetailLoading = signal(false);

  ngOnInit(): void {
    this.route.paramMap
      .pipe(untilDestroyed(this.destroyRef))
      .subscribe(params => {
        const id = params.get('courseId');
        if (id) {
          this.courseId = Number(id);
          this.loadData();
        } else {
          this.router.navigate(['/instructor/progress']);
        }
      });
  }

  private loadData(): void {
    this.isLoading.set(true);

    // Get Course details to show title
    this.courseService.getCourseById(this.courseId)
      .pipe(untilDestroyed(this.destroyRef))
      .subscribe({
        next: (course) => {
          if (course.status !== 2 && course.status !== 'Published') {
            this.toastService.showError('Cannot view progress for an unpublished course.');
            this.router.navigate(['/instructor/progress']);
            return;
          }
          this.courseTitle.set(course.title);
          this.loadStudentsProgress();
        },
        error: (err) => {
          this.toastService.showApiError(err, 'Failed to load course details.');
          this.router.navigate(['/instructor/progress']);
        }
      });
  }

  private loadStudentsProgress(): void {
    this.dashboardProgressService.getStudentsProgress(this.courseId)
      .pipe(untilDestroyed(this.destroyRef))
      .subscribe({
        next: (data) => {
          this.students.set(data || []);
          this.isLoading.set(false);
        },
        error: (err) => {
          this.toastService.showApiError(err, 'Failed to load student progress.');
          this.isLoading.set(false);
        }
      });
  }

  protected viewStudentDetails(student: StudentProgressSummaryDto): void {
    this.selectedStudent.set(student);
    this.isDetailLoading.set(true);
    this.detailedProgress.set(null);

    this.instructorProgressService.getStudentDetailedProgress(this.courseId, student.studentId)
      .pipe(untilDestroyed(this.destroyRef))
      .subscribe({
        next: (data) => {
          this.detailedProgress.set(data);
          this.isDetailLoading.set(false);
        },
        error: (err) => {
          this.toastService.showApiError(err, 'Failed to load detailed student progress.');
          this.isDetailLoading.set(false);
          this.selectedStudent.set(null);
        }
      });
  }

  protected closeDrawer(): void {
    this.selectedStudent.set(null);
    this.detailedProgress.set(null);
  }

  protected getInitials(name: string): string {
    if (!name) return 'U';
    const parts = name.trim().split(/\s+/);
    if (parts.length >= 2) {
      return (parts[0][0] + parts[1][0]).toUpperCase();
    }
    return parts[0].substring(0, 2).toUpperCase();
  }

  protected goBack(): void {
    this.router.navigate(['/instructor/progress']);
  }
}
