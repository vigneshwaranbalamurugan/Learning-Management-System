import { Component, OnInit, signal, inject, DestroyRef, effect, untracked } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { untilDestroyed } from '../../../rxjs/until-destroyed';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../environments/environment';
import { PaginationComponent } from '../../../components/pagination/pagination.component';
import { FormInput } from '../../../components/form-input/form-input';
import { Dropdown } from '../../../components/dropdown/dropdown';
import { ActivatedRoute, Router } from '@angular/router';

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
  private route = inject(ActivatedRoute);
  private router = inject(Router);

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
    this.route.queryParams.pipe(untilDestroyed(this.destroyRef)).subscribe(params => {
      if (params['tab'] === 'audit' || params['tab'] === 'activity') {
        this.activeTab.set(params['tab']);
      }
      if (params['page']) {
        this.pageNumber.set(parseInt(params['page'], 10) || 1);
      }
      
      this.filterUserId.set(params['userQuery'] || '');
      this.filterActivityType.set(params['activityType'] || '');
      this.filterTableName.set(params['tableName'] || '');
      this.filterAction.set(params['action'] || '');

      this.loadLogs();
    });
  }

  protected switchTab(tab: 'activity' | 'audit') {
    if (this.activeTab() !== tab) {
      this.router.navigate([], {
        relativeTo: this.route,
        queryParams: { tab: tab, page: 1, userQuery: null, activityType: null, tableName: null, action: null },
        queryParamsHandling: 'merge'
      });
    }
  }

  protected applyFilters() {
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: {
        page: 1,
        userQuery: this.filterUserId() || null,
        activityType: this.activeTab() === 'activity' ? (this.filterActivityType() || null) : null,
        tableName: this.activeTab() === 'audit' ? (this.filterTableName() || null) : null,
        action: this.activeTab() === 'audit' ? (this.filterAction() || null) : null,
      },
      queryParamsHandling: 'merge'
    });
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
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: {
        page: 1,
        userQuery: null,
        activityType: null,
        tableName: null,
        action: null
      },
      queryParamsHandling: 'merge'
    });
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
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { page: page },
      queryParamsHandling: 'merge'
    });
  }
  
  protected viewAuditLog(id: number) {
    if (this.activeTab() === 'audit') {
      this.router.navigate(['/admin/logs/audit', id]);
    }
  }
}
