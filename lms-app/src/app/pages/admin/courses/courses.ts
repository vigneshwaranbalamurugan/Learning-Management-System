import { Component, OnInit, signal, inject, DestroyRef, effect, untracked, computed } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../environments/environment';
import { PaginationComponent } from '../../../components/pagination/pagination.component';
import { ToastService } from '../../../services/toast.service';
import { untilDestroyed } from '../../../rxjs/until-destroyed';
import { CourseResponse, CourseSummaryStatsResponse, CategoryResponse, InstructorMetadata } from '../../../models/course';
import { CourseStatus } from '../../../enums/course-status.enum';
import { RouterModule, Router } from '@angular/router';
import { Dropdown } from '../../../components/dropdown/dropdown';
import { Button } from '../../../components/button/button';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-admin-courses',
  standalone: true,
  imports: [CommonModule, PaginationComponent, RouterModule, Dropdown, Button, FormsModule],
  templateUrl: './courses.html',
  providers: [DatePipe]
})
export class AdminCoursesComponent implements OnInit {
  private http = inject(HttpClient);
  private destroyRef = inject(DestroyRef);
  private toast = inject(ToastService);
  private datePipe = inject(DatePipe);
  private router = inject(Router);

  // State Signals
  protected isLoading = signal(true);
  protected courses = signal<CourseResponse[]>([]);
  protected summaryStats = signal<CourseSummaryStatsResponse | null>(null);
  
  // Drawer State
  protected isDrawerOpen = signal(false);
  protected drawerCourse = signal<CourseResponse | null>(null);

  // Bulk Actions
  protected selectedCourseIds = signal<Set<number>>(new Set<number>());
  protected isAllSelected = computed(() => {
    const ids = this.selectedCourseIds();
    const courseList = this.courses();
    return courseList.length > 0 && ids.size === courseList.length;
  });

  // Filters & Pagination
  protected activeTab = signal<'all' | 'pending'>('all');
  protected pageNumber = signal(1);
  protected pageSize = signal(10);
  protected totalCount = signal(0);
  protected totalPages = computed(() => Math.ceil(this.totalCount() / this.pageSize()) || 1);
  protected searchQuery = signal('');
  protected filterStatus = signal<string>('');
  protected filterCategory = signal<string>('');

  protected mathMin = Math.min;

  // Dropdown options
  protected statusOptions = [
    { value: '', label: 'All Statuses' },
    { value: String(CourseStatus.Draft), label: 'Draft' },
    { value: String(CourseStatus.Published), label: 'Published' },
    { value: String(CourseStatus.PendingApproval), label: 'Pending Approval' },
    { value: String(CourseStatus.Archived), label: 'Archived' }
  ];
  
  protected categoryOptions = signal<{value: string, label: string}[]>([{ value: '', label: 'All Categories' }]);

  constructor() {
    effect(() => {
      const page = this.pageNumber();
      const tab = this.activeTab();
      const status = this.filterStatus();
      const category = this.filterCategory();
      
      untracked(() => {
        this.fetchCourses(page, tab, status, category);
      });
    });
  }

  ngOnInit(): void {
    this.fetchSummaryStats();
    this.fetchFiltersMetadata();
  }

  private fetchSummaryStats(): void {
    this.http.get<CourseSummaryStatsResponse>(`${environment.apiUrl}/Courses/summary`)
      .pipe(untilDestroyed(this.destroyRef))
      .subscribe({
        next: (stats) => this.summaryStats.set(stats),
        error: (err) => {
          console.error('Failed to load summary stats', err);
        }
      });
  }

  private fetchFiltersMetadata(): void {
    this.http.get<{categories: CategoryResponse[]}>(`${environment.apiUrl}/Courses/filters-metadata`)
      .pipe(untilDestroyed(this.destroyRef))
      .subscribe({
        next: (metadata) => {
          const cats = metadata.categories.map(c => ({ value: c.id.toString(), label: c.name }));
          this.categoryOptions.set([{ value: '', label: 'All Categories' }, ...cats]);
        },
        error: (err) => console.error('Error fetching categories:', err)
      });
  }

  private fetchCourses(page: number, tab: string, status: string, category: string): void {
    this.isLoading.set(true);
    let url = `${environment.apiUrl}/Courses/all?pageNumber=${page}&pageSize=${this.pageSize()}`;
    
    if (tab === 'pending') {
      url = `${environment.apiUrl}/Courses/pending?pageNumber=${page}&pageSize=${this.pageSize()}`;
    }

    if (this.searchQuery().trim()) {
      url += `&search=${encodeURIComponent(this.searchQuery().trim())}`;
    }
    if (status) {
      url += `&statuses=${status}`;
    }
    if (category) {
      url += `&categoryIds=${category}`;
    }

    this.http.get<any>(url)
      .pipe(untilDestroyed(this.destroyRef))
      .subscribe({
        next: (res) => {
          this.courses.set(res.courses || []);
          this.totalCount.set(res.totalCount || 0);
          this.isLoading.set(false);
          // Clear selection on page change
          this.selectedCourseIds.set(new Set<number>());
        },
        error: (err) => {
          this.toast.showError('Failed to load courses.');
          this.isLoading.set(false);
        }
      });
  }

  protected onSearch(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.searchQuery.set(input.value);
    this.pageNumber.set(1);
    this.fetchCourses(1, this.activeTab(), this.filterStatus(), this.filterCategory());
  }

  protected onTabChange(tab: 'all' | 'pending'): void {
    this.activeTab.set(tab);
    this.pageNumber.set(1);
  }
  
  protected onFilterStatusChange(status: string): void {
    this.filterStatus.set(status);
    this.pageNumber.set(1);
  }

  protected onFilterCategoryChange(category: string): void {
    this.filterCategory.set(category);
    this.pageNumber.set(1);
  }

  protected onPageChange(newPage: number): void {
    this.pageNumber.set(newPage);
  }

  // --- Drawer Methods ---
  protected openDrawer(course: CourseResponse): void {
    this.drawerCourse.set(course);
    this.isDrawerOpen.set(true);
  }

  protected navigateToCreate() {
    this.router.navigate(['/admin/courses/new']);
  }

  protected navigateToEdit(slug: string, event?: Event) {
    event?.stopPropagation();
    this.router.navigate(['/admin/courses', slug, 'builder']);
  }

  protected closeDrawer(): void {
    this.isDrawerOpen.set(false);
    setTimeout(() => this.drawerCourse.set(null), 300); // Wait for animation
  }

  // --- Bulk Selection ---
  protected toggleSelectAll(event: Event): void {
    const isChecked = (event.target as HTMLInputElement).checked;
    if (isChecked) {
      const allIds = new Set(this.courses().map(c => c.id));
      this.selectedCourseIds.set(allIds);
    } else {
      this.selectedCourseIds.set(new Set<number>());
    }
  }

  protected toggleSelection(courseId: number, event?: Event): void {
    event?.stopPropagation();
    const current = new Set(this.selectedCourseIds());
    if (current.has(courseId)) {
      current.delete(courseId);
    } else {
      current.add(courseId);
    }
    this.selectedCourseIds.set(current);
  }

  protected clearSelection(): void {
    this.selectedCourseIds.set(new Set<number>());
  }

  protected isSelected(courseId: number): boolean {
    return this.selectedCourseIds().has(courseId);
  }

  // --- Actions ---
  protected approveCourse(id: number, event?: Event): void {
    event?.stopPropagation();
    this.http.patch(`${environment.apiUrl}/Courses/${id}/review`, { action: 'Approve' })
      .pipe(untilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.toast.showSuccess('Course approved successfully');
          this.fetchSummaryStats();
          this.fetchCourses(this.pageNumber(), this.activeTab(), this.filterStatus(), this.filterCategory());
        },
        error: () => this.toast.showError('Failed to approve course')
      });
  }

  protected previewCourse(course: CourseResponse): void {
    // Navigate to course detail and append query param for ID if needed
    this.router.navigate(['/admin/courses/preview', course.slug || course.id], {
      queryParams: { courseId: course.id }
    });
  }

  protected archiveCourse(id: number, event?: Event): void {
    event?.stopPropagation();
    this.http.patch(`${environment.apiUrl}/Courses/${id}/archive`, { reason: 'Admin archived' })
      .pipe(untilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.toast.showSuccess('Course archived successfully');
          this.fetchSummaryStats();
          this.fetchCourses(this.pageNumber(), this.activeTab(), this.filterStatus(), this.filterCategory());
        },
        error: () => this.toast.showError('Failed to archive course')
      });
  }

  protected bulkApprove(): void {
    const ids = Array.from(this.selectedCourseIds());
    // For simplicity, handle sequentially or in parallel, depending on API.
    // Assuming we do it one by one for now or a new bulk endpoint.
    let count = 0;
    ids.forEach(id => {
      this.http.patch(`${environment.apiUrl}/Courses/${id}/review`, { action: 'Approve' })
        .pipe(untilDestroyed(this.destroyRef))
        .subscribe({
          next: () => {
            count++;
            if (count === ids.length) {
              this.toast.showSuccess(`Approved ${count} courses`);
              this.selectedCourseIds.set(new Set());
              this.fetchSummaryStats();
              this.fetchCourses(this.pageNumber(), this.activeTab(), this.filterStatus(), this.filterCategory());
            }
          }
        });
    });
  }

  // --- Helpers ---
  protected getStatusClasses(status: number | string): string {
    const s = typeof status === 'string' ? parseInt(status, 10) : status;
    switch (s) {
      case CourseStatus.Published: return 'bg-green-100 text-green-700 border border-green-200'; // Published
      case CourseStatus.PendingApproval: return 'bg-orange-100 text-orange-700 border border-orange-200'; // Pending
      case CourseStatus.Archived: return 'bg-slate-100 text-slate-600 border border-slate-200'; // Archived
      default: return 'bg-blue-100 text-blue-700 border border-blue-200'; // Draft
    }
  }

  protected getStatusLabel(status: number | string): string {
    const s = typeof status === 'string' ? parseInt(status, 10) : status;
    switch (s) {
      case CourseStatus.Published: return 'Published';
      case CourseStatus.PendingApproval: return 'Pending Review';
      case CourseStatus.Archived: return 'Archived';
      default: return 'Draft';
    }
  }
}
