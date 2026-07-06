import { Component, OnInit, inject, DestroyRef, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { InstructorAssignmentService } from '@services/instructor-assignment.service';
import { InstructorAssignmentSummaryDto, PagedInstructorAssignmentResponse } from '@models/assignment';
import { ToastService } from '@services/toast.service';
import { untilDestroyed } from '../../rxjs/until-destroyed';
import { Loader } from '@components/loader/loader';
import { PaginationComponent } from '@components/pagination/pagination.component';
import { SearchInput } from '@components/search-input/search-input';
import { Dropdown } from '@components/dropdown/dropdown';

@Component({
  selector: 'app-instructor-assignments',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, Loader, PaginationComponent, SearchInput, Dropdown],
  templateUrl: './instructor-assignments.html'
})
export class InstructorAssignments implements OnInit {
  private assignmentService = inject(InstructorAssignmentService);
  private toastService = inject(ToastService);
  private destroyRef = inject(DestroyRef);
  private cdr = inject(ChangeDetectorRef);

  assignments: InstructorAssignmentSummaryDto[] = [];
  isLoading = true;
  
  // Pagination
  page = 1;
  pageSize = 10;
  totalCount = 0;
  totalPages = 0;

  // Filters
  searchQuery = '';
  selectedStatus = '';

  // Stats
  totalPendingSubmissions = 0;
  fullyGradedCount = 0;
  uniqueCoursesCount = 0;

  statusOptions = [
    { value: '', label: 'All Statuses' },
    { value: '0', label: 'Draft' },
    { value: '1', label: 'Published' }
  ];

  ngOnInit(): void {
    this.loadAssignments();
  }

  loadAssignments(): void {
    this.isLoading = true;
    const statusFilter = this.selectedStatus ? parseInt(this.selectedStatus) : undefined;
    
    this.assignmentService.getInstructorAssignments(this.page, this.pageSize, this.searchQuery, statusFilter)
      .pipe(untilDestroyed(this.destroyRef))
      .subscribe({
        next: (res: PagedInstructorAssignmentResponse) => {
          this.assignments = res.assignments;
          this.totalCount = res.totalCount;
          this.totalPages = res.totalPages;
          this.totalPendingSubmissions = res.totalPendingCount;
          this.fullyGradedCount = res.fullyGradedCount;
          this.uniqueCoursesCount = res.uniqueCourseCount;
          this.isLoading = false;
          this.cdr.detectChanges();
        },
        error: (err) => {
          this.toastService.showApiError(err, 'Failed to load assignments.');
          this.isLoading = false;
        }
      });
  }

  onSearchChange(search: string) {
    this.searchQuery = search;
    this.page = 1;
    this.loadAssignments();
  }

  onStatusChange(status: string) {
    this.selectedStatus = status;
    this.page = 1;
    this.loadAssignments();
  }

  onPageChange(newPage: number) {
    this.page = newPage;
    this.loadAssignments();
  }
}
