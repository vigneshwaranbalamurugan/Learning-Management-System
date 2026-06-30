import { Component, OnInit, signal, inject, DestroyRef, effect, untracked } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { untilDestroyed } from '../../../rxjs/until-destroyed';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../environments/environment';
import { PaginationComponent } from '../../../components/pagination/pagination.component';
import { FormInput } from '../../../components/form-input/form-input';
import { Dropdown } from '../../../components/dropdown/dropdown';

@Component({
  selector: 'app-admin-logs',
  standalone: true,
  imports: [CommonModule, PaginationComponent, FormInput, Dropdown],
  templateUrl: './logs.html',
  providers: [DatePipe]
})
export class AdminLogs implements OnInit {
  private http = inject(HttpClient);
  private destroyRef = inject(DestroyRef);
  private datePipe = inject(DatePipe);

  protected isLoading = signal(true);
  protected logs = signal<any[]>([]);
  
  protected activeTab = signal<'activity' | 'audit'>('activity');

  protected pageNumber = signal(1);
  protected pageSize = signal(10);
  protected totalCount = signal(0);
  protected totalPages = signal(0);

  // Filters
  protected filterUserId = signal('');
  protected filterActivityType = signal('');
  protected filterTableName = signal('');
  protected filterAction = signal('');

  // Dropdown options
  protected activityTypeOptions = [
    { value: '', label: 'All Activities' },
    { value: 'UserRegister', label: 'User Register' },
    { value: 'UserLogin', label: 'User Login' },
    { value: 'CourseCreated', label: 'Course Created' },
    { value: 'CoursePublished', label: 'Course Published' },
    { value: 'CourseEnrollment', label: 'Course Enrollment' },
    { value: 'QuizAttemptStarted', label: 'Quiz Attempt Started' },
    { value: 'QuizAttemptSubmitted', label: 'Quiz Attempt Submitted' },
    { value: 'AssignmentSubmitted', label: 'Assignment Submitted' },
    { value: 'AssignmentGraded', label: 'Assignment Graded' },
    { value: 'CertificateIssued', label: 'Certificate Issued' },
    { value: 'PaymentSuccess', label: 'Payment Success' },
    { value: 'PaymentFailed', label: 'Payment Failed' },
    { value: 'BatchAnnouncementCreated', label: 'Batch Announcement Created' },
    { value: 'DiscussionPostCreated', label: 'Discussion Post Created' }
  ];

  protected tableNameOptions = [
    { value: '', label: 'All Tables' },
    { value: 'Users', label: 'Users' },
    { value: 'Courses', label: 'Courses' },
    { value: 'Lessons', label: 'Lessons' },
    { value: 'Assignments', label: 'Assignments' },
    { value: 'Quizzes', label: 'Quizzes' },
    { value: 'Reviews', label: 'Reviews' },
    { value: 'Payments', label: 'Payments' },
    { value: 'Certificates', label: 'Certificates' }
  ];

  protected actionOptions = [
    { value: '', label: 'All Actions' },
    { value: 'Insert', label: 'Insert' },
    { value: 'Update', label: 'Update' },
    { value: 'Delete', label: 'Delete' }
  ];

  private searchTimeout: any;

  constructor() {}

  ngOnInit() {
    this.loadLogs();
  }

  protected switchTab(tab: 'activity' | 'audit') {
    if (this.activeTab() !== tab) {
      this.activeTab.set(tab);
      this.clearFilters();
    }
  }

  protected applyFilters() {
    this.pageNumber.set(1);
    this.loadLogs();
  }

  protected onFilterChange() {
    if (this.searchTimeout) {
      clearTimeout(this.searchTimeout);
    }
    this.searchTimeout = setTimeout(() => {
      this.applyFilters();
    }, 400);
  }

  protected clearFilters() {
    this.filterUserId.set('');
    this.filterActivityType.set('');
    this.filterTableName.set('');
    this.filterAction.set('');
    this.pageNumber.set(1);
    this.loadLogs();
  }

  private loadLogs() {
    this.isLoading.set(true);
    const endpoint = this.activeTab() === 'activity' ? 'activity' : 'audit';
    const params: any = {
      page: this.pageNumber().toString(),
      pageSize: this.pageSize().toString()
    };

    if (this.filterUserId()) params.userQuery = this.filterUserId();

    if (this.activeTab() === 'activity') {
      if (this.filterActivityType()) params.activityType = this.filterActivityType();
    } else {
      if (this.filterTableName()) params.tableName = this.filterTableName();
      if (this.filterAction()) params.action = this.filterAction();
    }

    this.http.get<any>(`${environment.apiUrl}/Logs/${endpoint}`, { params })
      .pipe(untilDestroyed(this.destroyRef))
      .subscribe({
        next: (data) => {
          this.logs.set(data.logs || data.items || []);
          this.totalCount.set(data.totalCount || 0);
          this.totalPages.set(data.totalPages || 0);
          this.isLoading.set(false);
        },
        error: (err) => {
          console.error('Failed to load logs', err);
          this.logs.set([]);
          this.isLoading.set(false);
        }
      });
  }

  protected onPageChange(page: number) {
    this.pageNumber.set(page);
    this.loadLogs();
  }
}
