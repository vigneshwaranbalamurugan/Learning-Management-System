import { Component, OnInit, signal, computed, inject, DestroyRef, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, Router } from '@angular/router';
import { DashboardService } from '@services/dashboard.service';
import { ToastService } from '@services/toast.service';
import { CourseResponse, CategoryResponse } from '@models/dashboard';
import { untilDestroyed } from '../../rxjs/until-destroyed';
import { Dropdown } from '@components/dropdown/dropdown';
import { FormInput } from '@components/form-input/form-input';
import { ConfirmModal } from '@components/confirm-modal/confirm-modal';
import { Button } from '@components/button/button';

@Component({
  selector: 'app-instructor-courses',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, Dropdown, FormInput, ConfirmModal, Button],
  templateUrl: './instructor-courses.html'
})
export class InstructorCourses implements OnInit {
  private dashboardService = inject(DashboardService);
  private toastService = inject(ToastService);
  private router = inject(Router);
  private destroyRef = inject(DestroyRef);

  // States
  protected courses = signal<CourseResponse[]>([]);
  protected categories = signal<CategoryResponse[]>([]);
  protected isLoading = signal(true);
  
  // Confirmation Modal state
  protected showDeleteModal = false;
  protected courseToDelete: number | null = null;
  
  // Filtering States
  protected searchQuery = signal('');
  protected selectedStatus = signal('all');
  protected selectedCategory = signal('all');
  protected selectedDifficulty = signal('all');
  protected sortBy = signal('newest');

  // Dropdown Options
  protected statusOptions = [
    { value: 'all', label: 'Status: All' },
    { value: 'published', label: 'Published' },
    { value: 'draft', label: 'Draft' },
    { value: 'archived', label: 'Archived' },
    { value: 'pending', label: 'Pending Approval' }
  ];

  protected difficultyOptions = [
    { value: 'all', label: 'Level: All' },
    { value: 'beginner', label: 'Beginner' },
    { value: 'intermediate', label: 'Intermediate' },
    { value: 'advanced', label: 'Advanced' }
  ];

  protected sortOptions = [
    { value: 'newest', label: 'Newest First' },
    { value: 'oldest', label: 'Oldest First' },
    { value: 'enrolled', label: 'Most Enrolled' },
    { value: 'rating', label: 'Highest Rated' }
  ];

  protected categoryOptions = computed(() => {
    return [
      { value: 'all', label: 'Category: All' },
      ...this.categories().map(cat => ({ value: String(cat.id), label: cat.name }))
    ];
  });

  // Active Dropdown
  protected activeDropdownId = signal<number | null>(null);

  // Stats computed from backend payload
  protected totalCourses = computed(() => this.courses().length);
  protected publishedCourses = computed(() => {
    return this.courses().filter(c => {
      const status = String(c.status).toLowerCase();
      return status === '2' || status === 'published';
    }).length;
  });
  protected draftCourses = computed(() => {
    return this.courses().filter(c => {
      const status = String(c.status).toLowerCase();
      return status === '1' || status === 'draft';
    }).length;
  });
  protected totalLearners = computed(() => {
    return this.courses().reduce((sum, c) => sum + (c.enrolledCount || 0), 0);
  });

  // Client-side filtering & sorting
  protected filteredCourses = computed(() => {
    let list = [...this.courses()];
    const query = this.searchQuery().toLowerCase().trim();
    const status = this.selectedStatus();
    const category = this.selectedCategory();
    const difficulty = this.selectedDifficulty();
    const sort = this.sortBy();

    // 1. Search Query
    if (query) {
      list = list.filter(c => c.title.toLowerCase().includes(query) || (c.description && c.description.toLowerCase().includes(query)));
    }

    // 2. Status Filter
    if (status !== 'all') {
      list = list.filter(c => {
        const cStatus = String(c.status).toLowerCase();
        if (status === 'draft') return cStatus === '1' || cStatus === 'draft';
        if (status === 'published') return cStatus === '2' || cStatus === 'published';
        if (status === 'archived') return cStatus === '3' || cStatus === 'archived';
        if (status === 'pending') return cStatus === '4' || cStatus === 'pending approval';
        return true;
      });
    }

    // 3. Category Filter
    if (category !== 'all') {
      list = list.filter(c => String(c.categoryId) === category);
    }

    // 4. Difficulty Filter
    if (difficulty !== 'all') {
      list = list.filter(c => this.getLevelName(c.level).toLowerCase() === difficulty.toLowerCase());
    }

    // 5. Sorting
    list.sort((a, b) => {
      if (sort === 'newest') {
        return new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime();
      }
      if (sort === 'oldest') {
        return new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime();
      }
      if (sort === 'enrolled') {
        return (b.enrolledCount || 0) - (a.enrolledCount || 0);
      }
      if (sort === 'rating') {
        return (b.averageRating || 0) - (a.averageRating || 0);
      }
      return 0;
    });

    return list;
  });

  // Check if any filter is active
  protected hasActiveFilters = computed(() => {
    return this.searchQuery() !== '' ||
      this.selectedStatus() !== 'all' ||
      this.selectedCategory() !== 'all' ||
      this.selectedDifficulty() !== 'all';
  });

  ngOnInit(): void {
    this.loadData();
  }

  private loadData(): void {
    this.isLoading.set(true);
    this.dashboardService.getMyCourses()
      .pipe(untilDestroyed(this.destroyRef))
      .subscribe({
        next: (data) => {
          this.courses.set(data || []);
          this.loadCategories();
        },
        error: (err) => {
          this.toastService.showApiError(err, 'Failed to load courses.');
          this.isLoading.set(false);
        }
      });
  }

  private loadCategories(): void {
    this.dashboardService.getAllCategories()
      .pipe(untilDestroyed(this.destroyRef))
      .subscribe({
        next: (data) => {
          this.categories.set(data || []);
          this.isLoading.set(false);
        },
        error: (err) => {
          this.toastService.showApiError(err, 'Failed to load categories.');
          this.isLoading.set(false);
        }
      });
  }

  // Helpers for Status Mapping
  protected getStatusString(status: number | string): string {
    const statusNum = typeof status === 'string' ? parseInt(status, 10) : status;
    switch (statusNum) {
      case 1: return 'Draft';
      case 2: return 'Published';
      case 3: return 'Archived';
      case 4: return 'Pending';
      case 5: return 'Rejected';
      default: return String(status);
    }
  }

  protected getStatusBadgeClass(status: number | string): string {
    const statusStr = this.getStatusString(status).toLowerCase();
    if (statusStr === 'published') return 'bg-emerald-50 text-emerald-700 border-emerald-200';
    if (statusStr === 'draft') return 'bg-gray-100 text-gray-700 border-gray-200';
    if (statusStr === 'archived') return 'bg-amber-50 text-amber-700 border-amber-200';
    if (statusStr === 'pending') return 'bg-blue-50 text-blue-700 border-blue-200';
    return 'bg-red-50 text-red-700 border-red-200';
  }

  protected getLevelName(level: number | string): string {
    const lvl = String(level).trim().toLowerCase();
    if (lvl === '1' || lvl === 'beginner') return 'Beginner';
    if (lvl === '2' || lvl === 'intermediate') return 'Intermediate';
    if (lvl === '3' || lvl === 'advanced') return 'Advanced';
    return 'All Levels';
  }

  protected getLevelBadgeClass(level: number | string): string {
    const lvl = String(level).trim().toLowerCase();
    if (lvl === '1' || lvl === 'beginner') return 'bg-slate-100 text-slate-700';
    if (lvl === '2' || lvl === 'intermediate') return 'bg-indigo-50 text-indigo-700';
    if (lvl === '3' || lvl === 'advanced') return 'bg-rose-50 text-rose-700';
    return 'bg-gray-100 text-gray-600';
  }

  protected isCoursePublished(course: CourseResponse): boolean {
    const status = String(course.status).toLowerCase();
    return status === '2' || status === 'published';
  }

  protected getCategoryName(categoryId: number): string {
    const cat = this.categories().find(c => c.id === categoryId);
    return cat ? cat.name : 'General';
  }

  // Dropdown Management
  protected toggleDropdown(courseId: number, event: Event): void {
    event.stopPropagation();
    if (this.activeDropdownId() === courseId) {
      this.activeDropdownId.set(null);
    } else {
      this.activeDropdownId.set(courseId);
    }
  }

  @HostListener('document:click')
  protected closeAllDropdowns(): void {
    this.activeDropdownId.set(null);
  }

  // Course actions
  protected onCreateCourse(): void {
    this.toastService.showInfo('Redirecting to Course Creation wizard...');
    // If a route exists, navigate: this.router.navigate(['/instructor/courses/new']);
  }

  protected onImportCourse(): void {
    this.toastService.showInfo('Course import wizard under development.');
  }

  protected editCourse(slug: string): void {
    this.router.navigate([`/instructor/courses/${slug}/overview`]);
  }

  protected previewCourse(slug: string, courseId: number): void {
    // Pass courseId via router state as CourseDetail relies on it if loaded directly from here
    this.router.navigate([`/instructor/preview/${slug}`], { state: { courseId } });
  }

  protected duplicateCourse(courseId: number): void {
    this.toastService.showSuccess('Course duplicated successfully (Mocked).');
  }

  protected togglePublishStatus(course: CourseResponse): void {
    const isPublished = this.isCoursePublished(course);
    const nextPublishState = !isPublished;
    
    this.dashboardService.publishCourse(course.id, nextPublishState)
      .pipe(untilDestroyed(this.destroyRef))
      .subscribe({
        next: (updatedCourse) => {
          this.toastService.showSuccess(
            nextPublishState ? 'Course published successfully!' : 'Course unpublished successfully!'
          );
          // Update the locally-cached course item status
          this.courses.update(list => list.map(c => c.id === course.id ? { ...c, status: nextPublishState ? 2 : 1 } : c));
        },
        error: (err) => {
          this.toastService.showApiError(err, 'Failed to update course publish status.');
        }
      });
  }

  protected archiveCourse(courseId: number): void {
    this.toastService.showSuccess('Course archived successfully (Mocked).');
    this.courses.update(list => list.map(c => c.id === courseId ? { ...c, status: 3 } : c));
  }

  protected confirmDeleteCourse(courseId: number): void {
    this.courseToDelete = courseId;
    this.showDeleteModal = true;
  }

  protected deleteCourse(): void {
    if (this.courseToDelete === null) return;
    this.dashboardService.deleteCourse(this.courseToDelete)
      .pipe(untilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.toastService.showSuccess('Course deleted successfully.');
          this.courses.update(list => list.filter(c => c.id !== this.courseToDelete));
          this.closeDeleteModal();
        },
        error: (err) => {
          this.toastService.showApiError(err, 'Failed to delete course.');
          this.closeDeleteModal();
        }
      });
  }

  protected closeDeleteModal(): void {
    this.showDeleteModal = false;
    this.courseToDelete = null;
  }

  protected removeFilter(filterName: string): void {
    if (filterName === 'search') this.searchQuery.set('');
    if (filterName === 'status') this.selectedStatus.set('all');
    if (filterName === 'category') this.selectedCategory.set('all');
    if (filterName === 'difficulty') this.selectedDifficulty.set('all');
  }

  protected resetFilters(): void {
    this.searchQuery.set('');
    this.selectedStatus.set('all');
    this.selectedCategory.set('all');
    this.selectedDifficulty.set('all');
    this.sortBy.set('newest');
  }
}
