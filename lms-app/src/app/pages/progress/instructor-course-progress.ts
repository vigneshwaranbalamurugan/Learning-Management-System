import { Component, OnInit, signal, computed, inject, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { InstructorProgressService, StudentCourseProgressResponse } from '@services/instructor-progress.service';
import { StudentProgressSummaryDto, PagedStudentProgressResponse } from '@models/progress';
import { ToastService } from '@services/toast.service';
import { untilDestroyed } from '../../rxjs/until-destroyed';
import { Loader } from '@components/loader/loader';
import { CourseService } from '@services/course.service';
import { ProgressService } from '@services/progress.service';
import { PublishStatus } from '../../enums/publish-status.enum';
import { CourseStatus } from '../../enums/course-status.enum';
import { PaginationComponent } from '@components/pagination/pagination.component';
import { SearchInput } from '@components/search-input/search-input';
import { Dropdown } from '@components/dropdown/dropdown';

@Component({
  selector: 'app-instructor-course-progress',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, Loader, PaginationComponent, SearchInput, Dropdown],
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
  
  // Pagination
  protected page = signal(1);
  protected pageSize = signal(10);
  protected totalPages = signal(0);
  protected totalCount = signal(0);
  
  // Filters
  protected searchQuery = signal('');
  protected selectedStatus = signal('');

  // Statistics
  protected totalStudents = signal(0);
  protected completedCount = signal(0);
  protected averageProgress = signal(0);

  protected statusOptions = [
    { value: '', label: 'All Statuses' },
    { value: 'true', label: 'Completed' },
    { value: 'false', label: 'In Progress' }
  ];

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

    const isCompletedFilter = this.selectedStatus() ? this.selectedStatus() === 'true' : undefined;

    this.dashboardProgressService.getStudentsProgress(this.courseId, this.page(), this.pageSize(), this.searchQuery(), isCompletedFilter)
      .pipe(untilDestroyed(this.destroyRef))
      .subscribe({
        next: (data: PagedStudentProgressResponse) => {
          if (data.courseStatus !== 'Published') {
            this.toastService.showError('Cannot view progress for an unpublished course.');
            this.router.navigate(['/instructor/progress']);
            return;
          }
          this.courseTitle.set(data.courseTitle);
          this.students.set(data.students || []);
          this.totalCount.set(data.totalCount);
          this.totalPages.set(data.totalPages);
          this.totalStudents.set(data.totalStudents);
          this.completedCount.set(data.completedCount);
          this.averageProgress.set(data.averageProgress);
          this.isLoading.set(false);
        },
        error: (err) => {
          this.toastService.showApiError(err, 'Failed to load student progress.');
          this.router.navigate(['/instructor/progress']);
          this.isLoading.set(false);
        }
      });
  }

  onSearchChange(search: string) {
    this.searchQuery.set(search);
    this.page.set(1);
    this.loadData();
  }

  onStatusChange(status: string) {
    this.selectedStatus.set(status);
    this.page.set(1);
    this.loadData();
  }

  onPageChange(newPage: number) {
    this.page.set(newPage);
    this.loadData();
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
