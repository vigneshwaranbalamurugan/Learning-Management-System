import { Component, HostListener, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './navbar.html',
  styleUrl: './navbar.css',
})
export class Navbar {
  protected isScrolled = false;
  protected isMobileMenuOpen = false;
  protected activeSection = signal<string>('home');

  constructor(private router: Router) {}

  @HostListener('window:scroll')
  protected onWindowScroll(): void {
    this.isScrolled = window.scrollY > 20;

    // Detect active section on scroll
    const sections = ['home', 'about', 'feedback'];
    const scrollPosition = window.scrollY + 100; // Offset for navbar height

    for (const sectionId of sections) {
      const el = document.getElementById(sectionId);
      if (el) {
        const top = el.offsetTop;
        const height = el.offsetHeight;
        if (scrollPosition >= top && scrollPosition < top + height) {
          this.activeSection.set(sectionId);
          break;
        }
      }
    }
  }

  protected toggleMobileMenu(): void {
    this.isMobileMenuOpen = !this.isMobileMenuOpen;
  }

  protected scrollToSection(sectionId: string, event: Event): void {
    event.preventDefault();
    this.isMobileMenuOpen = false;
    this.activeSection.set(sectionId);

    // If we're not on the home page route, navigate home first, then scroll
    if (this.router.url !== '/') {
      this.router.navigate(['/']).then(() => {
        setTimeout(() => this.performScroll(sectionId), 100);
      });
    } else {
      this.performScroll(sectionId);
    }
  }

  private performScroll(sectionId: string): void {
    const el = document.getElementById(sectionId);
    if (el) {
      const navbarHeight = 72;
      const elementPosition = el.getBoundingClientRect().top + window.scrollY;
      const offsetPosition = elementPosition - navbarHeight;

      window.scrollTo({
        top: offsetPosition,
        behavior: 'smooth',
      });
    }
  }

  protected navigateToAuth(screen: 'login' | 'register'): void {
    this.isMobileMenuOpen = false;
    this.router.navigate(['/login'], { queryParams: { screen } });
  }
}
