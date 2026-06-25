import { Component, OnInit, signal, inject, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { ToastService } from '@services/toast.service';
import { QuizAttemptDetailResponse } from '@models/quiz';
import { untilDestroyed } from '../../rxjs/until-destroyed';
import { QuizService } from '@services/quiz.service';

@Component({
  selector: 'app-quizzes-detail',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './quizzes-detail.html'
})
export class QuizDetailPage implements OnInit {
  private quizService = inject(QuizService);
  private toastService = inject(ToastService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private destroyRef = inject(DestroyRef);

  protected attempt = signal<QuizAttemptDetailResponse | null>(null);
  protected isLoading = signal(true);

  ngOnInit(): void {
    this.route.paramMap
      .pipe(untilDestroyed(this.destroyRef))
      .subscribe(params => {
        const id = params.get('id');
        if (id) {
          this.loadAttemptDetail(Number(id));
        } else {
          this.router.navigate(['/learner/quizzes']);
        }
      });
  }

  private loadAttemptDetail(attemptId: number): void {
    this.isLoading.set(true);
    this.quizService.getQuizAttemptDetail(attemptId)
      .pipe(untilDestroyed(this.destroyRef))
      .subscribe({
        next: (data) => {
          this.attempt.set(data);
          this.isLoading.set(false);
        },
        error: (err) => {
          this.toastService.showApiError(err, 'Failed to load quiz attempt details.');
          this.isLoading.set(false);
          this.router.navigate(['/learner/quizzes']);
        }
      });
  }

  protected getSelectedOptionId(questionId: number): number | null {
    const detail = this.attempt();
    if (!detail) return null;
    const ans = detail.answers.find(a => a.questionId === questionId);
    return ans ? ans.selectedOptionId : null;
  }

  protected isSelectedOption(questionId: number, optionId: number): boolean {
    return this.getSelectedOptionId(questionId) === optionId;
  }

  protected goBack(): void {
    this.router.navigate(['/learner/quizzes']);
  }

  protected getDuration(startedAt?: string, completedAt?: string): string {
    if (!startedAt) return 'N/A';
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

  protected getPercentage(obtained?: number, total?: number): number {
    if (!obtained || !total || total <= 0) return 0;
    return Math.round((obtained / total) * 100);
  }
}
