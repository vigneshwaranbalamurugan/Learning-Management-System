import { Component, OnInit, signal, inject, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { ToastService } from '@services/toast.service';
import { untilDestroyed } from '../../rxjs/until-destroyed';
import { Loader } from '@components/loader/loader';
import { AssignmentResponse, AssignmentSubmissionResponse, AssignmentStatusResponse } from '@models/assignment';
import { AssignmentService } from '@services/assignment.service';

@Component({
  selector: 'app-assignment-detail',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, Loader],
  templateUrl: './assignment-detail.html'
})
export class AssignmentDetailPage implements OnInit {
  private assignmentService = inject(AssignmentService);
  private toastService = inject(ToastService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private destroyRef = inject(DestroyRef);

  protected assignment = signal<AssignmentResponse | null>(null);
  protected statusInfo = signal<AssignmentStatusResponse | null>(null);
  protected submissions = signal<AssignmentSubmissionResponse[]>([]);
  protected isLoading = signal(true);
  protected isSubmitting = signal(false);

  // Form states
  protected submissionText = '';
  protected submissionType = 'file'; // 'file' | 'link'
  protected submittedUrl = '';
  protected selectedFile: File | null = null;

  ngOnInit(): void {
    this.route.paramMap
      .pipe(untilDestroyed(this.destroyRef))
      .subscribe(params => {
        const id = params.get('id');
        if (id) {
          this.loadAllData(Number(id));
        } else {
          this.router.navigate(['/learner/assignments']);
        }
      });
  }

  private loadAllData(assignmentId: number): void {
    this.isLoading.set(true);

    // Fetch assignment, status, and submissions in parallel/sequence
    this.assignmentService.getAssignmentById(assignmentId)
      .pipe(untilDestroyed(this.destroyRef))
      .subscribe({
        next: (assignmentData) => {
          this.assignment.set(assignmentData);
          
          // Load submissions and status
          this.loadStatusAndSubmissions(assignmentId);
        },
        error: (err) => {
          this.toastService.showApiError(err, 'Failed to load assignment details.');
          this.isLoading.set(false);
          this.router.navigate(['/learner/assignments']);
        }
      });
  }

  private loadStatusAndSubmissions(assignmentId: number): void {
    // Fetch status
    this.assignmentService.getAssignmentStatus(assignmentId)
      .pipe(untilDestroyed(this.destroyRef))
      .subscribe({
        next: (statusData) => {
          this.statusInfo.set(statusData);
        },
        error: (err) => {
          console.error('Failed to load status info', err);
        }
      });

    // Fetch submission history
    this.assignmentService.getAssignmentSubmissions(assignmentId)
      .pipe(untilDestroyed(this.destroyRef))
      .subscribe({
        next: (submissionsList) => {
          this.submissions.set(submissionsList || []);
          this.isLoading.set(false);
        },
        error: (err) => {
          this.toastService.showApiError(err, 'Failed to load submission history.');
          this.isLoading.set(false);
        }
      });
  }

  protected onFileSelected(event: any): void {
    const file = event.target.files[0];
    if (file) {
      this.selectedFile = file;
    }
  }

  protected onSubmit(): void {
    const assignmentData = this.assignment();
    if (!assignmentData) return;

    if (this.submissionType === 'file' && !this.selectedFile) {
      this.toastService.show('Please select a file to upload.', 'error');
      return;
    }

    if (this.submissionType === 'link' && !this.submittedUrl.trim()) {
      this.toastService.show('Please enter a submission URL.', 'error');
      return;
    }

    this.isSubmitting.set(true);

    const formData = new FormData();
    formData.append('AssignmentId', assignmentData.id.toString());
    formData.append('SubmissionText', this.submissionText);
    
    // 0 = File, 1 = Link
    const typeValue = this.submissionType === 'file' ? '0' : '1';
    formData.append('AttachmentType', typeValue);

    if (this.submissionType === 'file' && this.selectedFile) {
      formData.append('AttachmentFile', this.selectedFile);
    } else if (this.submissionType === 'link') {
      formData.append('SubmittedAssignmentUrl', this.submittedUrl);
    }

    this.assignmentService.submitAssignment(formData)
      .pipe(untilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.toastService.show('Assignment submitted successfully!', 'success');
          // Reset form fields
          this.submissionText = '';
          this.submittedUrl = '';
          this.selectedFile = null;
          this.isSubmitting.set(false);
          // Reload status and submission history
          this.loadStatusAndSubmissions(assignmentData.id);
        },
        error: (err) => {
          this.toastService.showApiError(err, 'Submission failed.');
          this.isSubmitting.set(false);
        }
      });
  }

  protected goBack(): void {
    this.router.navigate(['/learner/assignments']);
  }

  protected getStatusColor(status: string): string {
    const st = status.toLowerCase();
    if (st.includes('pass') || st.includes('grade')) return 'bg-emerald-50 text-emerald-700 border-emerald-100';
    if (st.includes('fail')) return 'bg-red-50 text-red-700 border-red-100';
    if (st.includes('submit') || st.includes('review')) return 'bg-amber-50 text-amber-700 border-amber-100';
    return 'bg-gray-50 text-gray-700 border-gray-100';
  }
}
