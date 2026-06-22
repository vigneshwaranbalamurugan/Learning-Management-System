import { Component, OnInit, signal, inject, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { InstructorAssignmentService } from '@services/instructor-assignment.service';
import { DashboardService } from '@services/dashboard.service';
import { ToastService } from '@services/toast.service';
import { untilDestroyed } from '../../rxjs/until-destroyed';
import { Loader } from '@components/loader/loader';
import { AssignmentSubmissionResponse, AssignmentResponse } from '@models/assignment';

@Component({
  selector: 'app-instructor-evaluate',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, Loader],
  templateUrl: './instructor-evaluate.html'
})
export class InstructorEvaluate implements OnInit {
  private assignmentService = inject(InstructorAssignmentService);
  private dashboardService = inject(DashboardService);
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
    this.dashboardService.getAssignmentById(this.assignmentId)
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
    this.assignmentService.getPendingSubmissions(this.assignmentId)
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
    this.assignmentService.gradeSubmission(submission.id, this.marksAwarded, this.feedback)
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
