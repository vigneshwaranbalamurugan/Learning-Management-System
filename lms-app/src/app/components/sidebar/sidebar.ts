import { Component, output, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { SidebarService } from '@services/sidebar.service';
import { AuthService } from '@services/auth.service';
import { environment } from '@environments/environment';

interface MenuItem {
  label: string;
  route: string;
  iconPath: string;
  hideForRole?: string;
  showForRole?: string;
  isExternal?: boolean;
}

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './sidebar.html'
})
export class Sidebar {
  // ── Injected service — single source of truth for sidebar state ──────────
  protected sidebar = inject(SidebarService);
  private authService = inject(AuthService);
  protected env = environment;

  // Logout still needs to be emitted up to DashboardLayout (which owns the modal)
  logout = output<void>();

  private router = inject(Router);

  protected get userRole(): string {
    return this.authService.userRole() || 'learner';
  }

  protected menuItems: MenuItem[] = [
    {
      label: 'Dashboard',
      route: 'dashboard',
      iconPath: 'M3 12l2-2m0 0l7-7 7 7M5 10v10a1 1 0 001 1h3m10-11l2 2m-2-2v10a1 1 0 01-1 1h3m-6 0a1 1 0 001-1v-4a1 1 0 011-1h2a1 1 0 011 1v4a1 1 0 001 1m-6 0h6'
    },
    {
      label: 'My Courses',
      route: 'courses',
      iconPath: 'M12 6.253v13m0-13C10.832 5.477 9.246 5 7.5 5S4.168 5.477 3 6.253v13C4.168 18.477 5.754 18 7.5 18s3.332.477 4.5 1.253m0-13C13.168 5.477 14.754 5 16.5 5c1.747 0 3.332.477 4.5 1.253v13C19.832 18.477 18.247 18 16.5 18c-1.746 0-3.332.477-4.5 1.253',
      hideForRole: 'admin'
    },
    {
      label: 'Courses',
      route: 'courses',
      iconPath: 'M12 6.253v13m0-13C10.832 5.477 9.246 5 7.5 5S4.168 5.477 3 6.253v13C4.168 18.477 5.754 18 7.5 18s3.332.477 4.5 1.253m0-13C13.168 5.477 14.754 5 16.5 5c1.747 0 3.332.477 4.5 1.253v13C19.832 18.477 18.247 18 16.5 18c-1.746 0-3.332.477-4.5 1.253',
      showForRole: 'admin'
    },
    {
      label: 'Explore Courses',
      route: 'explore',
      iconPath: 'M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z',
      showForRole: 'learner'
    },
    {
      label: 'Assignments',
      route: 'assignments',
      iconPath: 'M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2m-3 7h3m-3 4h3m-6-4h.01M9 16h.01'
    },
    {
      label: 'Quizzes',
      route: 'quizzes',
      iconPath: 'M16.862 4.487l1.687-1.688a1.875 1.875 0 112.652 2.652L10.582 16.07a4.5 4.5 0 01-1.897 1.13L6 18l.8-2.685a4.5 4.5 0 011.13-1.897l8.932-8.931zm0 0L19.5 7.125M18 14v4.75A2.25 2.25 0 0115.75 21H5.25A2.25 2.25 0 013 18.75V8.25A2.25 2.25 0 015.25 6H10',
      showForRole: 'learner'
    },
    {
      label: 'Certificates',
      route: 'certificates',
      iconPath: 'M9 12l2 2 4-4M7.835 4.697a3.42 3.42 0 001.946-.806 3.42 3.42 0 014.438 0 3.42 3.42 0 001.946.806 3.42 3.42 0 013.138 3.138 3.42 3.42 0 00.806 1.946 3.42 3.42 0 010 4.438 3.42 3.42 0 00-.806 1.946 3.42 3.42 0 01-3.138 3.138 3.42 3.42 0 00-1.946.806 3.42 3.42 0 01-4.438 0 3.42 3.42 0 00-1.946-.806 3.42 3.42 0 01-3.138-3.138 3.42 3.42 0 00-.806-1.946 3.42 3.42 0 010-4.438 3.42 3.42 0 00.806-1.946 3.42 3.42 0 013.138-3.138z',
      hideForRole:'instructor'
    },
    {
      label: 'Progress',
      route: 'progress',
      iconPath: 'M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z',
      hideForRole: 'admin'
    },
    {
      label: 'Reviews',
      route: 'reviews',
      iconPath: 'M7 8h10M7 12h4m1 8l-4-4H5a2 2 0 01-2-2V6a2 2 0 012-2h14a2 2 0 012 2v8a2 2 0 01-2 2h-3l-4 4z'
    },
    {
      label: 'System Logs',
      route: 'logs',
      iconPath: 'M4 6h16M4 10h16M4 14h16M4 18h16',
      showForRole: 'admin'
    },
    {
      label: 'Settings',
      route: 'settings',
      iconPath: 'M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.065 2.572c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.572 1.065c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.065-2.572c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z M15 12a3 3 0 11-6 0 3 3 0 016 0z',
      showForRole: 'admin'
    },
    {
      label: 'Job Dashboard',
      route: this.env.hangfireUrl,
      iconPath: 'M13 10V3L4 14h7v7l9-11h-7z',
      showForRole: 'admin',
      isExternal: true
    }
  ];

  protected get displayedMenuItems(): MenuItem[] {
    const role = this.userRole.toLowerCase();
    return this.menuItems.filter(item => {
      if (item.showForRole && item.showForRole.toLowerCase() !== role) return false;
      if (item.hideForRole && item.hideForRole.toLowerCase() === role) return false;
      return true;
    });
  }

  protected getAbsoluteRoute(itemRoute: string): string {
    return `/${this.userRole.toLowerCase()}/${itemRoute}`;
  }

  protected isRouteActive(itemRoute: string): boolean {
    const absRoute = this.getAbsoluteRoute(itemRoute);
    return this.router.url === absRoute || this.router.url.startsWith(`${absRoute}/`);
  }

  /** Close mobile drawer when any nav link is tapped */
  protected onNavLinkClick(): void {
    this.sidebar.closeMobile();
  }

  /** Toggle collapse/expand */
  protected onToggle(): void {
    this.sidebar.toggle();
  }
}
