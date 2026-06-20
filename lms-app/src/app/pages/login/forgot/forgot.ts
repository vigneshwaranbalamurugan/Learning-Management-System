import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Button } from '@components/button/button';
import { FormInput } from '@components/form-input/form-input';
import { ForgotPasswordModel } from '@models/auth';

@Component({
  selector: 'app-forgot',
  standalone: true,
  imports: [CommonModule, Button, FormInput],
  templateUrl: './forgot.html',
})
export class Forgot {
  @Input() isSubmitting = false;
  @Output() backToLogin = new EventEmitter<void>();
  @Output() forgotSubmit = new EventEmitter<ForgotPasswordModel>();

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

    this.forgotSubmit.emit({ email: this.email });
  }
}
