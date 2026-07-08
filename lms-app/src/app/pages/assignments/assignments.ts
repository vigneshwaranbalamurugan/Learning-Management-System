import { Component, OnInit, signal, computed, inject, DestroyRef, effect, untracked } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, Router } from '@angular/router';
import { ToastService } from '@services/toast.service';
import { untilDestroyed } from '../../rxjs/until-destroyed';
import { Loader } from '@components/loader/loader';
import { forkJoin, of } from 'rxjs';
import { catchError, map, switchMap } from 'rxjs/operators';
import { AssignmentResponse, AssignmentStatusResponse } from '@models/assignment';
import { CourseService } from '@services/course.service';
import { EnrollmentService } from '@services/enrollment.service';
import { AssignmentService } from '@services/assignment.service';

interface EnrichedAssignment {
  id: number;
  title: string;
  courseTitle: string;
  sectionTitle: string;
  isCompulsory: boolean;
  totalMarks: number;
  passingMarks: number;
  deadlineInDays: number;
  deadlineDate?: string;
  maxSubmissions: number;
  status: number; // PublishStatus
  latestStatus?: string; // e.g. "Pending", "Submitted", "UnderReview", "Graded", "Passed", "Failed"
  isPassed: boolean | null;
  attemptsMade: number;
  remainingAttempts: number;
  deadline?: string;
}

import { PaginationComponent } from '../../components/pagination/pagination.component';

@Component({
  selector: 'app-assignments-page',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, Loader, PaginationComponent],
  templateUrl: './assignments.html'
})
export class AssignmentsPage implements OnInit {
  private assignmentService = inject(AssignmentService);
  private enrollmentService = inject(EnrollmentService);
  private courseService = inject(CourseService);
  private toastService = inject(ToastService);
  private router = inject(Router);
  private destroyRef = inject(DestroyRef);

  protected assignments = signal<EnrichedAssignment[]>([]);
  protected isLoading = signal(true);
  protected searchQuery = signal('');

  protected currentPage = signal(1);
  protected pageSize = signal(10);
  protected totalPages = signal(0);
  
  protected totalCount = signal(0);
  protected pendingCount = signal(0);
  protected passedCount = signal(0);
  protected failedCount = signal(0);
  protected Math = Math;

  // Client-side search filtering no longer used directly since backend handles it, but we can debounce search.
  protected filteredAssignments = computed(() => this.assignments());

  constructor() {
    // Handle search debounce
    effect(() => {
      const query = this.searchQuery();
      untracked(() => {
        this.currentPage.set(1);
        this.loadAssignments();
      });
    });
  }

  ngOnInit(): void {
    // Load initial page
    this.loadAssignments();
  }

  protected onPageChange(page: number): void {
    this.currentPage.set(page);
    this.loadAssignments();
  }

  private loadAssignments(): void {
    this.isLoading.set(true);
    
    this.assignmentService.getMyAssignments(this.currentPage(), this.pageSize(), this.searchQuery())
      .pipe(untilDestroyed(this.destroyRef))
      .subscribe({
        next: (data: any) => {
          this.assignments.set(data.assignments || []);
          this.totalCount.set(data.totalCount || 0);
          this.totalPages.set(data.totalPages || 0);
          this.pendingCount.set(data.pendingCount || 0);
          this.passedCount.set(data.passedCount || 0);
          this.failedCount.set(data.failedCount || 0);
          this.isLoading.set(false);
        },
        error: (err) => {
          this.toastService.showApiError(err, 'Failed to load assignments.');
          this.isLoading.set(false);
        }
      });
  }

  protected viewAssignmentDetail(assignmentId: number): void {
    this.router.navigate(['/learner/assignments', assignmentId]);
  }

  protected navigateToExplore(): void {
    this.router.navigate(['/learner/explore']);
  }
}
