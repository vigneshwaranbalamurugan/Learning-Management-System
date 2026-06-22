import { Component, OnInit, signal, computed, inject, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { InstructorAssignmentService, InstructorAssignmentSummaryDto } from '@services/instructor-assignment.service';
import { ToastService } from '@services/toast.service';
import { untilDestroyed } from '../../rxjs/until-destroyed';
import { Loader } from '@components/loader/loader';

@Component({
  selector: 'app-instructor-assignments',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, Loader],
  templateUrl: './instructor-assignments.html'
})
export class InstructorAssignments implements OnInit {
  private assignmentService = inject(InstructorAssignmentService);
  private toastService = inject(ToastService);
  private destroyRef = inject(DestroyRef);

  protected assignments = signal<InstructorAssignmentSummaryDto[]>([]);
  protected isLoading = signal(true);
  protected searchQuery = signal('');

  // Statistics
  protected totalCount = computed(() => this.assignments().length);
  protected totalPendingSubmissions = computed(() => 
    this.assignments().reduce((sum, a) => sum + (a.pendingSubmissionsCount || 0), 0)
  );

  // Client-side filtering by query
  protected filteredAssignments = computed(() => {
    let list = this.assignments();
    const query = this.searchQuery().toLowerCase().trim();

    if (query) {
      list = list.filter(a =>
        (a.title && a.title.toLowerCase().includes(query)) ||
        (a.courseTitle && a.courseTitle.toLowerCase().includes(query)) ||
        (a.sectionTitle && a.sectionTitle.toLowerCase().includes(query))
      );
    }
    return list;
  });

  ngOnInit(): void {
    this.loadAssignments();
  }

  private loadAssignments(): void {
    this.isLoading.set(true);
    this.assignmentService.getInstructorAssignments()
      .pipe(untilDestroyed(this.destroyRef))
      .subscribe({
        next: (data) => {
          this.assignments.set(data || []);
          this.isLoading.set(false);
        },
        error: (err) => {
          this.toastService.showApiError(err, 'Failed to load assignments.');
          this.isLoading.set(false);
        }
      });
  }
}
