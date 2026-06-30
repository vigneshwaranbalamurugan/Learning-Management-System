import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { Button } from '@components/button/button';
import { FormInput } from '@components/form-input/form-input';
import { Navbar } from '@components/navbar/navbar';
import { Footer } from '@components/footer/footer';
import { AuthService } from '@services/auth.service';
import { ToastService } from '@services/toast.service';
import { ResetPasswordModel } from '@models/auth';

@Component({
  selector: 'app-reset-password',
  standalone: true,
  imports: [CommonModule, Button, FormInput, Navbar, Footer],
  templateUrl: './reset-password.html',
})
export class ResetPasswordPage implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private authService = inject(AuthService);
  private toastService = inject(ToastService);

  isSubmitting = signal(false);
  isSuccess = signal(false);

  protected email = '';
  protected token = '';

  protected newPassword = '';
  protected confirmPassword = '';

  protected newPasswordError = '';
  protected confirmPasswordError = '';

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      this.email = params['email'];
      this.token = params['token'];

      if (!this.email || !this.token) {
        this.toastService.showError('Invalid or missing reset link.');
        this.router.navigate(['/login']);
      }
    });
  }

  protected onNewPasswordChange(val: string): void {
    this.newPassword = val;
    this.validateNewPassword();
    if (this.confirmPassword) {
      this.validateConfirmPassword();
    }
  }

  protected onConfirmPasswordChange(val: string): void {
    this.confirmPassword = val;
    this.validateConfirmPassword();
  }

  private validateNewPassword(): boolean {
    if (!this.newPassword) {
      this.newPasswordError = 'New password is required';
      return false;
    }
    if (this.newPassword.length < 8) {
      this.newPasswordError = 'Password must be at least 8 characters long.';
      return false;
    }
    const strongPasswordRegex = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$/;
    if (!strongPasswordRegex.test(this.newPassword)) {
      this.newPasswordError = 'Password must contain at least one uppercase letter, one lowercase letter, and one number.';
      return false;
    }
    this.newPasswordError = '';
    return true;
  }

  private validateConfirmPassword(): boolean {
    if (!this.confirmPassword) {
      this.confirmPasswordError = 'Please confirm your password';
      return false;
    }
    if (this.newPassword !== this.confirmPassword) {
      this.confirmPasswordError = 'Passwords do not match';
      return false;
    }
    this.confirmPasswordError = '';
    return true;
  }

  protected onSubmit(event: Event): void {
    event.preventDefault();
    
    const isNewPasswordValid = this.validateNewPassword();
    const isConfirmPasswordValid = this.validateConfirmPassword();

    if (!isNewPasswordValid || !isConfirmPasswordValid) {
      return;
    }

    const payload: ResetPasswordModel = {
      email: this.email,
      token: this.token,
      newPassword: this.newPassword
    };

    this.isSubmitting.set(true);
    this.authService.resetPassword(payload).subscribe({
      next: () => {
        this.isSubmitting.set(false);
        this.isSuccess.set(true);
        this.toastService.showSuccess('Password reset successfully.');
        setTimeout(() => {
          this.router.navigate(['/login']);
        }, 3000);
      },
      error: (err) => {
        this.isSubmitting.set(false);
        this.toastService.showApiError(err?.error?.message, 'Failed to reset password. Link may be expired.');
      }
    });
  }
}
