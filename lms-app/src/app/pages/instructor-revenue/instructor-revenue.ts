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
  protected totalPages = signal(1);

  protected paginatedPayouts = computed(() => {
    return this.summary()?.payouts || [];
  });

  ngOnInit() {
    this.fetchRevenue();
  }

  private fetchRevenue() {
    this.isLoading.set(true);
    const search = this.searchQuery().trim() || undefined;
    const status = this.statusFilter() === 'All' ? undefined : this.statusFilter();
    
    this.revenueService.getInstructorRevenue(this.currentPage(), this.itemsPerPage(), search, status).subscribe({
      next: (res) => {
        this.summary.set(res);
        this.totalPages.set(res.totalPages || 1);
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
    this.fetchRevenue();
  }

  protected onSearchChange(value: string) {
    this.searchQuery.set(value);
    this.currentPage.set(1);
    this.fetchRevenue();
  }

  protected onStatusChange(value: string) {
    this.statusFilter.set(value);
    this.currentPage.set(1);
    this.fetchRevenue();
  }
}
