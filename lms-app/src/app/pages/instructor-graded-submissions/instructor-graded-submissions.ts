import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule, Location } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { InstructorAssignmentService } from '@services/instructor-assignment.service';
import { AssignmentService } from '@services/assignment.service';
import { ToastService } from '@services/toast.service';
import { AssignmentSubmissionResponse, AssignmentResponse } from '@models/assignment';
import { Loader } from '@components/loader/loader';
import { finalize } from 'rxjs';

@Component({
  selector: 'app-instructor-graded-submissions',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, Loader],
  templateUrl: './instructor-graded-submissions.html'
})
export class InstructorGradedSubmissions implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private location = inject(Location);
  private instructorService = inject(InstructorAssignmentService);
  private assignmentService = inject(AssignmentService);
  private toastService = inject(ToastService);

  protected assignmentId = signal<number>(0);
  protected assignment = signal<AssignmentResponse | null>(null);
  protected submissions = signal<AssignmentSubmissionResponse[]>([]);
  protected isLoading = signal<boolean>(true);
  
  // Grading Panel State
  protected selectedSubmission = signal<AssignmentSubmissionResponse | null>(null);
  protected marksAwarded = signal<number | null>(null);
  protected feedback = signal<string>('');
  protected isSubmitting = signal<boolean>(false);

  ngOnInit(): void {
    const idParam = this.route.snapshot.paramMap.get('assignmentId');
    if (idParam) {
      this.assignmentId.set(+idParam);
      this.loadData();
    } else {
      this.router.navigate(['/instructor/assignments']);
    }
  }

  private loadData(): void {
    this.isLoading.set(true);
    
    this.assignmentService.getAssignmentById(this.assignmentId()).subscribe({
      next: (res) => {
        this.assignment.set(res);
        this.loadSubmissions();
      },
      error: (err) => {
        this.toastService.showApiError(err, 'Failed to load assignment details');
        this.isLoading.set(false);
      }
    });
  }

  private loadSubmissions(): void {
    this.instructorService.getGradedSubmissions(this.assignmentId())
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: (res) => {
          this.submissions.set(res || []);
        },
        error: (err) => {
          this.toastService.showApiError(err, 'Failed to load graded submissions');
        }
      });
  }

  protected goBack(): void {
    this.location.back();
  }

  protected selectSubmission(submission: AssignmentSubmissionResponse): void {
    this.selectedSubmission.set(submission);
    this.marksAwarded.set(submission.marksAwarded ?? null);
    this.feedback.set(submission.feedback ?? '');
  }

  protected closeGradingPanel(): void {
    this.selectedSubmission.set(null);
    this.marksAwarded.set(null);
    this.feedback.set('');
  }

  protected submitGrade(): void {
    const submission = this.selectedSubmission();
    if (!submission) return;

    if (this.marksAwarded() === null || this.marksAwarded() === undefined) {
      this.toastService.show('Marks are required', 'error');
      return;
    }

    if (!this.feedback().trim()) {
      this.toastService.show('Feedback is required', 'error');
      return;
    }

    const totalMarks = this.assignment()?.totalMarks || 100;
    if (this.marksAwarded()! < 0 || this.marksAwarded()! > totalMarks) {
      this.toastService.show(`Marks must be between 0 and ${totalMarks}`, 'error');
      return;
    }

    this.isSubmitting.set(true);
    
    this.instructorService.gradeSubmission(submission.id, this.marksAwarded()!, this.feedback())
      .pipe(finalize(() => this.isSubmitting.set(false)))
      .subscribe({
        next: () => {
          this.toastService.show('Grade updated successfully!', 'success');
          this.closeGradingPanel();
          this.loadSubmissions();
        },
        error: (err) => {
          this.toastService.showApiError(err, 'Failed to update grade.');
        }
      });
  }
}
