import { Component } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-hero-section',
  standalone: true,
  templateUrl: './hero-section.html',
})
export class HeroSection {
  constructor(private router: Router) {}

  protected onGetStarted(): void {
    this.router.navigate(['/login']);
  }

  protected onExploreCourses(): void {
    const el = document.getElementById('about');
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
}
