import { Component, OnInit, signal, computed, inject, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, Router } from '@angular/router';
import { ToastService } from '@services/toast.service';
import { QuizAttemptResponse, QuizAttemptDetailResponse } from '@models/quiz';
import { untilDestroyed } from '../../rxjs/until-destroyed';

import { Loader } from '@components/loader/loader';
import { PaginationComponent } from '@components/pagination/pagination.component';
import { QuizService } from '@services/quiz.service';

@Component({
  selector: 'app-quizzes-page',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, Loader, PaginationComponent],
  templateUrl: './quizzes.html'
})
export class QuizzesPage implements OnInit {
  private quizService = inject(QuizService);
  private toastService = inject(ToastService);
  private router = inject(Router);
  private destroyRef = inject(DestroyRef);

  protected attempts = signal<QuizAttemptResponse[]>([]);
  protected isLoading = signal(true);
  protected searchQuery = signal('');
  
  // Pagination State
  protected currentPage = signal(1);
  protected pageSize = signal(10);
  protected totalItems = signal(0);
  protected totalPages = signal(0);

  // Stats computed signals (These represent the current page now, we should fetch totals from another API or keep it simple)
  // For simplicity since the backend now returns pagination, we'll keep stats based on what we have or remove them.
  protected totalCount = computed(() => this.totalItems());
  protected passedCount = computed(() => this.attempts().filter(a => a.isPassed).length);
  protected failedCount = computed(() => this.attempts().filter(a => !a.isPassed && a.completedAt).length);
  protected inProgressCount = computed(() => this.attempts().filter(a => !a.completedAt).length);

  // Client-side search filtering - note: real search should ideally be server-side now that it is paginated
  protected filteredAttempts = computed(() => {
    let list = this.attempts();
    const query = this.searchQuery().toLowerCase().trim();

    if (query) {
      list = list.filter(a => 
        (a.quizTitle && a.quizTitle.toLowerCase().includes(query)) ||
        (a.courseTitle && a.courseTitle.toLowerCase().includes(query))
      );
    }
    return list;
  });

  ngOnInit(): void {
    this.loadAttempts();
  }

  protected loadAttempts(page: number = 1): void {
    this.isLoading.set(true);
    this.quizService.getMyQuizAttempts(page, this.pageSize())
      .pipe(untilDestroyed(this.destroyRef))
      .subscribe({
        next: (data) => {
          this.attempts.set(data?.attempts ?? []);
          this.currentPage.set(data?.pageNumber ?? 1);
          this.pageSize.set(data?.pageSize ?? 10);
          this.totalItems.set(data?.totalCount ?? 0);
          this.totalPages.set(data?.totalPages ?? 0);
          this.isLoading.set(false);
        },
        error: (err) => {
          this.toastService.showApiError(err, 'Failed to load quiz attempts.');
          this.isLoading.set(false);
        }
      });
  }

  protected onPageChange(page: number): void {
    this.loadAttempts(page);
  }

  protected viewAttemptDetail(attemptId: number): void {
    this.router.navigate(['/learner/quizzes', attemptId]);
  }

  protected navigateToExplore(): void {
    this.router.navigate(['/learner/explore']);
  }

  protected getDuration(startedAt: string, completedAt?: string): string {
    if (!completedAt) return 'In Progress';
    const start = new Date(startedAt).getTime();
    const end = new Date(completedAt).getTime();
    const diffMs = end - start;
    if (diffMs <= 0) return '0s';
    const diffMins = Math.floor(diffMs / 60000);
    const diffSecs = Math.floor((diffMs % 60000) / 1000);
    if (diffMins > 0) {
      return `${diffMins}m ${diffSecs}s`;
    }
    return `${diffSecs}s`;
  }

  protected getPercentage(obtained: number, total: number): number {
    if (total <= 0) return 0;
    return Math.round((obtained / total) * 100);
  }
}
