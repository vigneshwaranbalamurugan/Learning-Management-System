import { Component, OnInit, signal, inject, DestroyRef } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../environments/environment';
import { PaginationComponent } from '../../../components/pagination/pagination.component';
import { Dropdown } from '../../../components/dropdown/dropdown';
import { ToastService } from '../../../services/toast.service';
import { untilDestroyed } from '../../../rxjs/until-destroyed';
import { AssignmentSubmissionResponse } from '../../../models/assignment';
import { FormsModule } from '@angular/forms';
import { Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';

@Component({
  selector: 'app-admin-assignments',
  standalone: true,
  imports: [CommonModule, PaginationComponent, Dropdown, FormsModule],
  templateUrl: './assignments.html',
  providers: [DatePipe]
})
export class AdminAssignmentsComponent implements OnInit {
  private http = inject(HttpClient);
  private destroyRef = inject(DestroyRef);
  private toast = inject(ToastService);
  private datePipe = inject(DatePipe);

  protected isLoading = signal(true);
  protected submissions = signal<AssignmentSubmissionResponse[]>([]);
  
  protected pageNumber = signal(1);
  protected pageSize = signal(10);
  protected totalCount = signal(0);
  protected totalPages = signal(0);
  
  protected filterStatus = signal<string>('');
  protected searchQuery = signal<string>('');
  protected selectedSubmission = signal<AssignmentSubmissionResponse | null>(null);

  private searchSubject = new Subject<string>();

  protected statusOptions = [
    { value: '', label: 'All Statuses' },
    { value: 'Submitted', label: 'Submitted' },
    { value: 'UnderReview', label: 'Under Review' },
    { value: 'Graded', label: 'Graded' }
  ];

  constructor() {
    this.searchSubject.pipe(
      debounceTime(400),
      distinctUntilChanged(),
      untilDestroyed(this.destroyRef)
    ).subscribe((query) => {
      this.searchQuery.set(query);
      this.pageNumber.set(1);
      this.loadSubmissions();
    });
  }

  ngOnInit() {
    this.loadSubmissions();
  }

  protected onFilterChange(status: string) {
    this.filterStatus.set(status);
    this.pageNumber.set(1);
    this.loadSubmissions();
  }

  protected onSearchInput(event: any) {
    this.searchSubject.next(event.target.value);
  }

  protected viewDetails(sub: AssignmentSubmissionResponse) {
    this.selectedSubmission.set(sub);
  }

  protected closeDetails() {
    this.selectedSubmission.set(null);
  }

  private loadSubmissions() {
    this.isLoading.set(true);
    const params: any = {
      page: this.pageNumber().toString(),
      pageSize: this.pageSize().toString()
    };

    if (this.filterStatus()) {
      params.status = this.filterStatus();
    }
    
    if (this.searchQuery()) {
      params.search = this.searchQuery();
    }

    this.http.get<any>(`${environment.apiUrl}/AssignmentSubmissions/admin/all`, { params })
      .pipe(untilDestroyed(this.destroyRef))
      .subscribe({
        next: (data) => {
          this.submissions.set(data.submissions || []);
          this.totalCount.set(data.totalCount || 0);
          this.totalPages.set(data.totalPages || 0);
          this.isLoading.set(false);
        },
        error: (err) => {
          console.error('Failed to load assignment submissions', err);
          this.toast.showError('Failed to load assignments');
          this.submissions.set([]);
          this.isLoading.set(false);
        }
      });
  }

  protected onPageChange(page: number) {
    this.pageNumber.set(page);
    this.loadSubmissions();
  }

  protected getStatusClass(status: string): string {
    switch (status?.toLowerCase()) {
      case 'graded':
        return 'bg-green-100 text-green-700 border border-green-200';
      case 'submitted':
      case 'underreview':
        return 'bg-orange-100 text-orange-700 border border-orange-200';
      case 'draft':
      case 'notsubmitted':
      default:
        return 'bg-slate-100 text-slate-600 border border-slate-200';
    }
  }
}
