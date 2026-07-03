import { Component, OnInit, signal, computed, inject, DestroyRef, HostListener, effect, untracked } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, Router } from '@angular/router';
import { ToastService } from '@services/toast.service';
import { CourseResponse, CategoryResponse, InstructorCourseCardResponse } from '@models/course';
import { CourseLevel } from '../../enums/course-level.enum';
import { PublishStatus } from '../../enums/publish-status.enum';
import { CourseStatus } from '../../enums/course-status.enum';
import { untilDestroyed } from '../../rxjs/until-destroyed';
import { Dropdown } from '@components/dropdown/dropdown';
import { FormInput } from '@components/form-input/form-input';
import { ConfirmModal } from '@components/confirm-modal/confirm-modal';
import { Button } from '@components/button/button';
import { CourseService } from '@services/course.service';

@Component({
  selector: 'app-instructor-courses',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, Dropdown, FormInput, ConfirmModal, Button],
  templateUrl: './instructor-courses.html'
})
export class InstructorCourses implements OnInit {
  private courseService = inject(CourseService);
  private toastService = inject(ToastService);
  private router = inject(Router);
  private destroyRef = inject(DestroyRef);

  // States
  protected courses = signal<InstructorCourseCardResponse[]>([]);
  protected categories = signal<CategoryResponse[]>([]);
  protected isLoading = signal(true);
  
  // Pagination States
  protected pageNumber = signal(1);
  protected pageSize = signal(6);
  protected totalCount = signal(0);
  protected totalPages = signal(0);
  
  protected allCoursesForStats = signal<InstructorCourseCardResponse[]>([]);
  
  // Confirmation Modal state
  protected showArchiveModal = false;
  protected showPublishModal=false;
  protected courseToArchive: number | null = null;
  protected courseToPublish:InstructorCourseCardResponse|null=null;
  protected isPublishing=false;
  
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

  // Stats computed from metadata payload
  protected totalCourses = computed(() => this.allCoursesForStats().length);
  protected publishedCourses = computed(() => {
    return this.allCoursesForStats().filter(c => {
      const status = String(c.status).toLowerCase();
      return status === String(PublishStatus.Published) || status === 'published';
    }).length;
  });
  protected draftCourses = computed(() => {
    return this.allCoursesForStats().filter(c => {
      const status = String(c.status).toLowerCase();
      return status === String(PublishStatus.Draft) || status === 'draft';
    }).length;
  });
  protected totalLearners = computed(() => {
    return this.allCoursesForStats().reduce((sum, c) => sum + (c.enrolledCount || 0), 0);
  });

  // Server-side filtered courses mapping
  protected filteredCourses = computed(() => this.courses());

  // Check if any filter is active
  protected hasActiveFilters = computed(() => {
    return this.searchQuery() !== '' ||
      this.selectedStatus() !== 'all' ||
      this.selectedCategory() !== 'all' ||
      this.selectedDifficulty() !== 'all';
  });

  constructor() {
    effect(() => {
      // Whenever filters change, reset pageNumber to 1
      this.searchQuery();
      this.selectedStatus();
      this.selectedCategory();
      this.selectedDifficulty();
      this.sortBy();

      untracked(() => {
        this.pageNumber.set(1);
      });
    });

    effect(() => {
      // Trigger API fetch on filter/page changes
      this.searchQuery();
      this.selectedStatus();
      this.selectedCategory();
      this.selectedDifficulty();
      this.sortBy();
      this.pageNumber();
      this.pageSize();

      untracked(() => {
        this.loadPagedCourses();
      });
    });
  }

  ngOnInit(): void {
    // Load all courses (no pagination) once for the stats cards
    this.courseService.getMyCourses()
      .pipe(untilDestroyed(this.destroyRef))
      .subscribe({
        next: (data) => {
          this.allCoursesForStats.set(data.courses || []);
          this.loadCategories();
        },
        error: (err) => {
          this.toastService.showApiError(err, 'Failed to load courses metadata.');
        }
      });
  }

  private loadPagedCourses(): void {
    this.isLoading.set(true);
    
    // Status mapping:
    let statuses: string | undefined = undefined;
    const statusVal = this.selectedStatus();
    if (statusVal === 'draft') statuses = String(PublishStatus.Draft);
    else if (statusVal === 'published') statuses = String(PublishStatus.Published);
    else if (statusVal === 'archived') statuses = String(CourseStatus.Archived);
    else if (statusVal === 'pending') statuses = String(CourseStatus.PendingApproval);

    // Difficulty level mapping:
    let levels: string | undefined = undefined;
    const diff = this.selectedDifficulty();
    if (diff === 'beginner') levels = String(CourseLevel.Beginner);
    else if (diff === 'intermediate') levels = String(CourseLevel.Intermediate);
    else if (diff === 'advanced') levels = String(CourseLevel.Advanced);

    const categoryIds = this.selectedCategory() !== 'all' ? this.selectedCategory() : undefined;

    const query = {
      categoryIds,
      levels,
      statuses,
      search: this.searchQuery().trim() || undefined,
      sortBy: this.sortBy(),
      pageNumber: this.pageNumber(),
      pageSize: this.pageSize()
    };

    this.courseService.getMyCourses(query)
      .pipe(untilDestroyed(this.destroyRef))
      .subscribe({
        next: (data) => {
          this.courses.set(data.courses || []);
          this.totalCount.set(data.totalCount || 0);
          this.totalPages.set(data.totalPages || 0);
          this.isLoading.set(false);
        },
        error: (err) => {
          this.toastService.showApiError(err, 'Failed to load courses.');
          this.isLoading.set(false);
        }
      });
  }

  private loadCategories(): void {
    this.courseService.getAllCategories()
      .pipe(untilDestroyed(this.destroyRef))
      .subscribe({
        next: (data) => {
          this.categories.set(data || []);
        },
        error: (err) => {
          this.toastService.showApiError(err, 'Failed to load categories.');
        }
      });
  }

  // Helpers for Status Mapping
  protected getStatusString(status: number | string): string {
    const statusNum = typeof status === 'string' ? parseInt(status, 10) : status;
    switch (statusNum) {
      case PublishStatus.Draft: return 'Draft';
      case PublishStatus.Published: return 'Published';
      case CourseStatus.Archived: return 'Archived';
      case CourseStatus.PendingApproval: return 'Pending';
      case CourseStatus.Rejected: return 'Rejected';
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
    if (lvl === String(CourseLevel.Beginner) || lvl === 'beginner') return 'Beginner';
    if (lvl === String(CourseLevel.Intermediate) || lvl === 'intermediate') return 'Intermediate';
    if (lvl === String(CourseLevel.Advanced) || lvl === 'advanced') return 'Advanced';
    return 'All Levels';
  }

  protected getLevelBadgeClass(level: number | string): string {
    const lvl = String(level).trim().toLowerCase();
    if (lvl === String(CourseLevel.Beginner) || lvl === 'beginner') return 'bg-slate-100 text-slate-700';
    if (lvl === String(CourseLevel.Intermediate) || lvl === 'intermediate') return 'bg-indigo-50 text-indigo-700';
    if (lvl === String(CourseLevel.Advanced) || lvl === 'advanced') return 'bg-rose-50 text-rose-700';
    return 'bg-gray-100 text-gray-600';
  }

  protected isCoursePublished(course: InstructorCourseCardResponse): boolean {
    const status = String(course.status).toLowerCase();
    return status === String(PublishStatus.Published) || status === 'published';
  }

  protected isCoursePendingApproval(course: InstructorCourseCardResponse): boolean {
    const status = String(course.status).toLowerCase();
    return status === String(CourseStatus.PendingApproval) || status === 'pending' || status === 'pending approval';
  }

  protected canUnpublish(course: InstructorCourseCardResponse): boolean {
    return this.isCoursePublished(course) || this.isCoursePendingApproval(course);
  }

  protected isCourseArchived(course: InstructorCourseCardResponse): boolean {
    const status = String(course.status).toLowerCase();
    return status === String(CourseStatus.Archived) || status === 'archived';
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
    this.router.navigate(['/instructor/courses/new']);
  }

  protected onImportCourse(): void {
    this.toastService.showInfo('Course import wizard under development.');
  }

  protected editCourse(slug: string): void {
    this.router.navigate([`/instructor/courses/${slug}/overview`]);
  }

  protected previewCourse(slug: string, courseId: number): void {
    // Pass courseId via router state as CourseDetail relies on it if loaded directly from here
    this.router.navigate([`/instructor/courses/preview/${slug}`], { state: { courseId } });
  }

  protected duplicateCourse(courseId: number): void {
    this.toastService.showSuccess('Course duplicated successfully (Mocked).');
  }

  protected togglePublishStatus(course: InstructorCourseCardResponse|null): void {
    if(course==null)
      return
    const shouldUnpublish = this.canUnpublish(course);
    const nextPublishState = !shouldUnpublish;
    
    this.courseService.publishCourse(course.id, nextPublishState)
      .pipe(untilDestroyed(this.destroyRef))
      .subscribe({
        next: (updatedCourse) => {
          this.toastService.showSuccess(
            nextPublishState ? 'Course submitted for approval successfully!' : 'Course unpublished successfully!'
          );
          // Update local signals
          this.courses.update(list => list.map(c => c.id === course.id ? { ...c, status: updatedCourse.status } : c));
          this.allCoursesForStats.update(list => list.map(c => c.id === course.id ? { ...c, status: updatedCourse.status } : c));
          this.showPublishModal=false;
        },
        error: (err) => {
          this.showPublishModal=false;
          this.toastService.showApiError(err, 'Failed to update course publish status.');
        }
      });
  }

  protected isArchivingAction = true;

  protected confirmArchiveCourse(courseId: number, archive: boolean): void {
    this.courseToArchive = courseId;
    this.isArchivingAction = archive;
    this.showArchiveModal = true;
  }

  protected ConfirmTogglePublishStatus(course:InstructorCourseCardResponse,shouldUnpublish:boolean):void{
    this.courseToPublish=course;
    this.isPublishing=shouldUnpublish;
    this.showPublishModal=true;
  }

  protected archiveCourse(): void {
    if (this.courseToArchive === null) return;
    this.courseService.archiveCourse(this.courseToArchive, this.isArchivingAction)
      .pipe(untilDestroyed(this.destroyRef))
      .subscribe({
        next: (updatedCourse) => {
          this.toastService.showSuccess(this.isArchivingAction ? 'Course archived successfully.' : 'Course unarchived successfully.');
          this.courses.update(list => list.map(c => c.id === this.courseToArchive ? { ...c, status: this.isArchivingAction ? 3 : 1 } : c));
          this.allCoursesForStats.update(list => list.map(c => c.id === this.courseToArchive ? { ...c, status: this.isArchivingAction ? 3 : 1 } : c));
          this.closeArchiveModal();
        },
        error: (err) => {
          this.toastService.showApiError(err, this.isArchivingAction ? 'Failed to archive course.' : 'Failed to unarchive course.');
          this.closeArchiveModal();
        }
      });
  }

  protected closeArchiveModal(): void {
    this.showArchiveModal = false;
    this.courseToArchive = null;
  }

  protected closePublishModal():void{
    this.showPublishModal=false;
    this.courseToPublish=null;
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

  protected Math = Math;

  protected get pagesArray(): number[] {
    return Array.from({ length: this.totalPages() }, (_, i) => i + 1);
  }
}
