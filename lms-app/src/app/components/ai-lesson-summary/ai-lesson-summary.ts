import {
  Component, OnInit, OnChanges, SimpleChanges,
  Input, signal, inject
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { AiService, AiSummaryResponse } from '@services/ai.service';

@Component({
  selector: 'app-ai-lesson-summary',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './ai-lesson-summary.html',
  styleUrl: './ai-lesson-summary.css'
})
export class AiLessonSummary implements OnInit, OnChanges {
  @Input() lessonId!: number;
  @Input() lessonType!: number; // LessonType enum value

  private aiService = inject(AiService);

  isLoading = signal(true);
  isSupported = signal(true);
  summary = signal<AiSummaryResponse | null>(null);
  errorMsg = signal('');

  // Polling handle for when status is "generating"
  private pollingTimer: any = null;
  private pollingInterval = 8000; // 8 seconds

  ngOnInit(): void {
    this.load();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['lessonId'] && !changes['lessonId'].firstChange) {
      this.stopPolling();
      this.isLoading.set(true);
      this.summary.set(null);
      this.errorMsg.set('');
      this.load();
    }
  }

  ngOnDestroy(): void {
    this.stopPolling();
  }

  private load(): void {
    // ExternalLink = 3
    if (this.lessonType === 3) {
      this.isSupported.set(false);
      this.isLoading.set(false);
      return;
    }
    this.isSupported.set(true);
    this.fetchSummary();
  }

  private fetchSummary(): void {
    this.aiService.getLessonSummary(this.lessonId).subscribe({
      next: (res) => {
        this.summary.set(res);
        this.isLoading.set(false);

        if (res.status === 'generating') {
          this.startPolling();
        } else {
          this.stopPolling();
        }
      },
      error: (err) => {
        this.isLoading.set(false);
        this.errorMsg.set('Could not load AI summary. Please try again later.');
        this.stopPolling();
      }
    });
  }

  private startPolling(): void {
    this.stopPolling();
    this.pollingTimer = setInterval(() => this.fetchSummary(), this.pollingInterval);
  }

  private stopPolling(): void {
    if (this.pollingTimer) {
      clearInterval(this.pollingTimer);
      this.pollingTimer = null;
    }
  }
}
