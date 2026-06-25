import { Component, inject, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RevenueService } from '@services/revenue.service';
import { InstructorRevenueSummaryResponse, InstructorPayoutResponse } from '@models/revenue';
import { PaginationComponent } from '@components/pagination/pagination.component';
import { Loader } from '@components/loader/loader';
import { FormInput } from '@components/form-input/form-input';
import { Dropdown } from '@components/dropdown/dropdown';
import { Button } from '@components/button/button';

@Component({
  selector: 'app-instructor-revenue',
  standalone: true,
  imports: [CommonModule, FormsModule, PaginationComponent, Loader, FormInput, Dropdown, Button],
  templateUrl: './instructor-revenue.html'
})
export class InstructorRevenue implements OnInit {
  private revenueService = inject(RevenueService);

  protected isLoading = signal(true);
  protected summary = signal<InstructorRevenueSummaryResponse | null>(null);

  // Filters
  protected searchQuery = signal('');
  protected statusFilter = signal('All');
  protected statusOptions = [
    { value: 'All', label: 'All Statuses' },
    { value: 'Processed', label: 'Processed' },
    { value: 'Pending', label: 'Pending' },
    { value: 'Failed', label: 'Failed' }
  ];

  // Pagination
  protected currentPage = signal(1);
  protected itemsPerPage = signal(10);

  protected filteredPayouts = computed(() => {
    const data = this.summary()?.payouts || [];
    const query = this.searchQuery().toLowerCase();
    const status = this.statusFilter();

    return data.filter(p => {
      const matchesSearch = (p.courseName?.toLowerCase() || '').includes(query) || (p.studentName?.toLowerCase() || '').includes(query);
      const matchesStatus = status === 'All' || (p.status?.toLowerCase() || '') === status.toLowerCase();
      return matchesSearch && matchesStatus;
    }).sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime());
  });

  protected paginatedPayouts = computed(() => {
    const start = (this.currentPage() - 1) * this.itemsPerPage();
    return this.filteredPayouts().slice(start, start + this.itemsPerPage());
  });

  protected totalPages = computed(() => {
    return Math.ceil(this.filteredPayouts().length / this.itemsPerPage()) || 1;
  });

  ngOnInit() {
    this.fetchRevenue();
  }

  private fetchRevenue() {
    this.isLoading.set(true);
    this.revenueService.getInstructorRevenue().subscribe({
      next: (res) => {
        this.summary.set(res);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Failed to load revenue', err);
        this.isLoading.set(false);
      }
    });
  }

  protected onPageChange(page: number) {
    this.currentPage.set(page);
  }

  protected onSearchChange(value: string) {
    this.searchQuery.set(value);
    this.currentPage.set(1);
  }

  protected onStatusChange(value: string) {
    this.statusFilter.set(value);
    this.currentPage.set(1);
  }
}
