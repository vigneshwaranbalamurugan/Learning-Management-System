import { Component, OnInit, signal, computed, inject, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, Router } from '@angular/router';
import { ToastService } from '@services/toast.service';
import { QuizAttemptResponse, QuizAttemptDetailResponse } from '@models/quiz';
import { untilDestroyed } from '../../rxjs/until-destroyed';

import { Loader } from '@components/loader/loader';
import { QuizService } from '@services/quiz.service';

@Component({
  selector: 'app-quizzes-page',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, Loader],
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

  // Stats computed signals
  protected totalCount = computed(() => this.attempts().length);
  protected passedCount = computed(() => this.attempts().filter(a => a.isPassed).length);
  protected failedCount = computed(() => this.attempts().filter(a => !a.isPassed && a.completedAt).length);
  protected inProgressCount = computed(() => this.attempts().filter(a => !a.completedAt).length);

  // Client-side search filtering
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

  private loadAttempts(): void {
    this.isLoading.set(true);
    this.quizService.getMyQuizAttempts()
      .pipe(untilDestroyed(this.destroyRef))
      .subscribe({
        next: (data) => {
          this.attempts.set(data ?? []);
          this.isLoading.set(false);
        },
        error: (err) => {
          this.toastService.showApiError(err, 'Failed to load quiz attempts.');
          this.isLoading.set(false);
        }
      });
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
