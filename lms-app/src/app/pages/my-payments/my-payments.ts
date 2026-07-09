import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { PaymentService, LearnerPayment } from '@services/payment.service';
import { ToastService } from '@services/toast.service';
import { Dropdown } from '@components/dropdown/dropdown';
import { FormInput } from '@components/form-input/form-input';
import { PaginationComponent } from '@components/pagination/pagination.component';

@Component({
  selector: 'app-my-payments',
  standalone: true,
  imports: [CommonModule, DatePipe, Dropdown, FormInput, PaginationComponent],
  templateUrl: './my-payments.html',
})
export class MyPaymentsPage implements OnInit {
  private paymentService = inject(PaymentService);
  private toastService = inject(ToastService);

  payments = signal<LearnerPayment[]>([]);
  isLoading = signal<boolean>(true);
  downloadingInvoiceId = signal<number | null>(null);

  searchTerm = signal<string>('');
  statusFilter = signal<string>('');
  currentPage = signal<number>(1);
  totalPages = signal<number>(1);
  totalCount = signal<number>(0);

  statusOptions = [
    { value: '', label: 'All Statuses' },
    { value: 'Completed', label: 'Completed' },
    { value: 'Pending', label: 'Pending' },
    { value: 'Failed', label: 'Failed' },
    { value: 'Transferred', label: 'Transferred' },
  ];

  ngOnInit() {
    this.loadPayments();
  }

  loadPayments() {
    this.isLoading.set(true);
    this.paymentService.getMyPayments(
      this.searchTerm(),
      this.statusFilter(),
      this.currentPage(),
      10
    ).subscribe({
      next: (res) => {
        this.payments.set(res.items);
        this.totalCount.set(res.totalCount);
        this.totalPages.set(res.totalPages);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Failed to load payments:', err);
        this.toastService.showError('Failed to load payments');
        this.isLoading.set(false);
      }
    });
  }

  onSearch(term: string) {
    this.searchTerm.set(term);
    this.currentPage.set(1);
    this.loadPayments();
  }

  onStatusChange(status: string) {
    this.statusFilter.set(status);
    this.currentPage.set(1);
    this.loadPayments();
  }

  onPageChange(page: number) {
    this.currentPage.set(page);
    this.loadPayments();
  }

  downloadInvoice(payment: LearnerPayment) {
    if (this.downloadingInvoiceId()) return;

    this.downloadingInvoiceId.set(payment.id);
    this.paymentService.downloadInvoice(payment.id).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        const timestamp = new Date().getTime();
        a.download = `${payment.invoiceNumber}_${timestamp}.pdf`;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        window.URL.revokeObjectURL(url);
        this.downloadingInvoiceId.set(null);
      },
      error: (err) => {
        console.error('Failed to download invoice:', err);
        this.toastService.showApiError(err, 'Failed to download invoice. Make sure the payment is completed.');
        this.downloadingInvoiceId.set(null);
      }
    });
  }

  getStatusClass(status: string): string {
    const s = status.toLowerCase();
    if (s === 'completed' || s === 'transferred') return 'bg-green-100 text-green-700';
    if (s === 'pending') return 'bg-yellow-100 text-yellow-700';
    if (s === 'failed') return 'bg-red-100 text-red-700';
    return 'bg-gray-100 text-gray-700';
  }
}
