import { Component, signal, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { Button } from '@components/button/button';
import { FormInput } from '@components/form-input/form-input';
import { Register } from './register/register';
import { Forgot } from './forgot/forgot';
import { Resend } from './resend/resend';
import { RegisterModel, ForgotPasswordModel, ResendVerificationModel, LoginModel } from '@models/auth';
import { Navbar } from '@components/navbar/navbar';
import { Footer } from '@components/footer/footer';
import { ToastService } from '@services/toast.service';
import { AuthService } from '@services/auth.service';

type AuthScreen = 'login' | 'register' | 'forgot-password' | 'resend-verification';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, Button, FormInput, Register, Forgot, Resend, Navbar, Footer],
  templateUrl: './login.html',
  styleUrl: './login.css',
})
export class Login implements OnInit {
  private toastService = inject(ToastService);
  private authService = inject(AuthService);

  // Current screen state signal
  protected currentScreen = signal<AuthScreen>('login');
  protected isSubmitting = signal<boolean>(false);

  // Form Fields
  protected email = '';
  protected password = '';
  protected rememberMe = false;

  // Validation Errors
  protected emailError = '';
  protected passwordError = '';

  constructor(private route: ActivatedRoute) {}

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      const screen = params['screen'];
      if (screen === 'register') {
        this.currentScreen.set('register');
      } else if (screen === 'forgot') {
        this.currentScreen.set('forgot-password');
      } else if (screen === 'resend') {
        this.currentScreen.set('resend-verification');
      } else {
        this.currentScreen.set('login');
      }
    });
  }

  protected changeScreen(screen: AuthScreen): void {
    this.currentScreen.set(screen);
    this.clearForm();
  }

  private clearForm(): void {
    this.email = '';
    this.password = '';
    this.emailError = '';
    this.passwordError = '';
  }

  protected onEmailChange(val: string): void {
    this.email = val;
    this.validateEmail(val);
  }

  protected onPasswordChange(val: string): void {
    this.password = val;
    this.passwordError = '';
  }

  protected onRememberMeChange(event: Event): void {
    this.rememberMe = (event.target as HTMLInputElement).checked;
  }

  private validateEmail(email: string): boolean {
    if (!email) {
      this.emailError = 'Email address is required';
      return false;
    }
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(email)) {
      this.emailError = 'Please enter a valid email address';
      return false;
    }
    this.emailError = '';
    return true;
  }

  private validatePassword(password: string): boolean {
    if (!password) {
      this.passwordError = 'Password is required';
      return false;
    }
    if (password.length < 6) {
      this.passwordError = 'Password must be at least 6 characters';
      return false;
    }
    this.passwordError = '';
    return true;
  }

  protected loginModel(): LoginModel {
    return {
      email: this.email,
      password: this.password
    };
  }

  protected onLoginSubmit(event: Event): void {
    event.preventDefault();
    this.emailError = '';
    this.passwordError = '';

    const isEmailValid = this.validateEmail(this.email);
    const isPasswordValid = this.validatePassword(this.password);

    if (!isEmailValid || !isPasswordValid) {
      this.toastService.showError('Invalid email or password');
      return;
    }

    if (isEmailValid && isPasswordValid) {
      this.isSubmitting.set(true);
      this.authService.loginApiCall(this.loginModel(), this.rememberMe).subscribe({
        next: (response: any) => {
          this.toastService.showSuccess('Login successful');

          const token = response.token || response.accessToken;
          let role = 'Learner';
          if (token) {
            role = this.authService.getRoleFromToken(token) || 'Learner';
          } else if (response.role) {
            role = response.role;
          }

          this.authService.userRole.set(role);

          // Reset sessionChecked so initializeAuth runs fresh after login
          this.authService.sessionChecked.set(false);

          // Redirect to appropriate dashboard immediately
          this.authService.redirectToDashboard(role);
          this.isSubmitting.set(false);

          // Initialize auth session state & fetch profile details in background
          this.authService.initializeAuth().subscribe();
        },
        error: (err: any) => {
          this.isSubmitting.set(false);
          this.toastService.showApiError(err, 'Login failed. Invalid email or password.');
        }
      });
    }
  }

  protected handleRegister(data: RegisterModel): void {
    this.isSubmitting.set(true);
    setTimeout(() => {
      this.isSubmitting.set(false);
      this.toastService.showSuccess('Registration successful');
      this.changeScreen('login');
    }, 1500);
  }

  protected handleForgot(data: ForgotPasswordModel): void {
    this.isSubmitting.set(true);
    setTimeout(() => {
      this.isSubmitting.set(false);
      this.toastService.showInfo('Password reset link sent');
      this.changeScreen('login');
    }, 1500);
  }

  protected handleResend(data: ResendVerificationModel): void {
    this.isSubmitting.set(true);
    setTimeout(() => {
      this.isSubmitting.set(false);
      this.toastService.showWarning('Please verify your email');
      this.changeScreen('login');
    }, 1500);
  }
}
