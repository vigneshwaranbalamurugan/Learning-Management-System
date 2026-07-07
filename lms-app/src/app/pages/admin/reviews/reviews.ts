import { Component, OnInit, signal, inject, DestroyRef, effect, untracked } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../environments/environment';
import { PaginationComponent } from '../../../components/pagination/pagination.component';
import { ToastService } from '../../../services/toast.service';
import { untilDestroyed } from '../../../rxjs/until-destroyed';
import { ReviewResponse } from '../../../models/review';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-admin-reviews',
  standalone: true,
  imports: [CommonModule, PaginationComponent, FormsModule],
  templateUrl: './reviews.html',
  providers: [DatePipe]
})
export class AdminReviewsComponent implements OnInit {
  private http = inject(HttpClient);
  private destroyRef = inject(DestroyRef);
  private toast = inject(ToastService);
  private datePipe = inject(DatePipe);

  protected isLoading = signal(true);
  protected reviews = signal<ReviewResponse[]>([]);
  
  protected pageNumber = signal(1);
  protected pageSize = signal(10);
  protected totalCount = signal(0);
  protected totalPages = signal(0);
  protected searchQuery = signal('');

  protected filterStatus = signal<string>('All');
  protected filterRating = signal<string>('All');

  constructor() {}

  ngOnInit() {
    this.loadReviews();
  }

  protected onSearch() {
    this.pageNumber.set(1);
    this.loadReviews();
  }

  protected onFilterChange() {
    this.pageNumber.set(1);
    this.loadReviews();
  }

  private loadReviews() {
    this.isLoading.set(true);
    const params: any = {
      page: this.pageNumber().toString(),
      pageSize: this.pageSize().toString()
    };

    if (this.searchQuery().trim()) {
      params.search = this.searchQuery().trim();
    }
    if (this.filterStatus() !== 'All') {
      params.status = this.filterStatus();
    }
    if (this.filterRating() !== 'All') {
      params.rating = this.filterRating();
    }

    this.http.get<any>(`${environment.apiUrl}/Reviews/admin/all`, { params })
      .pipe(untilDestroyed(this.destroyRef))
      .subscribe({
        next: (data) => {
          this.reviews.set(data.reviews || []);
          this.totalCount.set(data.totalCount || 0);
          this.totalPages.set(data.totalPages || 0);
          this.isLoading.set(false);
        },
        error: (err) => {
          console.error('Failed to load reviews', err);
          this.toast.showError('Failed to load reviews');
          this.reviews.set([]);
          this.isLoading.set(false);
        }
      });
  }

  protected onPageChange(page: number) {
    this.pageNumber.set(page);
    this.loadReviews();
  }

  protected deleteReview(id: number) {
    if (confirm('Are you sure you want to delete this review?')) {
      this.http.delete(`${environment.apiUrl}/Reviews/admin/${id}`)
        .subscribe({
          next: () => {
            this.toast.showSuccess('Review deleted successfully');
            this.loadReviews();
          },
          error: (err) => {
            console.error(err);
            this.toast.showError('Failed to delete review');
          }
        });
    }
  }

  protected restoreReview(id: number) {
    if (confirm('Are you sure you want to restore this deleted review?')) {
      this.http.put(`${environment.apiUrl}/Reviews/admin/${id}/restore`, {})
        .subscribe({
          next: () => {
            this.toast.showSuccess('Review restored successfully');
            this.loadReviews();
          },
          error: (err) => {
            console.error(err);
            this.toast.showError('Failed to restore review');
          }
        });
    }
  }
}
