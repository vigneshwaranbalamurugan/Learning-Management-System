import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute, RouterModule } from '@angular/router';
import { AuthService } from '@services/auth.service';
import { InstructorCourseLayout } from '../instructor-course-layout/instructor-course-layout';
import { ToastService } from '@services/toast.service';
import { FormInput } from '@components/form-input/form-input';
import { Dropdown } from '@components/dropdown/dropdown';
import { Button } from '@components/button/button';
import { Loader } from '@components/loader/loader';
import { AssignmentService } from '@services/assignment.service';
import { ConfirmModal } from '@components/confirm-modal/confirm-modal';
import { AssignmentAttachmentType } from '../../enums/assignment-attachment-type.enum';

@Component({
  selector: 'app-instructor-assignment-form',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, FormInput, Dropdown, Button, Loader, ConfirmModal],
  templateUrl: './instructor-assignment-form.html',
  styleUrl: './instructor-assignment-form.css'
})
export class InstructorAssignmentForm implements OnInit {
  protected layout = inject(InstructorCourseLayout);
  private toastService = inject(ToastService);
  private authService = inject(AuthService);

  protected get routePrefix(): string {
    return this.authService.userRole()?.toLowerCase() || 'instructor';
  }
  private assignmentService = inject(AssignmentService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);

  protected isEditMode = false;
  protected assignmentId: number | null = null;
  protected isLoading = signal(true);
  protected isSaving = signal(false);

  // Form Fields
  protected sectionId: number | null = null;
  protected title = '';
  protected description = '';
  protected instructions = '';
  protected isCompulsory = true;
  protected totalMarks = 100;
  protected passingMarks = 50;
  protected attachmentType = AssignmentAttachmentType.None;
  protected attachmentUrl = '';
  protected currentAttachmentPath = ''; // Holds existing filename/url when editing
  protected selectedFile: File | null = null;
  protected deadlineInDays = 7;
  protected maxSubmissions = 1;
  protected isLateSubmissionAllowed = false;
  protected maxFileSizeMB: number = 10; // default fallback

  private initialFormState = '';
  protected showUnsavedModal = signal(false);
  private unsavedResolve: ((val: boolean) => void) | null = null;

  private captureInitialState() {
    this.initialFormState = JSON.stringify({
      sectionId: this.sectionId,
      title: this.title,
      description: this.description,
      instructions: this.instructions,
      isCompulsory: this.isCompulsory,
      totalMarks: this.totalMarks,
      passingMarks: this.passingMarks,
      attachmentType: this.attachmentType,
      attachmentUrl: this.attachmentUrl,
      deadlineInDays: this.deadlineInDays,
      maxSubmissions: this.maxSubmissions,
      isLateSubmissionAllowed: this.isLateSubmissionAllowed
    });
  }

  protected get isDirty(): boolean {
    const currentState = JSON.stringify({
      sectionId: this.sectionId,
      title: this.title,
      description: this.description,
      instructions: this.instructions,
      isCompulsory: this.isCompulsory,
      totalMarks: this.totalMarks,
      passingMarks: this.passingMarks,
      attachmentType: this.attachmentType,
      attachmentUrl: this.attachmentUrl,
      deadlineInDays: this.deadlineInDays,
      maxSubmissions: this.maxSubmissions,
      isLateSubmissionAllowed: this.isLateSubmissionAllowed
    });
    return currentState !== this.initialFormState || this.selectedFile !== null;
  }

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

  protected get sectionIdStr(): string {
    return this.sectionId ? String(this.sectionId) : '';
  }

  protected setSectionIdStr(val: string) {
    this.sectionId = val ? Number(val) : null;
  }

  protected get attachmentTypeStr(): string {
    return String(this.attachmentType);
  }

  protected setAttachmentTypeStr(val: string) {
    this.attachmentType = Number(val);
  }

  protected readonly AssignmentAttachmentType = AssignmentAttachmentType;

  protected get totalMarksStr(): string {
    return String(this.totalMarks);
  }

  protected set totalMarksStr(val: string) {
    this.totalMarks = Number(val);
  }

  protected get passingMarksStr(): string {
    return String(this.passingMarks);
  }

  protected set passingMarksStr(val: string) {
    this.passingMarks = Number(val);
  }

  protected get deadlineInDaysStr(): string {
    return String(this.deadlineInDays);
  }

  protected set deadlineInDaysStr(val: string) {
    this.deadlineInDays = Number(val);
  }

  protected get maxSubmissionsStr(): string {
    return String(this.maxSubmissions);
  }

  protected set maxSubmissionsStr(val: string) {
    this.maxSubmissions = Number(val);
  }

  protected get sectionOptions() {
    return (this.course?.sections || []).map(sec => ({
      value: String(sec.id),
      label: sec.title
    }));
  }

  protected attachmentTypeOptions = [
    { value: String(AssignmentAttachmentType.None), label: 'None' },
    { value: String(AssignmentAttachmentType.File), label: 'File Upload 📁' },
    { value: String(AssignmentAttachmentType.Link), label: 'External URL Link 🔗' }
  ];

  protected get course() {
    return this.layout.course();
  }

  ngOnInit() {
    this.route.paramMap.subscribe(params => {
      const idParam = params.get('assignmentId');
      if (idParam) {
        this.isEditMode = true;
        this.assignmentId = +idParam;
        this.loadAssignmentDetails(this.assignmentId);
      } else {
        // Pre-select first section if available
        const currentCourse = this.course;
        if (currentCourse && currentCourse.sections && currentCourse.sections.length > 0) {
          this.sectionId = currentCourse.sections[0].id;
        }
        this.captureInitialState();
        this.isLoading.set(false);
      }
    });

    this.assignmentService.getAssignmentUploadLimits().subscribe({
      next: (limits) => {
        this.maxFileSizeMB = limits.maxFileSizeMB;
      },
      error: () => {
        // Silently fallback to default if it fails
      }
    });
  }

  private loadAssignmentDetails(id: number) {
    this.assignmentService.getAssignment(id).subscribe({
      next: (assignment) => {
        this.sectionId = assignment.courseSectionId;
        this.title = assignment.title;
        this.description = assignment.description || '';
        this.instructions = assignment.instructions || '';
        this.isCompulsory = assignment.isCompulsory;
        this.totalMarks = assignment.totalMarks;
        this.passingMarks = assignment.passingMarks;
        this.attachmentType = assignment.attachmentType as AssignmentAttachmentType;
        this.attachmentUrl = assignment.attachmentUrl || '';
        this.currentAttachmentPath = assignment.attachmentPath || '';
        this.deadlineInDays = assignment.deadlineInDays;
        this.maxSubmissions = assignment.maxSubmissions;
        this.isLateSubmissionAllowed = assignment.isLateSubmissionAllowed;
        this.captureInitialState();
        this.isLoading.set(false);
      },
      error: (err) => {
        this.toastService.showApiError(err, 'Failed to load assignment details.');
        this.isLoading.set(false);
      }
    });
  }

  protected onFileSelected(event: any) {
    const file = event.target.files?.[0];
    if (file) {
      this.selectedFile = file;
    }
  }

  protected saveAssignment() {
    if (!this.course) return;
    if (!this.sectionId) {
      this.toastService.showError('Section is required.');
      return;
    }
    if (!this.title.trim()) {
      this.toastService.showError('Assignment title is required.');
      return;
    }

    const formData = new FormData();
    formData.append('CourseSectionId', this.sectionId.toString());
    formData.append('Title', this.title);
    formData.append('Description', this.description);
    formData.append('Instructions', this.instructions);
    formData.append('IsCompulsory', this.isCompulsory.toString());
    formData.append('TotalMarks', this.totalMarks.toString());
    formData.append('PassingMarks', this.passingMarks.toString());
    formData.append('AttachmentType', this.attachmentType.toString());
    formData.append('DeadlineInDays', this.deadlineInDays.toString());
    formData.append('MaxSubmissions', this.maxSubmissions.toString());
    formData.append('IsLateSubmissionAllowed', this.isLateSubmissionAllowed.toString());

    if (this.attachmentType === AssignmentAttachmentType.File) {
      if (this.selectedFile) {
        formData.append('AttachmentFile', this.selectedFile);
      }
    } else if (this.attachmentType === AssignmentAttachmentType.Link) {
      formData.append('AttachmentUrl', this.attachmentUrl);
    }

    this.isSaving.set(true);
    const request = this.isEditMode && this.assignmentId
      ? this.assignmentService.updateAssignment(this.assignmentId, formData)
      : this.assignmentService.createAssignment(formData);

    request.subscribe({
      next: () => {
        this.toastService.showSuccess(this.isEditMode ? 'Assignment updated successfully.' : 'Assignment created successfully.');
        this.layout.loadCourse(this.course!.id);
        this.isSaving.set(false);
        this.navigateBack();
      },
      error: (err) => {
        this.toastService.showApiError(err, 'Failed to save assignment.');
        this.isSaving.set(false);
      }
    });
  }

  protected navigateBack() {
    if (this.course) {
      this.router.navigate([`/${this.routePrefix}/courses`, this.course.slug, 'assignments']);
    }
  }
}
