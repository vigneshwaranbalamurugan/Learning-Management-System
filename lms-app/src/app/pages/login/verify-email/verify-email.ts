import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { AuthService } from '@services/auth.service';
import { ToastService } from '@services/toast.service';
import { Navbar } from '@components/navbar/navbar';
import { Footer } from '@components/footer/footer';
import { Button } from '@components/button/button';

@Component({
  selector: 'app-verify-email',
  standalone: true,
  imports: [CommonModule, RouterModule, Navbar, Footer, Button],
  templateUrl: './verify-email.html'
})
export class VerifyEmailPage implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private authService = inject(AuthService);
  private toastService = inject(ToastService);

  status = signal<'loading' | 'success' | 'error'>('loading');
  errorMessage = signal<string>('');

  ngOnInit(): void {
    const email = this.route.snapshot.queryParamMap.get('email');
    const token = this.route.snapshot.queryParamMap.get('token');

    if (!email || !token) {
      this.status.set('error');
      this.errorMessage.set('Invalid verification link. Please check the URL and try again.');
      return;
    }

    this.authService.verifyEmail(email, token).subscribe({
      next: () => {
        this.status.set('success');
        this.toastService.showSuccess('Email verified successfully!');
      },
      error: (err) => {
        this.status.set('error');
        this.errorMessage.set(err?.error?.message || 'Verification failed. The link may have expired.');
        this.toastService.showApiError(err?.error?.message, 'Verification failed.');
      }
    });
  }

  goToLogin(): void {
    this.router.navigate(['/login']);
  }
}
