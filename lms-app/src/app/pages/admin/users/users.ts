import { Component, OnInit, inject, signal, effect, untracked } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { AdminUserService } from '../../../services/admin-user.service';
import { AdminUserResponse, UserSearchQuery } from '../../../models/admin-user';
import { Button } from '../../../components/button/button';
import { Dropdown } from '../../../components/dropdown/dropdown';
import { PaginationComponent } from '../../../components/pagination/pagination.component';
import { CreateUserModal } from './create-user-modal';

@Component({
  selector: 'app-admin-users',
  standalone: true,
  imports: [CommonModule, Button, Dropdown, PaginationComponent, CreateUserModal],
  templateUrl: './users.html'
})
export class AdminUsersComponent implements OnInit {
  private adminUserService = inject(AdminUserService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);

  // State
  users = signal<AdminUserResponse[]>([]);
  totalUsers = signal(0);
  totalPages = signal(1);
  isLoading = signal(true);
  isCreateModalOpen = signal(false);

  // Filters
  searchQuery = signal('');
  pageNumber = signal(1);
  pageSize = signal(10);
  filterRole = signal<string>('');
  filterStatus = signal<string>('');

  // Options
  roleOptions = [
    { value: '', label: 'All Roles' },
    { value: '1', label: 'Learner' },
    { value: '2', label: 'Instructor' },
    { value: '3', label: 'Admin' }
  ];

  statusOptions = [
    { value: '', label: 'All Statuses' },
    { value: 'true', label: 'Active' },
    { value: 'false', label: 'Inactive' }
  ];

  protected Math = Math;

  constructor() {
    effect(() => {
      const page = this.pageNumber();
      const search = this.searchQuery();
      const role = this.filterRole();
      const status = this.filterStatus();

      untracked(() => {
        this.updateUrlParams();
        this.fetchUsers();
      });
    });
  }

  ngOnInit(): void {
    // Read initial URL params
    const params = this.route.snapshot.queryParams;
    
    if (params['page']) this.pageNumber.set(+params['page']);
    if (params['search']) this.searchQuery.set(params['search']);
    if (params['roleId']) this.filterRole.set(params['roleId']);
    
    if (params['isActive'] !== undefined) {
      this.filterStatus.set(params['isActive']);
    }
  }

  fetchUsers() {
    this.isLoading.set(true);
    const query: UserSearchQuery = {
      pageNumber: this.pageNumber(),
      pageSize: this.pageSize(),
      search: this.searchQuery() || undefined,
      roleId: this.filterRole() ? +this.filterRole() : null,
      isActive: this.filterStatus() ? this.filterStatus() === 'true' : null
    };

    this.adminUserService.getUsers(query).subscribe({
      next: (res) => {
        this.users.set(res.users);
        this.totalUsers.set(res.totalCount);
        this.totalPages.set(res.totalPages);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Failed to fetch users', err);
        this.isLoading.set(false);
      }
    });
  }

  updateUrlParams() {
    const queryParams: any = {};
    
    if (this.pageNumber() > 1) queryParams.page = this.pageNumber();
    if (this.searchQuery()) queryParams.search = this.searchQuery();
    if (this.filterRole()) queryParams.roleId = this.filterRole();
    if (this.filterStatus()) queryParams.isActive = this.filterStatus();

    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: queryParams,
      replaceUrl: true
    });
  }

  onSearch(event: Event) {
    const value = (event.target as HTMLInputElement).value;
    this.searchQuery.set(value);
    this.pageNumber.set(1);
  }

  onFilterRoleChange(roleId: any) {
    this.filterRole.set(roleId);
    this.pageNumber.set(1);
  }

  onFilterStatusChange(status: any) {
    this.filterStatus.set(status);
    this.pageNumber.set(1);
  }

  onPageChange(page: number) {
    this.pageNumber.set(page);
  }

  hasActiveFilters(): boolean {
    return !!this.searchQuery() || !!this.filterRole() || !!this.filterStatus();
  }

  clearFilters() {
    this.searchQuery.set('');
    this.filterRole.set('');
    this.filterStatus.set('');
    this.pageNumber.set(1);
  }

  openCreateModal() {
    this.isCreateModalOpen.set(true);
  }

  closeCreateModal() {
    this.isCreateModalOpen.set(false);
  }

  onUserCreated() {
    this.closeCreateModal();
    this.fetchUsers();
  }
}
