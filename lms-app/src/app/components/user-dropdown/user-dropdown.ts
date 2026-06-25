import { Component, Input, Output, EventEmitter, HostListener, ElementRef, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { UserProfile } from '@services/profile.service';

@Component({
  selector: 'app-user-dropdown',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './user-dropdown.html'
})
export class UserDropdown {
  @Input({ required: true }) user!: UserProfile;
  @Output() close = new EventEmitter<void>();
  @Output() logout = new EventEmitter<void>();

  private elementRef = inject(ElementRef);
  private router = inject(Router);

  protected get avatarInitial(): string {
    return this.user.firstName ? this.user.firstName.charAt(0).toUpperCase() : 'U';
  }

  @HostListener('document:click', ['$event'])
  onClickOutside(event: Event) {
    if (!this.elementRef.nativeElement.contains(event.target)) {
      this.close.emit();
    }
  }

  protected navigateToProfile() {
    const role = this.user.role?.toLowerCase() === 'instructor' ? 'instructor' : 'learner';
    this.router.navigate([`/${role}/profile`]);
    this.close.emit();
  }

  protected navigateToCertificates() {
    this.router.navigate(['/learner/certificates']);
    this.close.emit();
  }

  protected navigateToRevenue() {
    this.router.navigate(['/instructor/revenue']);
    this.close.emit();
  }

  protected navigateToSettings() {
    const role = this.user.role?.toLowerCase() === 'instructor' ? 'instructor' : 'learner';
    this.router.navigate([`/${role}/settings`]);
    this.close.emit();
  }

  protected onLogout() {
    this.logout.emit();
    this.close.emit();
  }
}
