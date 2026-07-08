import { Component, inject, OnInit, effect, signal, NgZone } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import { InstructorCourseLayout } from '../instructor-course-layout/instructor-course-layout';
import { ToastService } from '@services/toast.service';
import { Button } from '@components/button/button';
import { FormInput } from '@components/form-input/form-input';
import { Dropdown } from '@components/dropdown/dropdown';
import { marked } from 'marked';
import { CourseService } from '@services/course.service';
import { ConfirmModal } from '@components/confirm-modal/confirm-modal';

@Component({
  selector: 'app-instructor-course-overview',
  standalone: true,
  imports: [CommonModule, FormsModule, Button, FormInput, Dropdown,ConfirmModal],
  templateUrl: './instructor-course-overview.html'
})
export class InstructorCourseOverview implements OnInit {
  protected layout = inject(InstructorCourseLayout);
  private courseService = inject(CourseService);
  private toastService = inject(ToastService);
  private sanitizer = inject(DomSanitizer);
  private ngZone = inject(NgZone);

  protected isEditMode = false;
  protected isSaving = false;

  // Metadata
  protected categories: any[] = [];
  protected languages: any[] = [];
  
  protected categoryOptions: any[] = [];
  protected languageOptions: any[] = [];
  protected levelOptions = [
    { label: 'Beginner', value: '1' },
    { label: 'Intermediate', value: '2' },
    { label: 'Advanced', value: '3' },
    { label: 'All Levels', value: '0' }
  ];

  // Form Fields
  protected title = '';
  protected categoryIdStr = '';
  protected languageIdStr = '';
  protected levelStr = '';
  protected isPremium = false;
  protected priceStr = '0';

  protected thumbnailFile: File | null = null;
  protected thumbnailPreview: string | null = null;
  protected description = '';
  protected requirements = '';
  protected learningOutcomes = '';

  private initialFormState:string | null=null;
  private isSubmitted=false;
  protected showUnsavedModal=signal(false);
  protected errors: any = {};
  protected thumbnailLimitMB = signal(5);
  private pendingNavigationResolve?: (value: boolean) => void;

  // Parsed Markdown
  protected parsedDescription: SafeHtml = '';
  protected parsedRequirements: SafeHtml = '';
  protected parsedOutcomes: SafeHtml = '';

  constructor() {
    effect(() => {
      const course = this.layout.course();
      if (course && !this.isEditMode) {
        this.parseMarkdownForView(course);
      }
    });
  }

  private captureInitialState(): void {
  this.initialFormState = JSON.stringify({
    title: this.title,
    categoryIdStr: this.categoryIdStr,
    languageIdStr: this.languageIdStr,
    levelStr: this.levelStr,
    isPremium: this.isPremium,
    priceStr: this.priceStr,
    description: this.description,
    requirements: this.requirements,
    learningOutcomes: this.learningOutcomes,
  });
}

  ngOnInit() {
    this.courseService.getUploadLimits().subscribe({
      next: (limits) => {
        this.thumbnailLimitMB.set(limits.thumbnailSizeMB);
      },
      error: (err) => console.error('Failed to load upload limits', err)
    });

    this.courseService.getFiltersMetadata().subscribe({
      next: (data) => {
        this.categories = data.categories || [];
        this.languages = data.languages || [];
        this.categoryOptions = this.categories.map(c => ({ label: c.name, value: c.id.toString() }));
        this.languageOptions = this.languages.map(l => ({ label: l.name, value: l.id.toString() }));

        this.captureInitialState();
      },
      error: (err) => console.error('Failed to load metadata', err)
    });
  }

  protected get course() {
    return this.layout.course();
  }

protected hasUnsavedChanges(): boolean {
  // thumbnailFile is checked separately because it's set synchronously
  // and its change is reliably detectable without relying on the async FileReader result
  if (this.thumbnailFile !== null) return true;

  const currentState = JSON.stringify({
    title: this.title,
    categoryIdStr: this.categoryIdStr,
    languageIdStr: this.languageIdStr,
    levelStr: this.levelStr,
    isPremium: this.isPremium,
    priceStr: this.priceStr,
    description: this.description,
    requirements: this.requirements,
    learningOutcomes: this.learningOutcomes,
  });
  return currentState !== this.initialFormState;
}

protected toggleEditMode() {
  const course = this.course;

  if (!this.isEditMode && course) {
    this.title = course.title || '';
    this.description = course.description || '';
    this.requirements = course.requirements || '';
    this.learningOutcomes = course.learningOutcomes || '';

    this.categoryIdStr = course.categoryId?.toString() || '';
    this.languageIdStr = course.languageId?.toString() || '';
    this.levelStr = course.level?.toString() || '';
    this.isPremium = course.isPremium || false;
    this.priceStr = course.price?.toString() || '0';

    this.thumbnailFile = null;
    this.thumbnailPreview = course.thumbnailUrl || null;

    this.updateMarkdownPreview();

    this.captureInitialState();
  } else {
    this.isSubmitted = false;
  }

  this.isEditMode = !this.isEditMode;
}

  protected validateField(field: string): void {
    if (this.errors[field]) {
      this.errors[field] = undefined;
    }
  }

  protected validate(): boolean {
    this.errors = {};
    let valid = true;

    if (!this.title?.trim()) {
      this.errors.title = 'Title is required.';
      valid = false;
    } else if (this.title.trim().length > 300) {
      this.errors.title = 'Title must not exceed 300 characters.';
      valid = false;
    }

    if (!this.description?.trim()) {
      this.errors.description = 'Description is required.';
      valid = false;
    } else if (this.description.trim().length > 500) {
      this.errors.description = 'Description must not exceed 500 characters.';
      valid = false;
    }

    if (this.requirements?.length > 1000) {
      this.errors.requirements = 'Requirements must not exceed 1000 characters.';
      valid = false;
    }

    if (this.learningOutcomes?.length > 1000) {
      this.errors.learningOutcomes = 'Learning outcomes must not exceed 1000 characters.';
      valid = false;
    }

    if (!this.categoryIdStr) {
      this.errors.categoryId = 'Please select a category.';
      valid = false;
    }

    if (this.isPremium) {
      const price = parseFloat(this.priceStr);
      if (isNaN(price) || price <= 0) {
        this.errors.price = 'Price must be greater than 0 for premium courses.';
        valid = false;
      }
    }

    return valid;
  }

  protected cancelEdit() {
    this.isEditMode = false;
    this.thumbnailFile = null;
    this.thumbnailPreview = null;
    if (this.course) {
      this.parseMarkdownForView(this.course);
    }
  }

  protected onThumbnailSelected(event: any) {
    const file = event.target.files?.[0];
    if (file) {
      const allowed = ['image/jpeg', 'image/png', 'image/webp'];
      if (!allowed.includes(file.type)) {
        this.errors.thumbnail = 'Only JPG, PNG, or WebP images are allowed.';
        this.toastService.showError(this.errors.thumbnail);
        return;
      }
      if (file.size > this.thumbnailLimitMB() * 1024 * 1024) {
        this.errors.thumbnail = `Thumbnail must be under ${this.thumbnailLimitMB()} MB.`;
        this.toastService.showError(this.errors.thumbnail);
        return;
      }
      this.errors.thumbnail = undefined;

      this.thumbnailFile = file;
      const reader = new FileReader();
      reader.onload = (e: any) => {
        // Run inside NgZone so Angular detects the change and re-evaluates [disabled] on the Save button
        this.ngZone.run(() => {
          this.thumbnailPreview = e.target.result;
        });
      };
      reader.readAsDataURL(file);
    }
  }

  protected updateMarkdownPreview() {
    this.parsedDescription = this.sanitizeAndParse(this.description);
    this.parsedRequirements = this.sanitizeAndParse(this.requirements);
    this.parsedOutcomes = this.sanitizeAndParse(this.learningOutcomes);
  }

  private parseMarkdownForView(course: any) {
    this.parsedDescription = this.sanitizeAndParse(course.description || 'No description has been added for this course yet.');
    this.parsedRequirements = this.sanitizeAndParse(course.requirements || '');
    this.parsedOutcomes = this.sanitizeAndParse(course.learningOutcomes || '');
  }

  private sanitizeAndParse(rawText: string): SafeHtml {
    if (!rawText) return '';
    const rawHtml = marked.parse(rawText) as string;
    return this.sanitizer.bypassSecurityTrustHtml(rawHtml);
  }

  protected saveChanges() {
    if (!this.course) return;

    if (!this.validate()) {
      this.toastService.showError('Please fix the errors before saving.');
      return;
    }

    this.isSaving = true;

    const formData = new FormData();
    formData.append('Title', this.title);
    formData.append('CategoryId', this.categoryIdStr);
    formData.append('Description', this.description);
    formData.append('Requirements', this.requirements);
    formData.append('LearningOutcomes', this.learningOutcomes);
    formData.append('Price', this.priceStr);
    formData.append('IsPremium', (this.isPremium ? 'true' : 'false'));

    if (this.languageIdStr) formData.append('LanguageId', this.languageIdStr);
    if (this.levelStr) formData.append('Level', this.levelStr);
    if (this.thumbnailFile) formData.append('Thumbnail', this.thumbnailFile);

    this.courseService.updateCourse(this.course.id, formData).subscribe({
      next: () => {
        this.toastService.showSuccess('Course overview updated successfully.');
        this.layout.loadCourse(this.course!.id);
        this.isEditMode = false;
        this.isSaving = false;
        this.isSubmitted=true;
      },
      error: (err) => {
        this.toastService.showApiError(err, 'Failed to update course overview.');
        this.isSaving = false;
      }
    });
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

  async canDeactivate(): Promise<boolean> {
  if (!this.isEditMode || this.isSubmitted) {
    return Promise.resolve(true);
  }

  if (!this.hasUnsavedChanges()) {
    return Promise.resolve(true);
  }

  this.showUnsavedModal.set(true);

  return new Promise<boolean>((resolve) => {
    this.pendingNavigationResolve = resolve;
  });
}

protected confirmLeave(): void {
  this.showUnsavedModal.set(false);
  this.pendingNavigationResolve?.(true);
  this.pendingNavigationResolve = undefined;
}

protected cancelLeave(): void {
  this.showUnsavedModal.set(false);
  this.pendingNavigationResolve?.(false);
  this.pendingNavigationResolve = undefined;
}
}
