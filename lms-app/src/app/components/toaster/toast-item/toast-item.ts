import { Component, Input, OnInit, OnDestroy, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Toast } from '../../../models/toast';
import { ToastService } from '../../../services/toast.service';

@Component({
  selector: 'app-toast-item',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './toast-item.html',
  styleUrl: './toast-item.css',
})
export class ToastItemComponent implements OnInit, OnDestroy {
  @Input({ required: true }) toast!: Toast;

  protected isDismissing = false;
  protected progressWidth = signal<number>(100);

  private animationFrameId: any = null;
  private timeRemaining = 3000;
  private lastTickTime = 0;
  private isPaused = false;

  constructor(private toastService: ToastService) { }

  ngOnInit() {
    this.timeRemaining = this.toast.duration || 3000;
    this.startTimer();
  }

  ngOnDestroy() {
    this.clearTimers();
  }

  private startTimer() {
    this.lastTickTime = performance.now();
    this.isPaused = false;

    const tick = (now: number) => {
      if (!this.isPaused) {
        const delta = now - this.lastTickTime;
        this.lastTickTime = now;

        this.timeRemaining -= delta;
        if (this.timeRemaining <= 0) {
          this.timeRemaining = 0;
          this.progressWidth.set(0);
          this.dismiss();
          return;
        }

        const total = this.toast.duration || 3000;
        this.progressWidth.set((this.timeRemaining / total) * 100);
      } else {
        this.lastTickTime = now;
      }
      this.animationFrameId = requestAnimationFrame(tick);
    };

    this.animationFrameId = requestAnimationFrame(tick);
  }

  protected pauseTimer() {
    this.isPaused = true;
  }

  protected resumeTimer() {
    this.isPaused = false;
    this.lastTickTime = performance.now();
  }

  protected dismiss() {
    if (this.isDismissing) return;
    this.isDismissing = true;
    this.clearTimers();
    setTimeout(() => {
      this.toastService.dismiss(this.toast.id);
    }, 300); // Wait for the fade-out animation to complete
  }

  private clearTimers() {
    if (this.animationFrameId) {
      cancelAnimationFrame(this.animationFrameId);
      this.animationFrameId = null;
    }
  }
}
