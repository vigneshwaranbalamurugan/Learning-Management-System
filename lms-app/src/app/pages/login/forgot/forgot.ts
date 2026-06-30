import { Component, Output, EventEmitter, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Button } from '@components/button/button';
import { FormInput } from '@components/form-input/form-input';
import { AuthService } from '@services/auth.service';
import { ToastService } from '@services/toast.service';

@Component({
  selector: 'app-forgot',
  standalone: true,
  imports: [CommonModule, Button, FormInput],
  templateUrl: './forgot.html',
})
export class Forgot {
  @Output() backToLogin = new EventEmitter<void>();

  private authService = inject(AuthService);
  private toastService = inject(ToastService);

  isSubmitting = signal(false);
  isSuccess = signal(false);

  protected email = '';
  protected emailError = '';

  protected onEmailChange(val: string): void {
    this.email = val;
    if (!this.email) {
      this.emailError = 'Email address is required';
    } else {
      const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
      if (!emailRegex.test(this.email)) {
        this.emailError = 'Please enter a valid email address';
      } else {
        this.emailError = '';
      }
    }
  }

  protected onSubmit(event: Event): void {
    event.preventDefault();
    this.emailError = '';

    if (!this.email) {
      this.emailError = 'Email address is required';
      return;
    }

    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(this.email)) {
      this.emailError = 'Please enter a valid email address';
      return;
    }

    this.isSubmitting.set(true);
    this.authService.forgotPassword(this.email).subscribe({
      next: () => {
        this.isSubmitting.set(false);
        this.isSuccess.set(true);
      },
      error: (err) => {
        this.isSubmitting.set(false);
        this.toastService.showApiError(err?.error?.message, 'Failed to send reset link.');
      }
    });
  }
}
