import { Component, inject, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute, RouterModule } from '@angular/router';
import { AuthService } from '@services/auth.service';
import { ToastService } from '@services/toast.service';
import { FormInput } from '@components/form-input/form-input';
import { Dropdown } from '@components/dropdown/dropdown';
import { Button } from '@components/button/button';
import { Loader } from '@components/loader/loader';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import { marked } from 'marked';
import { InstructorCourseLayout } from '../instructor-course-layout/instructor-course-layout';
import { CourseBuilderService } from '@services/course-builder.service';
import { ConfirmModal } from '@components/confirm-modal/confirm-modal';
import { CourseAccessType } from '../../enums/course-access-type.enum';
import { PublishStatus } from '../../enums/publish-status.enum';
import { LessonType } from '../../enums/lesson-types.enum';

@Component({
  selector: 'app-instructor-lesson-form',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, FormInput, Dropdown, Button, Loader, ConfirmModal],
  templateUrl: './instructor-lesson-form.html'
})
export class InstructorLessonForm implements OnInit {
  private toastService = inject(ToastService);
  private authService = inject(AuthService);

  protected get routePrefix(): string {
    return this.authService.userRole()?.toLowerCase() || 'instructor';
  }
  private courseBuilderService = inject(CourseBuilderService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private sanitizer = inject(DomSanitizer);
  protected layout = inject(InstructorCourseLayout);

  protected courseSlug = '';
  protected sectionIdStr: string = '';
  protected lessonId: number | null = null;
  protected isEditMode = false;
  protected isLoading = signal(true);
  protected isSaving = signal(false);

  // Form State
  protected title = '';
  protected description = '';
  protected type: string = String(LessonType.Video);
  protected duration = 10;
  protected content = '';
  protected contentUrl = '';
  protected selectedFile: File | null = null;
  protected isPreview = false;
  protected status = String(PublishStatus.Draft);
  protected sortOrder = 0;

  // File Upload State
  protected isDragOver = false;
  protected uploadProgress = 0;
  protected isUploaded = false;
  protected maxVideoSizeMB = 500;
  protected maxPdfSizeMB = 50;

  // Original State to detect changes
  private originalTitle = '';
  private originalDescription = '';
  private originalType = String(LessonType.Video);
  private originalDuration = 10;
  private originalContent = '';
  private originalContentUrl = '';
  protected originalIsPreview = false;
  protected originalStatus: string = String(PublishStatus.Draft);
  protected originalSortOrder = 0;
  protected originalSectionIdStr: string = '';

  // MetadataPreview Parsed Markdown
  protected parsedMarkdown: SafeHtml = '';

  // History state placeholders (inspired by Notion/Linear)
  protected createdAt: string | null = null;
  protected updatedAt: string | null = null;

  protected lessonTypeOptions = [
    { value: String(LessonType.Video), label: 'Video Lecture' },
    { value: String(LessonType.Article), label: 'Article / Markdown' },
    { value: String(LessonType.Pdf), label: 'PDF Document' },
    { value: String(LessonType.ExternalLink), label: 'External Link' }
  ];

  protected statusOptions = [
    { value: String(PublishStatus.Draft), label: 'Draft' },
    { value: String(PublishStatus.Published), label: 'Published' }
  ];

  protected readonly PublishStatus = PublishStatus;
  protected readonly LessonType = LessonType;

  // String constants for template comparisons (Angular templates can't call global String())
  protected readonly LT_VIDEO = String(LessonType.Video);
  protected readonly LT_PDF = String(LessonType.Pdf);
  protected readonly LT_ARTICLE = String(LessonType.Article);
  protected readonly LT_EXTERNAL = String(LessonType.ExternalLink);
  protected readonly PS_DRAFT = String(PublishStatus.Draft);
  protected readonly PS_PUBLISHED = String(PublishStatus.Published);

  protected get durationStr(): string { return String(this.duration); }
  protected set durationStr(val: string) { this.duration = Number(val); }

  protected get sortOrderStr(): string { return String(this.sortOrder); }
  protected set sortOrderStr(val: string) { this.sortOrder = Number(val); }

  protected get isSelfPaced(): boolean {
    const course = this.layout.course();
    if (!course) return false;
    const accessType = String(course.courseAccessType).trim().toLowerCase();
    return accessType === String(CourseAccessType.SelfPaced) || accessType === 'selfpaced';
  }

  protected get sectionOptions(): { label: string, value: any }[] {
    const course = this.layout.course();
    if (!course || !course.sections) return [];
    return course.sections.map((s: any) => ({
      label: s.title,
      value: s.id.toString()
    }));
  }

  protected get isDirty(): boolean {
    return (
      this.title !== this.originalTitle ||
      this.description !== this.originalDescription ||
      this.type !== this.originalType ||
      this.duration !== this.originalDuration ||
      this.content !== this.originalContent ||
      this.contentUrl !== this.originalContentUrl ||
      this.isPreview !== this.originalIsPreview ||
      (!this.isSelfPaced && this.status !== this.originalStatus) ||
      this.sortOrder !== this.originalSortOrder ||
      this.sectionIdStr !== this.originalSectionIdStr ||
      this.selectedFile !== null
    );
  }

  protected showUnsavedModal = signal(false);
  private unsavedResolve: ((val: boolean) => void) | null = null;

  async canDeactivate(): Promise<boolean> {
    if (!this.isDirty || this.isSaving()) return true;

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

  ngOnInit() {
    this.courseBuilderService.getLessonUploadLimits().subscribe({
      next: (limits) => {
        this.maxVideoSizeMB = limits.videoMaxFileSizeMB;
        this.maxPdfSizeMB = limits.pdfMaxFileSizeMB;
      },
      error: (err) => console.error('Failed to fetch file size limits', err)
    });

    this.route.parent?.paramMap.subscribe(parentParams => {
      this.courseSlug = parentParams.get('slug') || '';
      this.loadRouteParams();
    });
  }

  private loadRouteParams() {
    this.route.paramMap.subscribe(params => {
      const secId = params.get('sectionId');
      const lesId = params.get('lessonId');

      if (lesId) {
        this.isEditMode = true;
        this.lessonId = Number(lesId);
        this.loadLesson(this.lessonId);
      } else if (secId) {
        this.isEditMode = false;
        this.sectionIdStr = secId;
        this.isLoading.set(false);
        this.resetOriginalState();
      } else {
        this.toastService.showError('Invalid route parameters.');
        this.navigateBack();
      }
    });
  }

  private loadLesson(id: number) {
    this.isLoading.set(true);
    this.courseBuilderService.getLesson(id).subscribe({
      next: (lesson) => {
        this.title = lesson.title ?? '';
        this.description = lesson.description ?? '';
        this.type = this.resolveTypeString(lesson.type);
        this.duration = lesson.durationInMinutes ?? 10;
        this.content = lesson.content ?? '';
        this.contentUrl = lesson.contentUrl ?? '';
        this.isPreview = lesson.isPreview ?? false;
        this.status = (lesson.status === PublishStatus.Published || lesson.status === 'Published') ? String(PublishStatus.Published) : String(PublishStatus.Draft);
        this.sortOrder = lesson.sortOrder ?? 0;
        this.sectionIdStr = lesson.courseSectionId?.toString() || '';
        
        this.createdAt = lesson.createdAt;
        this.updatedAt = lesson.updatedAt;

        this.isLoading.set(false);
        this.updateMarkdownPreview();
        this.resetOriginalState();
      },
      error: (err) => {
        this.toastService.showApiError(err, 'Failed to load lesson details.');
        this.navigateBack();
      }
    });
  }

  private resetOriginalState() {
    this.originalTitle = this.title;
    this.originalDescription = this.description;
    this.originalType = this.type;
    this.originalDuration = this.duration;
    this.originalContent = this.content;
    this.originalContentUrl = this.contentUrl;
    this.originalIsPreview = this.isPreview;
    this.originalStatus = this.status;
    this.originalSortOrder = this.sortOrder;
    this.originalSectionIdStr = this.sectionIdStr;
    this.selectedFile = null;
    this.isUploaded = false;
    this.uploadProgress = 0;
  }

  private resolveTypeString(type: string | number): string {
    if (type === LessonType.Video || type === String(LessonType.Video) || type === 'Video') return String(LessonType.Video);
    if (type === LessonType.Pdf || type === String(LessonType.Pdf) || type === 'Pdf') return String(LessonType.Pdf);
    if (type === LessonType.Article || type === String(LessonType.Article) || type === 'Article') return String(LessonType.Article);
    if (type === LessonType.ExternalLink || type === String(LessonType.ExternalLink) || type === 'ExternalLink') return String(LessonType.ExternalLink);
    return String(LessonType.Video);
  }

  protected onFileSelected(event: any) {
    const file = event.target.files?.[0];
    if (file) {
      this.handleFile(file);
    }
  }

  protected onDragOver(event: DragEvent) {
    event.preventDefault();
    event.stopPropagation();
    this.isDragOver = true;
  }

  protected onDragLeave(event: DragEvent) {
    event.preventDefault();
    event.stopPropagation();
    this.isDragOver = false;
  }

  protected onDrop(event: DragEvent) {
    event.preventDefault();
    event.stopPropagation();
    this.isDragOver = false;
    const file = event.dataTransfer?.files?.[0];
    if (file) {
      this.handleFile(file);
    }
  }

  private handleFile(file: File) {
    // Basic file type validation based on lesson type
    if (this.type === this.LT_VIDEO) {
      if (!file.type.startsWith('video/')) {
        this.toastService.showError('Please upload a valid video file.');
        return;
      }
      if (file.size > this.maxVideoSizeMB * 1024 * 1024) {
        this.toastService.showError(`Video file exceeds the maximum allowed size of ${this.maxVideoSizeMB}MB.`);
        return;
      }
    }
    if (this.type === this.LT_PDF) {
      if (file.type !== 'application/pdf') {
        this.toastService.showError('Please upload a valid PDF document.');
        return;
      }
      if (file.size > this.maxPdfSizeMB * 1024 * 1024) {
        this.toastService.showError(`PDF file exceeds the maximum allowed size of ${this.maxPdfSizeMB}MB.`);
        return;
      }
    }

    this.selectedFile = file;
    this.simulateUpload();
  }

  private simulateUpload() {
    this.uploadProgress = 0;
    this.isUploaded = false;
    const interval = setInterval(() => {
      if (this.uploadProgress < 100) {
        this.uploadProgress += 10;
      } else {
        clearInterval(interval);
        this.isUploaded = true;
        this.toastService.showSuccess('File selected successfully.');
      }
    }, 100);
  }

  protected clearSelectedFile() {
    this.selectedFile = null;
    this.isUploaded = false;
    this.uploadProgress = 0;
  }

  protected getFormattedFileSize(bytes: number): string {
    if (!bytes) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  }

  protected async updateMarkdownPreview() {
    if (this.content) {
      const rawHtml = await marked.parse(this.content);
      this.parsedMarkdown = this.sanitizer.bypassSecurityTrustHtml(rawHtml);
    } else {
      this.parsedMarkdown = '';
    }
  }

  protected discardChanges() {
    this.title = this.originalTitle;
    this.description = this.originalDescription;
    this.type = this.originalType;
    this.duration = this.originalDuration;
    this.content = this.originalContent;
    this.contentUrl = this.originalContentUrl;
    this.isPreview = this.originalIsPreview;
    this.status = this.originalStatus;
    this.sortOrder = this.originalSortOrder;
    this.sectionIdStr = this.originalSectionIdStr;
    this.selectedFile = null;
    this.isUploaded = false;
    this.uploadProgress = 0;
    this.updateMarkdownPreview();
    this.toastService.showSuccess('Changes discarded.');
  }

  protected saveDraft() {
    this.status = 'Draft';
    this.saveLesson();
  }

  protected saveChanges() {
    this.status = 'Published';
    this.saveLesson();
  }

  protected previewLesson() {
    if (this.isEditMode && this.lessonId) {
      this.router.navigate([`/${this.routePrefix}/courses`, this.courseSlug, 'lessons', this.lessonId, 'detail']);
    } else {
      this.toastService.showSuccess('Please save the lesson first to preview.');
    }
  }

  protected duplicateLesson() {
    this.toastService.showSuccess('Duplicating lesson...');
  }

  protected archiveLesson() {
    this.toastService.showSuccess('Archiving lesson...');
  }

  protected deleteLesson() {
    if (!this.isEditMode || !this.lessonId) return;
    if (confirm('Are you sure you want to delete this lesson?')) {
      this.courseBuilderService.deleteLesson(this.lessonId).subscribe({
        next: () => {
          this.toastService.showSuccess('Lesson deleted successfully.');
          this.router.navigate([`/${this.routePrefix}/courses`, this.courseSlug, 'builder']);
        },
        error: (err) => {
          this.toastService.showApiError(err, 'Failed to delete lesson.');
        }
      });
    }
  }

  protected saveLesson() {
    if (!this.title.trim()) {
      this.toastService.showError('Lesson title is required.');
      return;
    }

    const formData = new FormData();
    if (this.sectionIdStr) {
      formData.append('CourseSectionId', this.sectionIdStr);
    }
    formData.append('Title', this.title);
    formData.append('Description', this.description);
    
    let typeVal: number = LessonType.Video;
    if (this.type === this.LT_PDF) typeVal = LessonType.Pdf;
    else if (this.type === this.LT_ARTICLE) typeVal = LessonType.Article;
    else if (this.type === this.LT_EXTERNAL) typeVal = LessonType.ExternalLink;
    
    formData.append('Type', typeVal.toString());
    formData.append('DurationInMinutes', this.duration.toString());
    formData.append('IsPreview', this.isPreview.toString());
    if (!this.isSelfPaced) {
      formData.append('Status', (this.status === String(PublishStatus.Published) ? PublishStatus.Published : PublishStatus.Draft).toString());
    }
    
    if (this.sectionIdStr === this.originalSectionIdStr) {
      formData.append('SortOrder', this.sortOrder.toString());
    }

    if (this.selectedFile) formData.append('File', this.selectedFile);
    if (this.type === this.LT_ARTICLE) formData.append('Content', this.content);
    if (this.type === this.LT_EXTERNAL) formData.append('ContentUrl', this.contentUrl);

    if (!this.isEditMode && (this.type === this.LT_VIDEO || this.type === this.LT_PDF) && !this.selectedFile) {
        this.toastService.showError(`File upload is required for this lesson.`);
        return;
    }

    this.isSaving.set(true);

    if (this.isEditMode && this.lessonId) {
      this.courseBuilderService.updateLesson(this.lessonId, formData).subscribe({
        next: () => {
          this.toastService.showSuccess('Lesson updated successfully.');
          this.isSaving.set(false);
          this.resetOriginalState();
          if (this.layout.courseId()) {
            this.layout.loadCourse(this.layout.courseId()!);
          }
          this.router.navigate([`/${this.routePrefix}/courses`, this.courseSlug, 'lessons', this.lessonId, 'detail']);
        },
        error: (err) => {
          this.toastService.showApiError(err, 'Failed to update lesson.');
          this.isSaving.set(false);
        }
      });
    } else {
      this.courseBuilderService.createLesson(formData).subscribe({
        next: (result) => {
          this.toastService.showSuccess('Lesson created successfully.');
          this.isSaving.set(false);
          this.resetOriginalState();
          if (this.layout.courseId()) {
            this.layout.loadCourse(this.layout.courseId()!);
          }
          this.router.navigate([`/${this.routePrefix}/courses`, this.courseSlug, 'builder']);
        },
        error: (err) => {
          this.toastService.showApiError(err, 'Failed to create lesson.');
          this.isSaving.set(false);
        }
      });
    }
  }

  protected navigateBack() {
    if (this.isEditMode && this.lessonId) {
      this.router.navigate([`/${this.routePrefix}/courses`, this.courseSlug, 'lessons', this.lessonId, 'detail']);
    } else {
      this.router.navigate([`/${this.routePrefix}/courses`, this.courseSlug, 'builder']);
    }
  }
}

