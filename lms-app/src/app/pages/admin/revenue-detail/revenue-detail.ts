import { Component, OnInit, signal, inject, computed } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdminRevenueService } from '@services/admin-revenue.service';
import { ToastService } from '@services/toast.service';
import {
  AdminRevenueSummary, AdminTransactionItem, AdminPayoutItem,
  PagedAdminTransactionResponse, PagedAdminPayoutResponse, RevenueFilters
} from '@models/admin-revenue';
import { Dropdown } from '@components/dropdown/dropdown';

type Tab = 'transactions' | 'payouts' | 'balances';

@Component({
  selector: 'app-admin-revenue-detail',
  standalone: true,
  imports: [CommonModule, FormsModule, Dropdown],
  templateUrl: './revenue-detail.html',
  providers: [DatePipe]
})
export class AdminRevenueDetail implements OnInit {
  private revenueService = inject(AdminRevenueService);
  private toast = inject(ToastService);

  // --- Summary KPIs ---
  protected summary = signal<AdminRevenueSummary | null>(null);
  protected summaryLoading = signal(true);

  // --- Tab ---
  protected activeTab = signal<Tab>('transactions');

  // --- Transactions ---
  protected txData = signal<PagedAdminTransactionResponse | null>(null);
  protected txLoading = signal(false);
  protected txFilters: RevenueFilters = {
    search: '', status: '', dateFrom: '', dateTo: '', page: 1, pageSize: 15
  };
  protected txSearchInput = '';
  protected txStatusFilter = '';
  protected txDateFrom = '';
  protected txDateTo = '';

  protected txStatusOptions = [
    { value: '', label: 'All Statuses' },
    { value: 'Pending', label: 'Pending' },
    { value: 'Completed', label: 'Completed' },
    { value: 'Transferred', label: 'Transferred' },
    { value: 'Failed', label: 'Failed' },
    { value: 'Refunded', label: 'Refunded' }
  ];

  // --- Payouts ---
  protected payoutData = signal<PagedAdminPayoutResponse | null>(null);
  protected payoutLoading = signal(false);
  protected payoutFilters: RevenueFilters = {
    search: '', status: '', dateFrom: '', dateTo: '', page: 1, pageSize: 15
  };
  protected payoutSearchInput = '';
  protected payoutStatusFilter = '';
  protected payoutDateFrom = '';
  protected payoutDateTo = '';

  protected payoutStatusOptions = [
    { value: '', label: 'All Statuses' },
    { value: 'Pending', label: 'Pending' },
    { value: 'Queued', label: 'Queued' },
    { value: 'Processing', label: 'Processing' },
    { value: 'Processed', label: 'Processed' },
    { value: 'Failed', label: 'Failed' },
    { value: 'Reversed', label: 'Reversed' },
    { value: 'PendingManualReview', label: 'Manual Review' }
  ];

  // --- Computed net revenue ---
  protected netRevenue = computed(() => {
    const s = this.summary();
    if (!s) return 0;
    return s.totalPlatformFees - 0; // platform keeps what's retained
  });

  ngOnInit() {
    this.loadSummary();
    this.loadTransactions();
  }

  // ================================================
  // SUMMARY
  // ================================================
  private loadSummary() {
    this.summaryLoading.set(true);
    this.revenueService.getSummary().subscribe({
      next: (data) => {
        this.summary.set(data);
        this.summaryLoading.set(false);
      },
      error: () => this.summaryLoading.set(false)
    });
  }

  // ================================================
  // TAB SWITCH
  // ================================================
  protected switchTab(tab: Tab) {
    this.activeTab.set(tab);
    if (tab === 'transactions' && !this.txData()) {
      this.loadTransactions();
    } else if (tab === 'payouts' && !this.payoutData()) {
      this.loadPayouts();
    }
  }

  // ================================================
  // TRANSACTIONS
  // ================================================
  private loadTransactions() {
    this.txLoading.set(true);
    this.revenueService.getTransactions(this.txFilters).subscribe({
      next: (data) => {
        this.txData.set(data);
        this.txLoading.set(false);
      },
      error: (err) => {
        this.toast.showApiError(err, 'Failed to load transactions');
        this.txLoading.set(false);
      }
    });
  }

  protected applyTxFilters() {
    this.txFilters = {
      ...this.txFilters,
      search: this.txSearchInput,
      status: this.txStatusFilter,
      dateFrom: this.txDateFrom,
      dateTo: this.txDateTo,
      page: 1
    };
    this.loadTransactions();
  }

  protected clearTxFilters() {
    this.txSearchInput = '';
    this.txStatusFilter = '';
    this.txDateFrom = '';
    this.txDateTo = '';
    this.txFilters = { search: '', status: '', dateFrom: '', dateTo: '', page: 1, pageSize: 15 };
    this.loadTransactions();
  }

  protected txPageChange(page: number) {
    this.txFilters = { ...this.txFilters, page };
    this.loadTransactions();
  }

  protected get txPages(): number[] {
    const total = this.txData()?.totalPages ?? 0;
    return Array.from({ length: total }, (_, i) => i + 1);
  }

  protected exportTransactionsCsv() {
    const items = this.txData()?.items ?? [];
    if (!items.length) return;

    const headers = ['ID', 'Date', 'Learner', 'Email', 'Course', 'Instructor', 'Gross (₹)', 'Platform Fee (₹)', 'Instructor Share (₹)', 'Status', 'Payment ID'];
    const rows = items.map(t => [
      t.id,
      t.paidAt ? new Date(t.paidAt).toLocaleDateString() : new Date(t.createdAt).toLocaleDateString(),
      t.learnerName,
      t.learnerEmail,
      t.courseName,
      t.instructorName,
      t.grossAmount.toFixed(2),
      t.platformFeeAmount.toFixed(2),
      t.instructorAmount.toFixed(2),
      t.status,
      t.providerPaymentId ?? '-'
    ]);

    this.downloadCsv('transactions.csv', headers, rows);
  }

  // ================================================
  // PAYOUTS
  // ================================================
  private loadPayouts() {
    this.payoutLoading.set(true);
    this.revenueService.getPayouts(this.payoutFilters).subscribe({
      next: (data) => {
        this.payoutData.set(data);
        this.payoutLoading.set(false);
      },
      error: (err) => {
        this.toast.showApiError(err, 'Failed to load payouts');
        this.payoutLoading.set(false);
      }
    });
  }

  protected applyPayoutFilters() {
    this.payoutFilters = {
      ...this.payoutFilters,
      search: this.payoutSearchInput,
      status: this.payoutStatusFilter,
      dateFrom: this.payoutDateFrom,
      dateTo: this.payoutDateTo,
      page: 1
    };
    this.loadPayouts();
  }

  protected clearPayoutFilters() {
    this.payoutSearchInput = '';
    this.payoutStatusFilter = '';
    this.payoutDateFrom = '';
    this.payoutDateTo = '';
    this.payoutFilters = { search: '', status: '', dateFrom: '', dateTo: '', page: 1, pageSize: 15 };
    this.loadPayouts();
  }

  protected payoutPageChange(page: number) {
    this.payoutFilters = { ...this.payoutFilters, page };
    this.loadPayouts();
  }

  protected get payoutPages(): number[] {
    const total = this.payoutData()?.totalPages ?? 0;
    return Array.from({ length: total }, (_, i) => i + 1);
  }

  protected exportPayoutsCsv() {
    const items = this.payoutData()?.items ?? [];
    if (!items.length) return;

    const headers = ['ID', 'Date', 'Instructor', 'Email', 'Course', 'Learner', 'Amount (₹)', 'Status', 'Transfer ID', 'Failure Reason'];
    const rows = items.map(p => [
      p.id,
      new Date(p.createdAt).toLocaleDateString(),
      p.instructorName,
      p.instructorEmail,
      p.courseName,
      p.learnerName ?? '-',
      p.amount.toFixed(2),
      p.status,
      p.razorpayTransferId ?? '-',
      p.failureReason ?? '-'
    ]);

    this.downloadCsv('payouts.csv', headers, rows);
  }

  // ================================================
  // HELPERS
  // ================================================
  protected getStatusClass(status: string): string {
    const s = status.toLowerCase();
    if (s === 'completed' || s === 'transferred' || s === 'processed') return 'bg-green-100 text-green-700 border-green-200';
    if (s === 'pending' || s === 'queued' || s === 'processing') return 'bg-yellow-100 text-yellow-700 border-yellow-200';
    if (s === 'failed' || s === 'reversed') return 'bg-red-100 text-red-700 border-red-200';
    if (s === 'pendingmanualreview') return 'bg-orange-100 text-orange-700 border-orange-200';
    if (s === 'refunded') return 'bg-slate-100 text-slate-600 border-slate-200';
    return 'bg-slate-100 text-slate-600 border-slate-200';
  }

  protected formatStatus(status: string): string {
    if (status === 'PendingManualReview') return 'Manual Review';
    return status;
  }

  protected getMin(a: number, b: number): number {
    return Math.min(a, b);
  }

  private downloadCsv(filename: string, headers: string[], rows: any[][]) {
    const csvContent = [headers, ...rows]
      .map(row => row.map(v => `"${String(v).replace(/"/g, '""')}"`).join(','))
      .join('\n');
    const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = filename;
    a.click();
    URL.revokeObjectURL(url);
  }
}
