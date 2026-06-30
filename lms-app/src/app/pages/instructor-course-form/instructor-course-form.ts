import { Component, OnInit, signal, inject, DestroyRef, ViewChild } from '@angular/core';
import { marked } from 'marked';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '@services/auth.service';
import { ToastService } from '@services/toast.service';
import { CategoryResponse } from '@models/course';
import { untilDestroyed } from '../../rxjs/until-destroyed';
import { Dropdown } from '@components/dropdown/dropdown';
import { FormInput } from '@components/form-input/form-input';
import { Button } from '@components/button/button';
import { Loader } from '@components/loader/loader';
import { ConfirmModal } from '@components/confirm-modal/confirm-modal';
import { CourseService } from '@services/course.service';

interface CourseFormData {
  title: string;
  description: string;
  categoryId: string;
  price: string;
  isPremium: boolean;
  requirements: string;
  learningOutcomes: string;
  level: string;
  languageId: string;
  courseAccessType: string;
  defaultDeadlineDays: string;
}

interface FormErrors {
  title?: string;
  description?: string;
  categoryId?: string;
  price?: string;
  level?: string;
  languageId?: string;
  courseAccessType?: string;
  thumbnail?: string;
}

@Component({
  selector: 'app-instructor-course-form',
  standalone: true,
  imports: [CommonModule, FormsModule, Dropdown, FormInput, Button, Loader, ConfirmModal],
  templateUrl: './instructor-course-form.html'
})
export class InstructorCourseForm implements OnInit {
  private courseService = inject(CourseService);
  private toastService = inject(ToastService);
  private router = inject(Router);
  private authService = inject(AuthService);
  private destroyRef = inject(DestroyRef);

  protected get routePrefix(): string {
    return this.authService.userRole()?.toLowerCase() || 'instructor';
  }

  protected isMetaLoading = signal(true);
  protected isSubmitting = signal(false);

  protected categories = signal<CategoryResponse[]>([]);

  protected form: CourseFormData = {
    title: '',
    description: '',
    categoryId: '',
    price: '',
    isPremium: false,
    requirements: '',
    learningOutcomes: '',
    level: '',
    languageId: '',
    courseAccessType: '',
    defaultDeadlineDays: '30'
  };

  private initialFormState = JSON.stringify(this.form);
  private isSubmitted = false;
  
  protected showUnsavedModal = signal(false);
  private unsavedResolve: ((val: boolean) => void) | null = null;

  protected errors: FormErrors = {};

  // File state
  protected thumbnailFile: File | null = null;
  protected thumbnailPreview: string | null = null;
  protected introVideoFile: File | null = null;
  protected introVideoName: string | null = null;

  // Dropdown options
  protected levelOptions = [
    { value: '1', label: 'Beginner' },
    { value: '2', label: 'Intermediate' },
    { value: '3', label: 'Advanced' }
  ];

  protected accessTypeOptions = [
    { value: '1', label: 'Self Paced' },
    { value: '2', label: 'Cohort Based' }
  ];

  protected languageOptions = signal<{ value: string; label: string }[]>([]);
  protected categoryOptions = signal<{ value: string; label: string }[]>([]);

  protected activeTab: { [key: string]: 'write' | 'preview' } = {
    description: 'write',
    requirements: 'write',
    outcomes: 'write'
  };

  get descriptionPreview(): string {
    return marked.parse(this.form.description || '') as string;
  }

  get requirementsPreview(): string {
    return marked.parse(this.form.requirements || '') as string;
  }

  get outcomesPreview(): string {
    return marked.parse(this.form.learningOutcomes || '') as string;
  }

  ngOnInit(): void {
    this.loadMetadata();
  }

  private loadMetadata(): void {
    this.isMetaLoading.set(true);
    this.courseService.getFiltersMetadata()
      .pipe(untilDestroyed(this.destroyRef))
      .subscribe({
        next: (data) => {
          this.categories.set(data.categories || []);
          this.categoryOptions.set(
            (data.categories || []).map(c => ({ value: String(c.id), label: c.name }))
          );
          this.languageOptions.set(
            (data.languages || []).map(l => ({ value: String(l.id), label: l.name }))
          );
          this.isMetaLoading.set(false);
        },
        error: (err) => {
          this.toastService.showApiError(err, 'Failed to load form metadata.');
          this.isMetaLoading.set(false);
        }
      });
  }

  // ── File Handlers ─────────────────────────────────────────────────────────

  protected onThumbnailSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;

    const allowed = ['image/jpeg', 'image/png', 'image/webp'];
    if (!allowed.includes(file.type)) {
      this.errors.thumbnail = 'Only JPG, PNG, or WebP images are allowed.';
      return;
    }
    if (file.size > 5 * 1024 * 1024) {
      this.errors.thumbnail = 'Thumbnail must be under 5 MB.';
      return;
    }

    this.thumbnailFile = file;
    this.errors.thumbnail = undefined;

    const reader = new FileReader();
    reader.onload = (e) => {
      this.thumbnailPreview = e.target?.result as string;
    };
    reader.readAsDataURL(file);
  }

  protected removeThumbnail(): void {
    this.thumbnailFile = null;
    this.thumbnailPreview = null;
  }

  protected onVideoSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;

    const allowed = ['video/mp4', 'video/webm', 'video/ogg'];
    if (!allowed.includes(file.type)) {
      this.toastService.showError('Only MP4, WebM, or OGG videos are allowed.');
      return;
    }
    if (file.size > 500 * 1024 * 1024) {
      this.toastService.showError('Intro video must be under 500 MB.');
      return;
    }

    this.introVideoFile = file;
    this.introVideoName = file.name;
  }

  protected removeVideo(): void {
    this.introVideoFile = null;
    this.introVideoName = null;
  }

  // ── Validation ────────────────────────────────────────────────────────────

  private validate(): boolean {
    this.errors = {};
    let valid = true;

    if (!this.form.title.trim()) {
      this.errors.title = 'Title is required.';
      valid = false;
    } else if (this.form.title.trim().length > 300) {
      this.errors.title = 'Title must not exceed 300 characters.';
      valid = false;
    }

    if (!this.form.description.trim()) {
      this.errors.description = 'Description is required.';
      valid = false;
    }else if(this.form.description.trim().length>500){
        this.errors.description='Description must not exeed 500 charactera.';
        valid=false;
    }

    if (!this.form.categoryId) {
      this.errors.categoryId = 'Please select a category.';
      valid = false;
    }

    if (this.form.isPremium) {
      const price = parseFloat(this.form.price);
      if (isNaN(price) || price <= 0) {
        this.errors.price = 'Price must be greater than 0 for premium courses.';
        valid = false;
      }
    }

    if (!this.form.level) {
      this.errors.level = 'Please select a level.';
      valid = false;
    }

    if (!this.form.languageId) {
      this.errors.languageId = 'Please select a language.';
      valid = false;
    }

    if (!this.form.courseAccessType) {
      this.errors.courseAccessType = 'Please select an access type.';
      valid = false;
    }

    return valid;
  }

  // ── Submit ────────────────────────────────────────────────────────────────

  protected async onSubmit(): Promise<void> {
    if (!this.validate()) {
      this.toastService.showError('Please fix the errors before submitting.');
      return;
    }

    this.isSubmitting.set(true);
    const formData = new FormData();
    formData.append('Title', this.form.title.trim());
    formData.append('Description', this.form.description.trim());
    formData.append('CategoryId', this.form.categoryId);
    formData.append('IsPremium', String(this.form.isPremium));

    formData.append('Level', this.form.level);
    formData.append('LanguageId', this.form.languageId);
    formData.append('CourseAccessType', this.form.courseAccessType);

    if (this.form.isPremium && this.form.price) {
      formData.append('Price', this.form.price);
    }
    if (this.form.requirements.trim()) {
      formData.append('Requirements', this.form.requirements.trim());
    }
    if (this.form.learningOutcomes.trim()) {
      formData.append('LearningOutcomes', this.form.learningOutcomes.trim());
    }
    if (this.form.courseAccessType === '1' && this.form.defaultDeadlineDays) {
      formData.append('DefaultDeadlineDays', this.form.defaultDeadlineDays);
    }
    if (this.thumbnailFile) {
      formData.append('Thumbnail', this.thumbnailFile);
    }
    if (this.introVideoFile) {
      formData.append('IntroVideo', this.introVideoFile);
    }

    this.courseService.createCourse(formData)
      .pipe(untilDestroyed(this.destroyRef))
      .subscribe({
        next: (course) => {
          this.toastService.showSuccess('Course created successfully!');
          this.isSubmitted = true;
          this.router.navigate([`/${this.routePrefix}/courses`, course.slug, 'builder']);
        },
        error: (err) => {
          this.toastService.showApiError(err, 'Failed to create course.');
          this.isSubmitting.set(false);
        }
      });
  }

  protected onCancel(): void {
    this.router.navigate([`/${this.routePrefix}/courses`]);
  }

  async canDeactivate(): Promise<boolean> {
    if (this.isSubmitted) return true;

    const hasChanges = JSON.stringify(this.form) !== this.initialFormState || 
                       this.thumbnailFile !== null || 
                       this.introVideoFile !== null;

    if (!hasChanges) return true;

    return new Promise<boolean>((resolve) => {
      this.unsavedResolve = resolve;
      this.showUnsavedModal.set(true);
    });
  }

  protected confirmLeave(): void {
    this.showUnsavedModal.set(false);
    if (this.unsavedResolve) {
      this.unsavedResolve(true);
      this.unsavedResolve = null;
    }
  }

  protected cancelLeave(): void {
    this.showUnsavedModal.set(false);
    if (this.unsavedResolve) {
      this.unsavedResolve(false);
      this.unsavedResolve = null;
    }
  }

  protected get showDeadlineDays(): boolean {
    return this.form.courseAccessType === '1';
  }
}
