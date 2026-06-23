import {
  Component,
  OnInit,
  OnDestroy,
  inject,
  signal,
  computed,
  output,
  ElementRef,
  HostListener,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { Subject, takeUntil } from 'rxjs';
import { HubConnectionState } from '@microsoft/signalr';
import { NotificationService } from '@services/notification.service';
import { SignalRService } from '@services/signalr.service';
import { Notification, NotificationType } from '@models/notification';



@Component({
  selector: 'app-notification-panel',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './notification-panel.html',
  styleUrl: './notification-panel.css',
})
export class NotificationPanel implements OnInit, OnDestroy {
  close = output<void>();

  private notificationService = inject(NotificationService);
  private signalrService = inject(SignalRService);
  private router = inject(Router);
  private elRef = inject(ElementRef);
  private destroy$ = new Subject<void>();

  protected allNotifications = signal<Notification[]>([]);
  protected currentPage = signal(1);
  protected isLoading = signal(false);
  protected isLoadingMore = signal(false);

  protected hasMore = signal(true);

  protected visibleNotifications = computed(() => this.allNotifications());

  protected unreadNotifications = computed(() =>
    this.visibleNotifications().filter((n) => !n.isRead)
  );

  protected readNotifications = computed(() =>
    this.visibleNotifications().filter((n) => n.isRead)
  );

  // ─── Lifecycle ─────────────────────────────────────────────────────────────

  ngOnInit() {
    this.loadAll();

    // New notification arrives via SignalR — prepend to list
    this.signalrService.notification$
      .pipe(takeUntil(this.destroy$))
      .subscribe((notification) => {
        this.allNotifications.update((prev) => [notification, ...prev]);
      });
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }

  // ─── Data loading ──────────────────────────────────────────────────────────

  private loadAll() {
    this.isLoading.set(true);
    this.notificationService
      .getNotifications(1, 10)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (data) => {
          this.allNotifications.set(data);
          this.hasMore.set(data.length === 10);
          this.isLoading.set(false);
        },
        error: () => this.isLoading.set(false),
      });
  }

  protected loadMore() {
    if (!this.hasMore() || this.isLoadingMore()) return;
    this.isLoadingMore.set(true);
    const nextPage = this.currentPage() + 1;
    this.notificationService
      .getNotifications(nextPage, 10)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (data) => {
          this.allNotifications.update((prev) => [...prev, ...data]);
          this.hasMore.set(data.length === 10);
          this.currentPage.set(nextPage);
          this.isLoadingMore.set(false);
        },
        error: () => this.isLoadingMore.set(false),
      });
  }

  protected onScroll(event: Event) {
    const target = event.target as HTMLElement;
    const scrollPosition = target.scrollTop + target.clientHeight;
    const scrollHeight = target.scrollHeight;

    if (scrollPosition >= scrollHeight - 50) {
      this.loadMore();
    }
  }

  // ─── Actions ───────────────────────────────────────────────────────────────

  protected async onMarkAsRead(notification: Notification, event: Event) {
    event.stopPropagation();
    if (notification.isRead) return;

    // Optimistic UI update
    this.allNotifications.update((prev) =>
      prev.map((n) =>
        n.id === notification.id
          ? { ...n, isRead: true, readAt: new Date().toISOString() }
          : n
      )
    );

    if (this.signalrService.connectionState$.value === HubConnectionState.Connected) {
      // Use hub — server will push back UpdateUnreadCount
      await this.signalrService.markAsRead(notification.id);
    } else {
      // Fallback to REST when socket is disconnected
      this.notificationService.markAsRead(notification.id).subscribe();
    }
  }

  protected async onMarkAllAsRead() {
    // Optimistic UI update
    this.allNotifications.update((prev) =>
      prev.map((n) => ({ ...n, isRead: true, readAt: n.readAt ?? new Date().toISOString() }))
    );

    if (this.signalrService.connectionState$.value === HubConnectionState.Connected) {
      await this.signalrService.markAllAsRead();
    } else {
      this.notificationService.markAllAsRead().subscribe();
    }
  }

  protected onDelete(id: number, event: Event) {
    event.stopPropagation();
    this.allNotifications.update((prev) => prev.filter((n) => n.id !== id));
    this.notificationService.deleteNotification(id).subscribe();
  }

  protected onNotificationClick(notification: Notification) {
    if (!notification.isRead) {
      this.onMarkAsRead(notification, new Event('click'));
    }
    if (notification.redirectUrl) {
      this.router.navigateByUrl(notification.redirectUrl);
      this.close.emit();
    }
  }

  // ─── Utility ───────────────────────────────────────────────────────────────

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

  protected relativeTime(dateStr: string): string {
    // Backend sends DateTime.UtcNow serialized WITHOUT the 'Z' suffix.
    // Appending 'Z' forces the browser to treat it as UTC instead of local time.
    // Only skip if the string already has a timezone indicator (Z or ±HH:MM).
    const hasTimezone = dateStr.endsWith('Z') || /[+-]\d{2}:\d{2}$/.test(dateStr);
    const utc = hasTimezone ? dateStr : dateStr + 'Z';

    const diff = Date.now() - new Date(utc).getTime();
    const mins = Math.floor(diff / 60000);
    if (mins < 1) return 'just now';
    if (mins < 60) return `${mins}m ago`;
    const hrs = Math.floor(mins / 60);
    if (hrs < 24) return `${hrs}h ago`;
    const days = Math.floor(hrs / 24);
    if (days < 7) return `${days}d ago`;
    return new Date(utc).toLocaleDateString('en-US', { month: 'short', day: 'numeric' });
  }

  protected get unreadCount(): number {
    return this.allNotifications().filter((n) => !n.isRead).length;
  }

  @HostListener('document:keydown.escape')
  protected onEscape() {
    this.close.emit();
  }
}
