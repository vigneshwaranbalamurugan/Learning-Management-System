import {
  Component,
  OnInit,
  OnDestroy,
  inject,
  signal,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { Subject, takeUntil } from 'rxjs';
import { SignalRService } from '@services/signalr.service';
import { Notification, NotificationType } from '@models/notification';

interface PopupNotification extends Notification {
  popupId: string;
  isLeaving: boolean;
}

const AUTO_DISMISS_MS = 5000;

@Component({
  selector: 'app-notification-popup',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './notification-popup.html',
  styleUrl: './notification-popup.css',
})
export class NotificationPopup implements OnInit, OnDestroy {
  private signalrService = inject(SignalRService);
  private router = inject(Router);
  private destroy$ = new Subject<void>();

  protected popups = signal<PopupNotification[]>([]);

  ngOnInit() {
    this.signalrService.notification$
      .pipe(takeUntil(this.destroy$))
      .subscribe((notification) => {
        this.addPopup(notification);
      });
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private addPopup(notification: Notification) {
    const popup: PopupNotification = {
      ...notification,
      popupId: `popup-${Date.now()}-${Math.random()}`,
      isLeaving: false,
    };

    this.popups.update((prev) => [...prev, popup]);

    // Auto-dismiss after timeout
    setTimeout(() => this.dismiss(popup.popupId), AUTO_DISMISS_MS);
  }

  protected dismiss(popupId: string) {
    // Trigger leave animation
    this.popups.update((prev) =>
      prev.map((p) => (p.popupId === popupId ? { ...p, isLeaving: true } : p))
    );
    // Remove after animation
    setTimeout(() => {
      this.popups.update((prev) => prev.filter((p) => p.popupId !== popupId));
    }, 350);
  }

  protected onClick(popup: PopupNotification) {
    this.dismiss(popup.popupId);
    if (popup.redirectUrl) {
      this.router.navigateByUrl(popup.redirectUrl);
    }
  }

  protected getTypeIcon(type: NotificationType): string {
    const icons: Record<NotificationType, string> = {
      CourseEnrollment: '🎓',
      AssignmentCreated: '📝',
      AssignmentDeadline: '⏰',
      AssignmentGraded: '✅',
      QuizCreated: '❓',
      QuizResult: '🏆',
      CertificateIssued: '🏅',
      PaymentSuccess: '💳',
      PaymentFailed: '❌',
      BatchAnnouncement: '📢',
      CoursePublished: '🚀',
      General: '🔔',
    };
    return icons[type] ?? '🔔';
  }

  protected getTypeColor(type: NotificationType): string {
    const colors: Record<NotificationType, string> = {
      CourseEnrollment: '#6366f1',
      AssignmentCreated: '#f59e0b',
      AssignmentDeadline: '#ef4444',
      AssignmentGraded: '#10b981',
      QuizCreated: '#8b5cf6',
      QuizResult: '#f59e0b',
      CertificateIssued: '#f59e0b',
      PaymentSuccess: '#10b981',
      PaymentFailed: '#ef4444',
      BatchAnnouncement: '#3b82f6',
      CoursePublished: '#6366f1',
      General: '#64748b',
    };
    return colors[type] ?? '#64748b';
  }
}
