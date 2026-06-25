import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute, RouterModule } from '@angular/router';
import { ToastService } from '@services/toast.service';
import { LessonResourcesService } from '@services/lesson-resources.service';
import { FormInput } from '@components/form-input/form-input';
import { Dropdown } from '@components/dropdown/dropdown';
import { Button } from '@components/button/button';
import { Loader } from '@components/loader/loader';
import { InstructorCourseLayout } from '../instructor-course-layout/instructor-course-layout';
import { CourseBuilderService } from '@services/course-builder.service';

@Component({
  selector: 'app-instructor-resource-form',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, FormInput, Dropdown, Button, Loader],
  templateUrl: './instructor-resource-form.html'
})
export class InstructorResourceForm implements OnInit {
  private toastService = inject(ToastService);
  private resourcesService = inject(LessonResourcesService);
  private courseBuilderService = inject(CourseBuilderService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  protected layout = inject(InstructorCourseLayout);

  protected courseSlug = '';
  protected lessonIdStr: string = '';
  protected resourceId: number | null = null;
  protected isEditMode = false;
  protected isLoading = signal(true);
  protected isSaving = signal(false);

  // Form State
  protected title = '';
  protected description = '';
  protected type = 'Pdf'; // As requested by user, only Pdf or ExternalLink
  protected contentUrl = '';
  protected selectedFile: File | null = null;
  protected status = 'Draft';

  // File Upload State
  protected isDragOver = false;
  protected uploadProgress = signal(0);
  protected isUploaded = signal(false);
  protected maxPdfSizeMB = 50;

  // Original State to detect changes
  private originalTitle = '';
  private originalDescription = '';
  private originalType = 'Pdf';
  private originalContentUrl = '';
  protected originalStatus = 'Draft';
  protected originalLessonIdStr: string = '';

  protected resourceTypeOptions = [
    { value: 'Pdf', label: 'PDF Document' },
    { value: 'ExternalLink', label: 'External Link' }
  ];

  protected statusOptions = [
    { value: 'Draft', label: 'Draft' },
    { value: 'Published', label: 'Published' }
  ];

  protected get isSelfPaced(): boolean {
    const course = this.layout.course();
    if (!course) return false;
    const accessType = String(course.courseAccessType).trim().toLowerCase();
    return accessType === '1' || accessType === 'selfpaced';
  }

  protected get isDirty(): boolean {
    return (
      this.title !== this.originalTitle ||
      this.description !== this.originalDescription ||
      this.type !== this.originalType ||
      this.contentUrl !== this.originalContentUrl ||
      (!this.isSelfPaced && this.status !== this.originalStatus) ||
      this.lessonIdStr !== this.originalLessonIdStr ||
      this.selectedFile !== null
    );
  }

  ngOnInit() {
    this.courseBuilderService.getLessonUploadLimits().subscribe({
      next: (limits) => {
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
      const lesId = params.get('lessonId');
      const resId = params.get('resourceId');

      if (resId) {
        this.isEditMode = true;
        this.resourceId = Number(resId);
        this.loadResource(this.resourceId);
      } else if (lesId) {
        this.isEditMode = false;
        this.lessonIdStr = lesId;
        this.isLoading.set(false);
        this.resetOriginalState();
      } else {
        this.toastService.showError('Invalid route parameters.');
        this.navigateBack();
      }
    });
  }

  private loadResource(id: number) {
    this.isLoading.set(true);
    this.resourcesService.getResourceById(id).subscribe({
      next: (resource) => {
        this.title = resource.resourceTitle ?? '';
        this.description = resource.description ?? '';
        this.type = this.resolveTypeString(resource.resourceType);
        this.contentUrl = resource.resourceUrl ?? '';
        this.status = resource.status === 2 || resource.status === 'Published' ? 'Published' : 'Draft';
        this.lessonIdStr = resource.lessonId?.toString() || '';
        
        this.isLoading.set(false);
        this.resetOriginalState();
      },
      error: (err) => {
        this.toastService.showApiError(err, 'Failed to load resource details.');
        this.navigateBack();
      }
    });
  }

  private resetOriginalState() {
    this.originalTitle = this.title;
    this.originalDescription = this.description;
    this.originalType = this.type;
    this.originalContentUrl = this.contentUrl;
    this.originalStatus = this.status;
    this.originalLessonIdStr = this.lessonIdStr;
    this.selectedFile = null;
    this.isUploaded.set(false);
    this.uploadProgress.set(0);
  }

  private resolveTypeString(type: string | number): string {
    if (type === 0 || type === '0' || type === 'Pdf') return 'Pdf';
    if (type === 1 || type === '1' || type === 'ExternalLink') return 'ExternalLink';
    return 'Pdf';
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
    if (this.type === 'Pdf') {
      if (file.type !== 'application/pdf') {
        this.toastService.showError('Please upload a valid PDF document.');
        return;
      }
      if (file.size > this.maxPdfSizeMB * 1024 * 1024) {
        this.toastService.showError(`PDF file exceeds the maximum allowed size of ${this.maxPdfSizeMB}MB.`);
        return;
      }
    } else {
        this.toastService.showError(`Cannot upload file for ExternalLink.`);
        return;
    }

    this.selectedFile = file;
    this.simulateUpload();
  }

  private simulateUpload() {
    this.uploadProgress.set(0);
    this.isUploaded.set(false);
    const interval = setInterval(() => {
      if (this.uploadProgress() < 100) {
        this.uploadProgress.update(v => v + 10);
      } else {
        clearInterval(interval);
        this.isUploaded.set(true);
        this.toastService.showSuccess('File selected successfully.');
      }
    }, 100);
  }

  protected clearSelectedFile() {
    this.selectedFile = null;
    this.isUploaded.set(false);
    this.uploadProgress.set(0);
  }

  protected getFormattedFileSize(bytes: number): string {
    if (!bytes) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  }

  protected discardChanges() {
    this.title = this.originalTitle;
    this.description = this.originalDescription;
    this.type = this.originalType;
    this.contentUrl = this.originalContentUrl;
    this.status = this.originalStatus;
    this.lessonIdStr = this.originalLessonIdStr;
    this.selectedFile = null;
    this.isUploaded.set(false);
    this.uploadProgress.set(0);
    this.toastService.showSuccess('Changes discarded.');
  }

  protected saveDraft() {
    this.status = 'Draft';
    this.saveResource();
  }

  protected saveChanges() {
    this.status = 'Published';
    this.saveResource();
  }

  protected deleteResource() {
    if (!this.isEditMode || !this.resourceId) return;
    if (confirm('Are you sure you want to delete this resource?')) {
      this.resourcesService.deleteResource(this.resourceId).subscribe({
        next: () => {
          this.toastService.showSuccess('Resource deleted successfully.');
          this.router.navigate(['/instructor/courses', this.courseSlug, 'builder']);
        },
        error: (err) => {
          this.toastService.showApiError(err, 'Failed to delete resource.');
        }
      });
    }
  }

  protected saveResource() {
    if (!this.title.trim()) {
      this.toastService.showError('Resource title is required.');
      return;
    }

    let typeVal = 0; // Default to Pdf
    if (this.type === 'ExternalLink') typeVal = 1;

    if (!this.isEditMode && this.type === 'Pdf' && !this.selectedFile) {
        this.toastService.showError(`File upload is required for PDF resource.`);
        return;
    }

    if (this.type === 'ExternalLink' && !this.contentUrl.trim()) {
        this.toastService.showError(`Resource URL is required for External Link.`);
        return;
    }

    this.isSaving.set(true);

    if (this.isEditMode && this.resourceId) {
      const req = {
        resourceType: typeVal,
        resourceTitle: this.title,
        resourceUrl: this.type === 'ExternalLink' ? this.contentUrl : undefined,
        description: this.description,
        status: !this.isSelfPaced ? (this.status === 'Published' ? 2 : 1) : undefined,
        file: this.type === 'Pdf' && this.selectedFile ? this.selectedFile : undefined
      };

      this.resourcesService.updateResource(this.resourceId, req).subscribe({
        next: () => {
          this.toastService.showSuccess('Resource updated successfully.');
          this.isSaving.set(false);
          this.resetOriginalState();
          if (this.layout.courseId()) {
            this.layout.loadCourse(this.layout.courseId()!);
          }
          this.navigateBack();
        },
        error: (err) => {
          this.toastService.showApiError(err, 'Failed to update resource.');
          this.isSaving.set(false);
        }
      });
    } else {
      const req = {
        lessonId: Number(this.lessonIdStr),
        resourceType: typeVal,
        resourceTitle: this.title,
        resourceUrl: this.type === 'ExternalLink' ? this.contentUrl : undefined,
        description: this.description,
        status: !this.isSelfPaced ? (this.status === 'Published' ? 2 : 1) : 1, // Default draft
        file: this.type === 'Pdf' && this.selectedFile ? this.selectedFile : undefined
      };

      this.resourcesService.addResource(req).subscribe({
        next: () => {
          this.toastService.showSuccess('Resource created successfully.');
          this.isSaving.set(false);
          this.resetOriginalState();
          if (this.layout.courseId()) {
            this.layout.loadCourse(this.layout.courseId()!);
          }
          this.navigateBack();
        },
        error: (err) => {
          this.toastService.showApiError(err, 'Failed to create resource.');
          this.isSaving.set(false);
        }
      });
    }
  }

  protected navigateBack() {
    this.router.navigate(['/instructor/courses', this.courseSlug, 'builder']);
  }
}
