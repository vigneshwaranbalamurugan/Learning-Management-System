import { Component, OnInit, signal, inject, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { InstructorAssignmentService } from '@services/instructor-assignment.service';
import { ToastService } from '@services/toast.service';
import { untilDestroyed } from '../../rxjs/until-destroyed';
import { Loader } from '@components/loader/loader';
import { AssignmentSubmissionResponse, AssignmentResponse } from '@models/assignment';
import { AssignmentService } from '@services/assignment.service';
import { ConfirmModal } from '@components/confirm-modal/confirm-modal';
import { AssignmentAttachmentType } from '../../enums/assignment-attachment-type.enum';

@Component({
  selector: 'app-instructor-evaluate',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, Loader, ConfirmModal],
  templateUrl: './instructor-evaluate.html'
})
export class InstructorEvaluate implements OnInit {
  private instructorAssignmentService = inject(InstructorAssignmentService);
  private assignmentService = inject(AssignmentService);
  private toastService = inject(ToastService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private destroyRef = inject(DestroyRef);

  protected assignmentId = 0;
  protected assignment = signal<AssignmentResponse | null>(null);
  
  protected submissions = signal<AssignmentSubmissionResponse[]>([]);
  protected isLoading = signal(true);
  protected isSubmitting = signal(false);

  // Grading modal/panel state
  protected selectedSubmission = signal<AssignmentSubmissionResponse | null>(null);
  protected marksAwarded = 0;
  protected feedback = '';

  protected showUnsavedModal = signal(false);
  private unsavedResolve: ((val: boolean) => void) | null = null;
  protected readonly AssignmentAttachmentType = AssignmentAttachmentType;

  protected get isDirty(): boolean {
    const submission = this.selectedSubmission();
    if (!submission) return false;
    return this.marksAwarded !== (submission.marksAwarded ?? 0) || this.feedback !== (submission.feedback ?? '');
  }

  async canDeactivate(): Promise<boolean> {
    if (!this.isDirty || this.isSubmitting()) return true;

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

  ngOnInit(): void {
    this.route.paramMap
      .pipe(untilDestroyed(this.destroyRef))
      .subscribe(params => {
        const id = params.get('assignmentId');
        if (id) {
          this.assignmentId = Number(id);
          this.loadData();
        } else {
          this.router.navigate(['/instructor/assignments']);
        }
      });
  }

  private loadData(): void {
    this.isLoading.set(true);

    // Fetch assignment details to get total marks & title
    this.assignmentService.getAssignmentById(this.assignmentId)
      .pipe(untilDestroyed(this.destroyRef))
      .subscribe({
        next: (assignmentData) => {
          this.assignment.set(assignmentData);

          // Now load the pending submissions
          this.loadPendingSubmissions();
        },
        error: (err) => {
          this.toastService.showApiError(err, 'Failed to load assignment info.');
          this.router.navigate(['/instructor/assignments']);
        }
      });
  }

  private loadPendingSubmissions(): void {
    this.instructorAssignmentService.getPendingSubmissions(this.assignmentId)
      .pipe(untilDestroyed(this.destroyRef))
      .subscribe({
        next: (data) => {
          this.submissions.set(data || []);
          this.isLoading.set(false);
        },
        error: (err) => {
          this.toastService.showApiError(err, 'Failed to load pending submissions.');
          this.isLoading.set(false);
        }
      });
  }

  protected selectSubmission(submission: AssignmentSubmissionResponse): void {
    this.selectedSubmission.set(submission);
    this.marksAwarded = submission.marksAwarded ?? 0;
    this.feedback = submission.feedback ?? '';
  }

  protected closeGradingPanel(): void {
    this.selectedSubmission.set(null);
  }

  protected submitGrade(): void {
    const submission = this.selectedSubmission();
    const assignmentData = this.assignment();
    if (!submission || !assignmentData) return;

    if (this.marksAwarded < 0 || this.marksAwarded > assignmentData.totalMarks) {
      this.toastService.show(`Marks awarded must be between 0 and ${assignmentData.totalMarks}.`, 'error');
      return;
    }

    if (!this.feedback.trim()) {
      this.toastService.show('Please provide feedback for the submission.', 'error');
      return;
    }

    this.isSubmitting.set(true);
    this.instructorAssignmentService.gradeSubmission(submission.id, this.marksAwarded, this.feedback)
      .pipe(untilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.toastService.show('Submission evaluated successfully!', 'success');
          this.closeGradingPanel();
          this.loadPendingSubmissions();
        },
        error: (err) => {
          this.toastService.showApiError(err, 'Failed to grade submission.');
          this.isSubmitting.set(false);
        }
      });
  }

  protected goBack(): void {
    this.router.navigate(['/instructor/assignments']);
  }
}
