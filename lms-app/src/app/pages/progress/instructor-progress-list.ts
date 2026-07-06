import { Component, inject, OnInit, signal, DestroyRef, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { ToastService } from '@services/toast.service';
import { untilDestroyed } from '../../rxjs/until-destroyed';
import { Loader } from '@components/loader/loader';
import { CourseService } from '@services/course.service';
import { PublishStatus } from '../../enums/publish-status.enum';
import { InstructorCourseCardResponse, PagedInstructorCourseResponse } from '@models/course';
import { PaginationComponent } from '@components/pagination/pagination.component';
import { SearchInput } from '@components/search-input/search-input';

@Component({
  selector: 'app-instructor-progress-list',
  standalone: true,
  imports: [CommonModule, RouterModule, Loader, PaginationComponent, SearchInput],
  templateUrl: './instructor-progress-list.html'
})
export class InstructorProgressList implements OnInit {
  private courseService = inject(CourseService);
  private toastService = inject(ToastService);
  private router = inject(Router);
  private destroyRef = inject(DestroyRef);
  private cdr = inject(ChangeDetectorRef);

  protected courses = signal<InstructorCourseCardResponse[]>([]);
  protected isLoading = signal(true);

  protected page = signal(1);
  protected pageSize = signal(10);
  protected totalCount = signal(0);
  protected totalPages = signal(0);
  protected searchQuery = signal('');

  ngOnInit(): void {
    this.loadCourses();
  }

  private loadCourses(): void {
    this.isLoading.set(true);
    const query = {
      statuses: String(PublishStatus.Published),
      pageNumber: this.page(),
      pageSize: this.pageSize(),
      search: this.searchQuery() || undefined
    };

    this.courseService.getMyCourses(query)
      .pipe(untilDestroyed(this.destroyRef))
      .subscribe({
        next: (data: PagedInstructorCourseResponse) => {
          this.courses.set(data?.courses || []);
          this.totalCount.set(data?.totalCount || 0);
          this.totalPages.set(data?.totalPages || 0);
          this.isLoading.set(false);
          this.cdr.detectChanges();
        },
        error: (err) => {
          this.toastService.showApiError(err, 'Failed to load courses.');
          this.isLoading.set(false);
        }
      });
  }

  onSearchChange(search: string) {
    this.searchQuery.set(search);
    this.page.set(1);
    this.loadCourses();
  }

  onPageChange(newPage: number) {
    this.page.set(newPage);
    this.loadCourses();
  }

  protected viewProgress(courseId: number): void {
    this.router.navigate(['/instructor/progress/course', courseId]);
  }
}
