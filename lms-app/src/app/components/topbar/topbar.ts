import { Component, input, output, inject, signal, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, NavigationEnd, RouterModule } from '@angular/router';
import { filter, takeUntil } from 'rxjs/operators';
import { Subject } from 'rxjs';
import { UserProfile } from '@services/profile.service';
import { UserDropdown } from '../user-dropdown/user-dropdown';
import { NotificationPanel } from '../notification-panel/notification-panel';
import { SignalRService } from '@services/signalr.service';

@Component({
  selector: 'app-topbar',
  standalone: true,
  imports: [CommonModule, UserDropdown, RouterModule, NotificationPanel],
  templateUrl: './topbar.html',
})
export class Topbar implements OnInit, OnDestroy {
  user = input.required<UserProfile>();
  isCollapsed = input<boolean>(false);
  isMobile = input<boolean>(false);
  toggleSidebar = output<void>();
  logout = output<void>();

  protected pageTitle = signal('Dashboard');
  protected isDropdownOpen = signal(false);
  protected isPanelOpen = signal(false);
  protected unreadCount = signal(0);

  private router = inject(Router);
  private signalrService = inject(SignalRService);
  private destroy$ = new Subject<void>();

  constructor() {
    this.updateTitle(this.router.url);
    this.router.events
      .pipe(filter((event) => event instanceof NavigationEnd))
      .subscribe((event: any) => {
        this.updateTitle(event.urlAfterRedirects || event.url);
        // Close panel on navigation
        this.isPanelOpen.set(false);
      });
  }

  ngOnInit() {
    // Connect to SignalR hub and subscribe to unread count stream
    this.signalrService.connect();

    this.signalrService.unreadCount$
      .pipe(takeUntil(this.destroy$))
      .subscribe((count) => this.unreadCount.set(count));
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private updateTitle(url: string) {
    if (url.includes('/dashboard')) {
      this.pageTitle.set('Dashboard');
    } else if (url.includes('/profile')) {
      this.pageTitle.set('My Profile');
    } else if (url.includes('/courses')) {
      this.pageTitle.set('My Courses');
    } else if (url.includes('/explore')) {
      this.pageTitle.set('Explore Courses');
    } else if (url.includes('/assignments')) {
      this.pageTitle.set('Assignments');
    } else if (url.includes('/quizzes')) {
      this.pageTitle.set('Quizzes');
    } else if (url.includes('/certificates')) {
      this.pageTitle.set('Certificates');
    } else if (url.includes('/progress')) {
      this.pageTitle.set('Progress Tracking');
    } else if (url.includes('/notifications')) {
      this.pageTitle.set('Notifications');
    } else if (url.includes('/feedback')) {
      this.pageTitle.set('Send Feedback');
    } else {
      this.pageTitle.set('LMS Portal');
    }
  }

  protected get avatarInitial(): string {
    return this.user().firstName ? this.user().firstName.charAt(0).toUpperCase() : 'U';
  }

  protected toggleDropdown(event: Event) {
    event.stopPropagation();
    this.isDropdownOpen.update((prev) => !prev);
  }

  protected togglePanel(event: Event) {
    event.stopPropagation();
    this.isPanelOpen.update((prev) => !prev);
    // Close user dropdown if open
    if (this.isDropdownOpen()) this.isDropdownOpen.set(false);
  }

  protected get badgeLabel(): string {
    const count = this.unreadCount();
    return count > 99 ? '99+' : count.toString();
  }
}
