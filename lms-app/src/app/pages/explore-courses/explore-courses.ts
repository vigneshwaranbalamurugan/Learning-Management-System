import { Component, OnInit, signal, inject, DestroyRef, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { DashboardService } from '@services/dashboard.service';
import {
  CourseResponse,
  CategoryResponse,
  InstructorMetadata,
  LanguageMetadata,
  FiltersMetadataResponse
} from '@models/dashboard';
import { ToastService } from '@services/toast.service';
import { untilDestroyed } from '../../rxjs/until-destroyed';
import { PaginationComponent } from '../../components/pagination/pagination.component';
import { forkJoin, Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';

import { Loader } from '@components/loader/loader';

@Component({
  selector: 'app-explore-courses',
  standalone: true,
  imports: [CommonModule, FormsModule, PaginationComponent, Loader],
  templateUrl: './explore-courses.html'
})
export class ExploreCourses implements OnInit {
  private dashboardService = inject(DashboardService);
  private toastService     = inject(ToastService);
  private router           = inject(Router);
  private route            = inject(ActivatedRoute);
  private destroyRef       = inject(DestroyRef);

  // ── Data signals ─────────────────────────────────────────────────────────
  protected courses          = signal<CourseResponse[]>([]);
  protected availableCourses = computed(() => {
    const enrolled = this.enrolledCourseIds();
    let list = this.courses().filter(c => !enrolled.includes(c.id));

    // Client-side duration filter fallback
    const durations = this.selectedDurations();
    if (durations.length > 0) {
      list = list.filter(course => {
        if (!course.estimatedDuration) return false;
        const parts = course.estimatedDuration.split(':');
        const hours = parseInt(parts[0] ?? '0', 10) + (parseInt(parts[1] ?? '0', 10) / 60);

        return (
          (durations.includes('lt1') && hours < 1) ||
          (durations.includes('1to5') && hours >= 1 && hours <= 5) ||
          (durations.includes('5to10') && hours >= 5 && hours <= 10) ||
          (durations.includes('10to20') && hours >= 10 && hours <= 20) ||
          (durations.includes('gt20') && hours > 20)
        );
      });
    }
    return list;
  });
  protected categories       = signal<CategoryResponse[]>([]);
  protected languages        = signal<LanguageMetadata[]>([]);
  protected instructors      = signal<InstructorMetadata[]>([]);
  protected enrolledCourseIds = signal<number[]>([]);
  protected enrollmentProgress = signal<{ [courseId: number]: number }>({});

  // ── UI state ─────────────────────────────────────────────────────────────
  protected isLoading       = signal(true);
  protected filtersLoading  = signal(true);
  protected isEnrolling     = signal<number | null>(null);
  protected mobileFiltersOpen = signal(false);
  protected desktopFiltersOpen = signal(true);
  protected showMoreCategories = signal(false);
  protected showMoreInstructors = signal(false);
  protected searchInput     = '';
  private searchSubject     = new Subject<string>();
  protected collapsedSections = signal<Record<string, boolean>>({});
  protected sortDropdownOpen = signal(false);

  // ── Pagination state ─────────────────────────────────────────────────────
  protected pageNumber = signal(1);
  protected pageSize   = signal(10);
  protected totalPages = signal(1);
  protected totalCount = signal(0);

  // ── Filter state ─────────────────────────────────────────────────────────
  protected selectedCategoryIds  = signal<number[]>([]);
  protected selectedLevels       = signal<number[]>([]);
  protected selectedLanguageIds  = signal<number[]>([]);
  protected selectedIsPremium    = signal<boolean | null>(null);
  protected selectedMinRating    = signal<number | null>(null);
  protected selectedDurations    = signal<string[]>([]);
  protected selectedInstructorIds = signal<number[]>([]);
  protected selectedAccessTypes  = signal<number[]>([]);
  protected selectedSortBy       = signal<string>('newest');
  protected searchQuery          = signal<string>('');

  // ── Active filter chips ─────────────────────────────────────────────────
  protected activeFilterChips = computed<Array<{ label: string; removeKey: string; removeValue: string }>>(() => {
    const chips: Array<{ label: string; removeKey: string; removeValue: string }> = [];

    this.selectedCategoryIds().forEach(id => {
      const cat = this.categories().find(c => c.id === id);
      if (cat) chips.push({ label: cat.name, removeKey: 'category', removeValue: String(id) });
    });

    this.selectedLevels().forEach(lv => {
      chips.push({ label: this.getLevelName(lv), removeKey: 'level', removeValue: String(lv) });
    });

    this.selectedLanguageIds().forEach(id => {
      const lang = this.languages().find(l => l.id === id);
      if (lang) chips.push({ label: lang.name, removeKey: 'language', removeValue: String(id) });
    });

    if (this.selectedIsPremium() === true)  chips.push({ label: 'Paid', removeKey: 'premium', removeValue: 'true' });
    if (this.selectedIsPremium() === false) chips.push({ label: 'Free', removeKey: 'premium', removeValue: 'false' });

    if (this.selectedMinRating() != null)
      chips.push({ label: `${this.selectedMinRating()}★ & Above`, removeKey: 'rating', removeValue: '' });

    this.selectedDurations().forEach(d =>
      chips.push({ label: this.getDurationLabel(d), removeKey: 'duration', removeValue: d }));

    this.selectedInstructorIds().forEach(id => {
      const inst = this.instructors().find(i => i.id === id);
      if (inst) chips.push({ label: inst.fullName, removeKey: 'instructor', removeValue: String(id) });
    });

    this.selectedAccessTypes().forEach(at =>
      chips.push({ label: at === 1 ? 'Self Paced' : 'Batch Course', removeKey: 'accessType', removeValue: String(at) }));

    return chips;
  });

  // ── Static filter options ─────────────────────────────────────────────────
  readonly LEVELS = [
    { value: 1, label: 'Beginner' },
    { value: 2, label: 'Intermediate' },
    { value: 3, label: 'Advanced' }
  ];

  readonly DURATIONS = [
    { key: 'lt1',    label: '< 1 Hour' },
    { key: '1to5',   label: '1 – 5 Hours' },
    { key: '5to10',  label: '5 – 10 Hours' },
    { key: '10to20', label: '10 – 20 Hours' },
    { key: 'gt20',   label: '20+ Hours' }
  ];

  readonly RATINGS = [
    { value: 4, label: '4★ & Above' },
    { value: 3, label: '3★ & Above' },
    { value: 2, label: '2★ & Above' }
  ];

  readonly ACCESS_TYPES = [
    { value: 1, label: 'Self Paced' },
    { value: 2, label: 'Batch Course' }
  ];

  readonly SORT_OPTIONS = [
    { value: 'newest',       label: 'Newest First' },
    { value: 'popular',      label: 'Most Popular' },
    { value: 'rating',       label: 'Highest Rated' },
    { value: 'oldest',       label: 'Oldest First' },
    { value: 'az',           label: 'A – Z' },
    { value: 'za',           label: 'Z – A' },
    { value: 'duration_asc', label: 'Duration: Low to High' },
    { value: 'duration_desc','label': 'Duration: High to Low' }
  ];

  ngOnInit(): void {
    // Read URL params on init
    this.route.queryParams.pipe(untilDestroyed(this.destroyRef)).subscribe(params => {
      if (params['search'])       this.searchQuery.set(params['search']);
      if (params['sortBy'])       this.selectedSortBy.set(params['sortBy']);
      if (params['pageNumber'])   this.pageNumber.set(Number(params['pageNumber']));
      if (params['categories'])   this.selectedCategoryIds.set(params['categories'].split(',').map(Number));
      if (params['levels'])       this.selectedLevels.set(params['levels'].split(',').map(Number));
      if (params['languages'])    this.selectedLanguageIds.set(params['languages'].split(',').map(Number));
      if (params['isPremium'] != null) {
        this.selectedIsPremium.set(params['isPremium'] === 'true' ? true : (params['isPremium'] === 'false' ? false : null));
      }
      if (params['minRating'])    this.selectedMinRating.set(Number(params['minRating']));
      if (params['durations'])    this.selectedDurations.set(params['durations'].split(','));
      if (params['instructors'])  this.selectedInstructorIds.set(params['instructors'].split(',').map(Number));
      if (params['accessTypes'])  this.selectedAccessTypes.set(params['accessTypes'].split(',').map(Number));
      this.searchInput = this.searchQuery();
    });

    // Setup debounced search
    this.searchSubject.pipe(debounceTime(400), distinctUntilChanged(), untilDestroyed(this.destroyRef))
      .subscribe(q => { this.searchQuery.set(q); this.pageNumber.set(1); this.loadCoursesAndUpdateUrl(); });

    // Load filter metadata + enrollments
    this.filtersLoading.set(true);
    forkJoin({
      metadata: this.dashboardService.getFiltersMetadata(),
      enrollments: this.dashboardService.getMyEnrollments()
    }).pipe(untilDestroyed(this.destroyRef)).subscribe({
      next: ({ metadata, enrollments }) => {
        this.categories.set(metadata.categories ?? []);
        this.languages.set(metadata.languages ?? []);
        this.instructors.set(metadata.instructors ?? []);
        this.enrolledCourseIds.set((enrollments ?? []).map(e => e.courseId));
        const progress: { [id: number]: number } = {};
        (enrollments ?? []).forEach(e => { progress[e.courseId] = e.progressPercentage; });
        this.enrollmentProgress.set(progress);
        this.filtersLoading.set(false);
        this.loadCoursesAndUpdateUrl();
      },
      error: () => {
        this.toastService.showError('Failed to load filters.');
        this.filtersLoading.set(false);
        this.loadCoursesAndUpdateUrl();
      }
    });
  }

  protected loadCoursesAndUpdateUrl(): void {
    this.isLoading.set(true);
    this.updateUrlParams();

    this.dashboardService.getAllCourses({
      categoryIds:      this.selectedCategoryIds().join(',') || undefined,
      levels:           this.selectedLevels().join(',') || undefined,
      languageIds:      this.selectedLanguageIds().join(',') || undefined,
      isPremium:        this.selectedIsPremium(),
      minRating:        this.selectedMinRating(),
      durations:        this.selectedDurations().join(',') || undefined,
      instructorIds:    this.selectedInstructorIds().join(',') || undefined,
      courseAccessTypes: this.selectedAccessTypes().join(',') || undefined,
      sortBy:           this.selectedSortBy(),
      search:           this.searchQuery() || undefined,
      pageNumber:       this.pageNumber(),
      pageSize:         this.pageSize(),
      excludeCourseIds: this.enrolledCourseIds().join(',') || undefined
    }).pipe(untilDestroyed(this.destroyRef)).subscribe({
      next: response => {
        this.courses.set(response?.courses ?? []);
        this.totalCount.set(response?.totalCount ?? 0);
        this.totalPages.set(response?.totalPages ?? 1);
        this.isLoading.set(false);
      },
      error: () => {
        this.toastService.showError('Failed to load courses.');
        this.isLoading.set(false);
      }
    });
  }

  private updateUrlParams(): void {
    const queryParams: Record<string, string | null> = {
      search:      this.searchQuery() || null,
      sortBy:      this.selectedSortBy() !== 'newest' ? this.selectedSortBy() : null,
      pageNumber:  this.pageNumber() > 1 ? String(this.pageNumber()) : null,
      categories:  this.selectedCategoryIds().length ? this.selectedCategoryIds().join(',') : null,
      levels:      this.selectedLevels().length ? this.selectedLevels().join(',') : null,
      languages:   this.selectedLanguageIds().length ? this.selectedLanguageIds().join(',') : null,
      isPremium:   this.selectedIsPremium() != null ? String(this.selectedIsPremium()) : null,
      minRating:   this.selectedMinRating() != null ? String(this.selectedMinRating()) : null,
      durations:   this.selectedDurations().length ? this.selectedDurations().join(',') : null,
      instructors: this.selectedInstructorIds().length ? this.selectedInstructorIds().join(',') : null,
      accessTypes: this.selectedAccessTypes().length ? this.selectedAccessTypes().join(',') : null
    };
    this.router.navigate([], { queryParams, replaceUrl: true });
  }

  // ── Filter toggles ────────────────────────────────────────────────────────
  protected toggleCategory(id: number): void {
    const cur = this.selectedCategoryIds();
    this.selectedCategoryIds.set(cur.includes(id) ? cur.filter(x => x !== id) : [...cur, id]);
    this.pageNumber.set(1);
    this.loadCoursesAndUpdateUrl();
  }

  protected toggleLevel(val: number): void {
    const cur = this.selectedLevels();
    this.selectedLevels.set(cur.includes(val) ? cur.filter(x => x !== val) : [...cur, val]);
    this.pageNumber.set(1);
    this.loadCoursesAndUpdateUrl();
  }

  protected toggleLanguage(id: number): void {
    const cur = this.selectedLanguageIds();
    this.selectedLanguageIds.set(cur.includes(id) ? cur.filter(x => x !== id) : [...cur, id]);
    this.pageNumber.set(1);
    this.loadCoursesAndUpdateUrl();
  }

  protected toggleDuration(key: string): void {
    const cur = this.selectedDurations();
    this.selectedDurations.set(cur.includes(key) ? cur.filter(x => x !== key) : [...cur, key]);
    this.pageNumber.set(1);
    this.loadCoursesAndUpdateUrl();
  }

  protected toggleInstructor(id: number): void {
    const cur = this.selectedInstructorIds();
    this.selectedInstructorIds.set(cur.includes(id) ? cur.filter(x => x !== id) : [...cur, id]);
    this.pageNumber.set(1);
    this.loadCoursesAndUpdateUrl();
  }

  protected toggleAccessType(val: number): void {
    const cur = this.selectedAccessTypes();
    this.selectedAccessTypes.set(cur.includes(val) ? cur.filter(x => x !== val) : [...cur, val]);
    this.pageNumber.set(1);
    this.loadCoursesAndUpdateUrl();
  }

  protected onPremiumChange(val: boolean | null): void {
    this.selectedIsPremium.set(this.selectedIsPremium() === val ? null : val);
    this.pageNumber.set(1);
    this.loadCoursesAndUpdateUrl();
  }

  protected onRatingChange(val: number): void {
    this.selectedMinRating.set(this.selectedMinRating() === val ? null : val);
    this.pageNumber.set(1);
    this.loadCoursesAndUpdateUrl();
  }

  protected onSortChange(val: string): void {
    this.selectedSortBy.set(val);
    this.pageNumber.set(1);
    this.loadCoursesAndUpdateUrl();
    this.sortDropdownOpen.set(false);
  }

  protected onSearchInput(val: string): void {
    this.searchInput = val;
    this.searchSubject.next(val);
  }

  protected onPageChange(page: number): void {
    this.pageNumber.set(page);
    this.loadCoursesAndUpdateUrl();
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  protected removeChip(chip: { removeKey: string; removeValue: string }): void {
    switch (chip.removeKey) {
      case 'category':    this.selectedCategoryIds.update(a => a.filter(x => x !== Number(chip.removeValue))); break;
      case 'level':       this.selectedLevels.update(a => a.filter(x => x !== Number(chip.removeValue))); break;
      case 'language':    this.selectedLanguageIds.update(a => a.filter(x => x !== Number(chip.removeValue))); break;
      case 'premium':     this.selectedIsPremium.set(null); break;
      case 'rating':      this.selectedMinRating.set(null); break;
      case 'duration':    this.selectedDurations.update(a => a.filter(x => x !== chip.removeValue)); break;
      case 'instructor':  this.selectedInstructorIds.update(a => a.filter(x => x !== Number(chip.removeValue))); break;
      case 'accessType':  this.selectedAccessTypes.update(a => a.filter(x => x !== Number(chip.removeValue))); break;
    }
    this.pageNumber.set(1);
    this.loadCoursesAndUpdateUrl();
  }

  protected clearAllFilters(): void {
    this.selectedCategoryIds.set([]);
    this.selectedLevels.set([]);
    this.selectedLanguageIds.set([]);
    this.selectedIsPremium.set(null);
    this.selectedMinRating.set(null);
    this.selectedDurations.set([]);
    this.selectedInstructorIds.set([]);
    this.selectedAccessTypes.set([]);
    this.searchQuery.set('');
    this.searchInput = '';
    this.selectedSortBy.set('newest');
    this.pageNumber.set(1);
    this.loadCoursesAndUpdateUrl();
  }

  // ── Helpers ───────────────────────────────────────────────────────────────
  protected getLevelName(level: number | string): string {
    switch (Number(level)) {
      case 1: return 'Beginner';
      case 2: return 'Intermediate';
      case 3: return 'Advanced';
      default: return 'All Levels';
    }
  }

  protected getLevelColor(level: number | string): string {
    switch (Number(level)) {
      case 1: return 'bg-emerald-50 text-emerald-700';
      case 2: return 'bg-amber-50 text-amber-700';
      case 3: return 'bg-red-50 text-red-700';
      default: return 'bg-gray-100 text-gray-600';
    }
  }

  protected getDurationLabel(key: string): string {
    return this.DURATIONS.find(d => d.key === key)?.label ?? key;
  }

  protected formatDuration(isoStr: string): string {
    if (!isoStr) return '';
    const parts = isoStr.split(':');
    const h = parseInt(parts[0] ?? '0', 10);
    const m = parseInt(parts[1] ?? '0', 10);
    if (h > 0 && m > 0) return `${h}h ${m}m`;
    if (h > 0) return `${h}h`;
    return `${m}m`;
  }

  protected starsArray(rating: number): boolean[] {
    return [1, 2, 3, 4, 5].map(s => s <= Math.round(rating));
  }

  protected isEnrolled(courseId: number): boolean {
    return this.enrolledCourseIds().includes(courseId);
  }

  protected getProgress(courseId: number): number {
    return this.enrollmentProgress()[courseId] ?? 0;
  }

  protected visibleCategories(): CategoryResponse[] {
    const cats = this.categories();
    return this.showMoreCategories() ? cats : cats.slice(0, 8);
  }

  protected visibleInstructors(): InstructorMetadata[] {
    const insts = this.instructors();
    return this.showMoreInstructors() ? insts : insts.slice(0, 6);
  }

  protected enroll(course: CourseResponse): void {
    if (this.isEnrolled(course.id)) {
      this.toastService.showInfo('You are already enrolled in this course.');
      return;
    }
    if (course.isPremium) {
      this.toastService.showWarning('Premium courses require payment. Please purchase this course.');
      return;
    }
    this.isEnrolling.set(course.id);
    this.dashboardService.enrollFreeCourse(course.id).pipe(untilDestroyed(this.destroyRef)).subscribe({
      next: () => {
        this.toastService.showSuccess(`Enrolled in "${course.title}"!`);
        this.enrolledCourseIds.update(ids => [...ids, course.id]);
        this.isEnrolling.set(null);
      },
      error: (err) => {
        this.toastService.showApiError(err, 'Enrollment failed.');
        this.isEnrolling.set(null);
      }
    });
  }

  protected navigateToCourse(course: CourseResponse): void {
    this.router.navigate(['/learner/explore', course.slug], {
      state: { courseId: course.id }
    });
  }

  protected hasActiveFilters(): boolean {
    return this.activeFilterChips().length > 0;
  }

  protected minOf(a: number, b: number): number {
    return Math.min(a, b);
  }

  protected toggleSection(key: string): void {
    this.collapsedSections.update(s => ({ ...s, [key]: !s[key] }));
  }

  protected isSectionCollapsed(key: string): boolean {
    return !!this.collapsedSections()[key];
  }

  protected toggleAllSections(collapse: boolean): void {
    const keys = ['category', 'level', 'language', 'price', 'rating', 'duration', 'accessType', 'instructor'];
    const updated: Record<string, boolean> = {};
    keys.forEach(k => {
      updated[k] = collapse;
    });
    this.collapsedSections.set(updated);
  }

  protected areAllSectionsCollapsed(): boolean {
    const keys = ['category', 'level', 'language', 'price', 'rating', 'duration', 'accessType', 'instructor'];
    return keys.every(k => this.isSectionCollapsed(k));
  }

  protected getSelectedSortLabel(): string {
    return this.SORT_OPTIONS.find(s => s.value === this.selectedSortBy())?.label ?? 'Sort';
  }
}
