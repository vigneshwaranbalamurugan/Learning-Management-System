import { Injectable, signal } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class SidebarService {
  readonly isCollapsed        = signal<boolean>(false);
  readonly isMobile           = signal<boolean>(false);
  readonly isMobileSidebarOpen = signal<boolean>(false);

  checkScreenSize(): void {
    const width = window.innerWidth;
    if (width < 768) {
      this.isMobile.set(true);
      this.isCollapsed.set(true);
      this.isMobileSidebarOpen.set(false);
    } else if (width < 1024) {
      this.isMobile.set(false);
      this.isCollapsed.set(true);
      this.isMobileSidebarOpen.set(false);
    } else {
      this.isMobile.set(false);
      this.isCollapsed.set(false);
      this.isMobileSidebarOpen.set(false);
    }
  }

  toggle(): void {
    if (this.isMobile()) {
      this.isMobileSidebarOpen.update(v => !v);
    } else {
      this.isCollapsed.update(v => !v);
    }
  }

  closeMobile(): void {
    this.isMobileSidebarOpen.set(false);
  }
}
