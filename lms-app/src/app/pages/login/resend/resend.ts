import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Button } from '@components/button/button';
import { FormInput } from '@components/form-input/form-input';
import { ResendVerificationModel } from '@models/auth';

@Component({
  selector: 'app-resend',
  standalone: true,
  imports: [CommonModule, Button, FormInput],
  templateUrl: './resend.html',
})
export class Resend {
  @Input() isSubmitting = false;
  @Output() backToLogin = new EventEmitter<void>();
  @Output() resendSubmit = new EventEmitter<ResendVerificationModel>();

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

    this.resendSubmit.emit({ email: this.email });
  }
}
