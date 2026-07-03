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



import { DomSanitizer, SafeHtml } from '@angular/platform-browser';

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
  private sanitizer = inject(DomSanitizer);
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

  protected getTypeIllustration(type: NotificationType): SafeHtml {
    const svgs: Record<NotificationType, string> = {
      CourseEnrollment: `
        <svg class="w-6 h-6" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
          <path d="M12 3L2 8l10 5 10-5-10-5z" fill="url(#gradCap)" stroke="#4f46e5" stroke-width="1.5" stroke-linejoin="round"/>
          <path d="M6 10v4c0 2 3 3.5 6 3.5s6-1.5 6-3.5v-4" stroke="#4f46e5" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"/>
          <path d="M21.5 8v6.5l-1.5 1-1.5-1V8" fill="#f59e0b" stroke="#d97706" stroke-width="1" stroke-linejoin="round"/>
          <defs>
            <linearGradient id="gradCap" x1="2" y1="8" x2="22" y2="8" gradientUnits="userSpaceOnUse">
              <stop stop-color="#818cf8"/>
              <stop offset="1" stop-color="#4f46e5"/>
            </linearGradient>
          </defs>
        </svg>
      `,
      AssignmentCreated: `
        <svg class="w-6 h-6" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
          <rect x="4" y="3" width="16" height="18" rx="2" fill="url(#assignDoc)" stroke="#d97706" stroke-width="1.5"/>
          <path d="M8 7h8M8 11h8M8 15h5" stroke="#b45309" stroke-width="1.5" stroke-linecap="round"/>
          <path d="M16 14l3 3-5 2 2-5 3 3z" fill="#f59e0b" stroke="#b45309" stroke-width="1"/>
          <defs>
            <linearGradient id="assignDoc" x1="4" y1="3" x2="20" y2="21" gradientUnits="userSpaceOnUse">
              <stop stop-color="#fef3c7"/>
              <stop offset="1" stop-color="#fde68a"/>
            </linearGradient>
          </defs>
        </svg>
      `,
      AssignmentDeadline: `
        <svg class="w-6 h-6" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
          <circle cx="12" cy="12" r="9" fill="url(#deadlineClock)" stroke="#dc2626" stroke-width="1.5"/>
          <path d="M12 7v5l3 2" stroke="#dc2626" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"/>
          <circle cx="19" cy="5" r="3" fill="#ef4444" stroke="#ffffff" stroke-width="1"/>
          <path d="M19 4v1.5M19 7h.01" stroke="#ffffff" stroke-width="1" stroke-linecap="round"/>
          <defs>
            <linearGradient id="deadlineClock" x1="3" y1="3" x2="21" y2="21" gradientUnits="userSpaceOnUse">
              <stop stop-color="#fee2e2"/>
              <stop offset="1" stop-color="#fca5a5"/>
            </linearGradient>
          </defs>
        </svg>
      `,
      AssignmentGraded: `
        <svg class="w-6 h-6" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
          <path d="M5 3h10l4 4v14H5V3z" fill="url(#gradedPaper)" stroke="#059669" stroke-width="1.5" stroke-linejoin="round"/>
          <path d="M9 12l2 2 4-4" stroke="#10b981" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/>
          <path d="M17 3.5v4h-4" fill="none" stroke="#059669" stroke-width="1.5" stroke-linejoin="round"/>
          <defs>
            <linearGradient id="gradedPaper" x1="5" y1="3" x2="19" y2="21" gradientUnits="userSpaceOnUse">
              <stop stop-color="#ecfdf5"/>
              <stop offset="1" stop-color="#d1fae5"/>
            </linearGradient>
          </defs>
        </svg>
      `,
      QuizCreated: `
        <svg class="w-6 h-6" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
          <path d="M12 21c-4.97 0-9-3.582-9-8s4.03-8 9-8 9 3.582 9 8-4.03 8-9 8z" fill="url(#quizBubble)" stroke="#7c3aed" stroke-width="1.5"/>
          <path d="M12 16h.01" stroke="#7c3aed" stroke-width="2.5" stroke-linecap="round"/>
          <path d="M10 10.5c0-1.5 1-2.5 2-2.5s2 1 2 2.5c0 1.25-1 1.75-2 2.25" stroke="#7c3aed" stroke-width="1.5" stroke-linecap="round"/>
          <circle cx="4" cy="5" r="1.5" fill="#f59e0b"/>
          <circle cx="20" cy="18" r="2" fill="#3b82f6"/>
          <defs>
            <linearGradient id="quizBubble" x1="3" y1="5" x2="21" y2="21" gradientUnits="userSpaceOnUse">
              <stop stop-color="#ede9fe"/>
              <stop offset="1" stop-color="#ddd6fe"/>
            </linearGradient>
          </defs>
        </svg>
      `,
      QuizResult: `
        <svg class="w-6 h-6" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
          <path d="M6 9H4.5A1.5 1.5 0 013 7.5v-1A1.5 1.5 0 014.5 5H6v4zm12 0h1.5A1.5 1.5 0 0021 7.5v-1A1.5 1.5 0 0019.5 5H18v4z" stroke="#d97706" stroke-width="1.5" stroke-linejoin="round"/>
          <path d="M6 5h12v5c0 3.314-2.686 6-6 6s-6-2.686-6-6V5z" fill="url(#trophyGrad)" stroke="#d97706" stroke-width="1.5" stroke-linejoin="round"/>
          <path d="M12 16v4m-3 0h6" stroke="#d97706" stroke-width="1.5" stroke-linecap="round"/>
          <circle cx="3" cy="16" r="1" fill="#8b5cf6"/>
          <circle cx="21" cy="12" r="1.5" fill="#ec4899"/>
          <defs>
            <linearGradient id="trophyGrad" x1="6" y1="5" x2="18" y2="16" gradientUnits="userSpaceOnUse">
              <stop stop-color="#fef08a"/>
              <stop offset="1" stop-color="#f59e0b"/>
            </linearGradient>
          </defs>
        </svg>
      `,
      CertificateIssued: `
        <svg class="w-6 h-6" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
          <rect x="3" y="5" width="18" height="13" rx="1.5" fill="url(#certBg)" stroke="#d97706" stroke-width="1.5"/>
          <circle cx="16" cy="13" r="2.5" fill="#f59e0b" stroke="#b45309" stroke-width="1"/>
          <path d="M15 15l-1 4.5L16 18l2 1.5-1-4.5" fill="#ef4444" stroke="#b45309" stroke-width="1" stroke-linejoin="round"/>
          <path d="M7 9h5M7 12h5" stroke="#b45309" stroke-width="1.5" stroke-linecap="round"/>
          <defs>
            <linearGradient id="certBg" x1="3" y1="5" x2="21" y2="18" gradientUnits="userSpaceOnUse">
              <stop stop-color="#fffbeb"/>
              <stop offset="1" stop-color="#fef3c7"/>
            </linearGradient>
          </defs>
        </svg>
      `,
      PaymentSuccess: `
        <svg class="w-6 h-6" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
          <rect x="3" y="6" width="18" height="12" rx="2" fill="url(#cardBg)" stroke="#059669" stroke-width="1.5"/>
          <path d="M3 10h18" stroke="#059669" stroke-width="1.5"/>
          <circle cx="17" cy="14" r="2.5" fill="#10b981" stroke="#ffffff" stroke-width="1"/>
          <path d="M16 14l1 1 2-2" stroke="#ffffff" stroke-width="1" stroke-linecap="round" stroke-linejoin="round"/>
          <defs>
            <linearGradient id="cardBg" x1="3" y1="6" x2="21" y2="18" gradientUnits="userSpaceOnUse">
              <stop stop-color="#d1fae5"/>
              <stop offset="1" stop-color="#a7f3d0"/>
            </linearGradient>
          </defs>
        </svg>
      `,
      PaymentFailed: `
        <svg class="w-6 h-6" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
          <rect x="3" y="6" width="18" height="12" rx="2" fill="url(#cardFailBg)" stroke="#dc2626" stroke-width="1.5"/>
          <path d="M3 10h18" stroke="#dc2626" stroke-width="1.5"/>
          <circle cx="17" cy="14" r="2.5" fill="#ef4444" stroke="#ffffff" stroke-width="1"/>
          <path d="M16.5 14h1M16.5 12.5v2" stroke="#ffffff" stroke-width="1" stroke-linecap="round" stroke-linejoin="round"/>
          <defs>
            <linearGradient id="cardFailBg" x1="3" y1="6" x2="21" y2="18" gradientUnits="userSpaceOnUse">
              <stop stop-color="#fee2e2"/>
              <stop offset="1" stop-color="#fca5a5"/>
            </linearGradient>
          </defs>
        </svg>
      `,
      BatchAnnouncement: `
        <svg class="w-6 h-6" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
          <path d="M6 14.5l-2-1v-4l2-1V14.5z" fill="#3b82f6" stroke="#2563eb" stroke-width="1.5"/>
          <path d="M6 8.5h8.5l4-3v13l-4-3H6v-7z" fill="url(#megaGrad)" stroke="#2563eb" stroke-width="1.5" stroke-linejoin="round"/>
          <path d="M21 9a3 3 0 010 6m-2-4.5a1.5 1.5 0 010 3" stroke="#f59e0b" stroke-width="1.5" stroke-linecap="round"/>
          <path d="M8 14v3.5a1.5 1.5 0 01-3 0V14" stroke="#2563eb" stroke-width="1.5" stroke-linecap="round"/>
          <defs>
            <linearGradient id="megaGrad" x1="6" y1="5.5" x2="18.5" y2="15.5" gradientUnits="userSpaceOnUse">
              <stop stop-color="#dbeafe"/>
              <stop offset="1" stop-color="#93c5fd"/>
            </linearGradient>
          </defs>
        </svg>
      `,
      CoursePublished: `
        <svg class="w-6 h-6" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
          <path d="M12 2S8 7 8 12c0 1.5.5 3 1.5 4.5L8 19l1.5-1 1-1.5c1.5 1 3 1.5 4.5 1.5 5 0 10-4 10-4S17 2 12 2z" fill="url(#rocketBg)" stroke="#4f46e5" stroke-width="1.5" stroke-linejoin="round"/>
          <circle cx="14" cy="10" r="1.5" fill="#ffffff"/>
          <path d="M8 16l-3 3v1h1l3-3" stroke="#f59e0b" stroke-width="1.5" stroke-linecap="round"/>
          <path d="M6 19.5c-.8.8-1.5 1-1.5 1s.2-.7 1-1.5" stroke="#ef4444" stroke-width="1.5" stroke-linecap="round"/>
          <defs>
            <linearGradient id="rocketBg" x1="8" y1="2" x2="22" y2="16" gradientUnits="userSpaceOnUse">
              <stop stop-color="#c7d2fe"/>
              <stop offset="1" stop-color="#818cf8"/>
            </linearGradient>
          </defs>
        </svg>
      `,
      General: `
        <svg class="w-6 h-6" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
          <path d="M18 10a6 6 0 00-12 0v4l-2 2h16l-2-2v-4z" fill="url(#bellGrad)" stroke="#475569" stroke-width="1.5" stroke-linejoin="round"/>
          <path d="M10 16a2 2 0 004 0" stroke="#475569" stroke-width="1.5" stroke-linecap="round"/>
          <circle cx="4" cy="6" r="1" fill="#3b82f6"/>
          <path d="M21 5l-1.5 1.5M19.5 5l1.5 1.5" stroke="#f59e0b" stroke-width="1" stroke-linecap="round"/>
          <defs>
            <linearGradient id="bellGrad" x1="6" y1="4" x2="18" y2="16" gradientUnits="userSpaceOnUse">
              <stop stop-color="#f1f5f9"/>
              <stop offset="1" stop-color="#cbd5e1"/>
            </linearGradient>
          </defs>
        </svg>
      `,
      PaymentDispute: `
        <svg class="w-6 h-6" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
          <path d="M12 2L3 7v5c0 5.25 3.9 10.16 9 11.36C17.1 22.16 21 17.25 21 12V7L12 2z" fill="url(#disputeShield)" stroke="#dc2626" stroke-width="1.5" stroke-linejoin="round"/>
          <path d="M12 8v4" stroke="#dc2626" stroke-width="2" stroke-linecap="round"/>
          <circle cx="12" cy="15" r="1" fill="#dc2626"/>
          <defs>
            <linearGradient id="disputeShield" x1="3" y1="2" x2="21" y2="23" gradientUnits="userSpaceOnUse">
              <stop stop-color="#fee2e2"/>
              <stop offset="1" stop-color="#fca5a5"/>
            </linearGradient>
          </defs>
        </svg>
      `,
      PaymentDowntime: `
        <svg class="w-6 h-6" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
          <path d="M10.29 3.86L1.82 18a2 2 0 001.71 3h16.94a2 2 0 001.71-3L13.71 3.86a2 2 0 00-3.42 0z" fill="url(#downtimeWarn)" stroke="#d97706" stroke-width="1.5" stroke-linejoin="round"/>
          <path d="M12 9v4" stroke="#b45309" stroke-width="2" stroke-linecap="round"/>
          <circle cx="12" cy="17" r="1" fill="#b45309"/>
          <defs>
            <linearGradient id="downtimeWarn" x1="2" y1="3" x2="22" y2="21" gradientUnits="userSpaceOnUse">
              <stop stop-color="#fef3c7"/>
              <stop offset="1" stop-color="#fde68a"/>
            </linearGradient>
          </defs>
        </svg>
      `,
      Settlement: `
        <svg class="w-6 h-6" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
          <rect x="3" y="10" width="18" height="11" rx="1.5" fill="url(#settleBg)" stroke="#059669" stroke-width="1.5"/>
          <path d="M3 10l9-7 9 7" stroke="#059669" stroke-width="1.5" stroke-linejoin="round"/>
          <rect x="9" y="14" width="6" height="7" rx="1" fill="#10b981" stroke="#059669" stroke-width="1"/>
          <path d="M12 14v7" stroke="#059669" stroke-width="1"/>
          <defs>
            <linearGradient id="settleBg" x1="3" y1="10" x2="21" y2="21" gradientUnits="userSpaceOnUse">
              <stop stop-color="#d1fae5"/>
              <stop offset="1" stop-color="#a7f3d0"/>
            </linearGradient>
          </defs>
        </svg>
      `,
      ProductRoute: `
        <svg class="w-6 h-6" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
          <circle cx="5" cy="6" r="2.5" fill="#3b82f6" stroke="#2563eb" stroke-width="1.5"/>
          <circle cx="19" cy="18" r="2.5" fill="url(#routeGrad)" stroke="#2563eb" stroke-width="1.5"/>
          <path d="M5 8.5C5 13 10 11 14 13s5 6 5 6" stroke="#2563eb" stroke-width="1.5" stroke-linecap="round" stroke-dasharray="2 2"/>
          <circle cx="19" cy="6" r="1.5" fill="#f59e0b"/>
          <path d="M17 6h2m0-1.5v1.5" stroke="#b45309" stroke-width="1" stroke-linecap="round"/>
          <defs>
            <linearGradient id="routeGrad" x1="16.5" y1="15.5" x2="21.5" y2="20.5" gradientUnits="userSpaceOnUse">
              <stop stop-color="#dbeafe"/>
              <stop offset="1" stop-color="#93c5fd"/>
            </linearGradient>
          </defs>
        </svg>
      `,
    };
    return this.sanitizer.bypassSecurityTrustHtml(svgs[type] ?? svgs.General);
  }

  protected getTypeColor(type: NotificationType): string {
    const colors: Record<NotificationType, string> = {
      CourseEnrollment: '#1C1C7B',     // Primary
      CoursePublished: '#1C1C7B',      // Primary
      AssignmentCreated: '#FF8C00',     // Secondary
      QuizResult: '#FF8C00',            // Secondary
      CertificateIssued: '#FF8C00',     // Secondary
      AssignmentDeadline: '#DC2626',    // Danger
      PaymentFailed: '#DC2626',         // Danger
      PaymentDispute: '#DC2626',        // Danger — disputes are urgent
      AssignmentGraded: '#16A34A',      // Success
      PaymentSuccess: '#16A34A',        // Success
      Settlement: '#16A34A',            // Success — money settled to platform
      QuizCreated: '#8B5CF6',           // Purple
      BatchAnnouncement: '#0284C7',     // Info
      ProductRoute: '#0284C7',          // Info — payout route status
      PaymentDowntime: '#D97706',       // Amber — warning
      General: '#475569',               // Slate
    };
    return colors[type] ?? '#475569';
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
    return new Date(utc).toLocaleDateString('en-IN', { month: 'short', day: 'numeric', timeZone: 'Asia/Kolkata' });
  }

  protected get unreadCount(): number {
    return this.allNotifications().filter((n) => !n.isRead).length;
  }

  @HostListener('document:keydown.escape')
  protected onEscape() {
    this.close.emit();
  }
}
