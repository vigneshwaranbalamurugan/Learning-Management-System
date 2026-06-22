import { Component, inject, HostListener, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterOutlet } from '@angular/router';
import { Sidebar } from '../sidebar/sidebar';
import { Topbar } from '../topbar/topbar';
import { AuthService } from '@services/auth.service';
import { SidebarService } from '@services/sidebar.service';
import { ConfirmModal } from '../confirm-modal/confirm-modal';
import { Loader } from '../loader/loader';
import { NotificationPopup } from '../notification-popup/notification-popup';
import { SignalRService } from '@services/signalr.service';

@Component({
  selector: 'app-dashboard-layout',
  standalone: true,
  imports: [CommonModule, RouterOutlet, Sidebar, Topbar, ConfirmModal, Loader, NotificationPopup],
  templateUrl: './dashboard-layout.html'
})
export class DashboardLayout implements OnInit {
  // ── Sidebar state lives in the service ──────────────────────────────────
  protected sidebar = inject(SidebarService);

  protected showLogoutModal = signal(false);
  protected isLoggingOut = signal(false);

  protected authService = inject(AuthService);
  private signalrService = inject(SignalRService);
  private router = inject(Router);

  ngOnInit() {
    this.sidebar.checkScreenSize();
  }

  @HostListener('window:resize')
  onResize() {
    this.sidebar.checkScreenSize();
  }

  protected onLogout() {
    this.showLogoutModal.set(true);
  }

  protected closeLogoutModal() {
    this.showLogoutModal.set(false);
  }

  protected confirmLogout() {
    this.closeLogoutModal();
    this.isLoggingOut.set(true);
    // Disconnect SignalR before logout
    this.signalrService.disconnect();
    this.authService.logout().subscribe({
      next: () => {
        this.isLoggingOut.set(false);
        this.router.navigate(['/login']);
      },
      error: () => {
        this.isLoggingOut.set(false);
        this.router.navigate(['/login']);
      }
    });
  }

  protected get userProfile() {
    return this.authService.currentUser() || {
      fullName: 'User Profile',
      firstName: 'User',
      lastName: 'Profile',
      bio: '',
      dateOfBirth: '',
      location: '',
      profilePictureUrl: '',
      email: localStorage.getItem('user_email') || ''
    };
  }
}
