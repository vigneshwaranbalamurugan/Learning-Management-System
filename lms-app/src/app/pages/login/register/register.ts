import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Button } from '../../../components/button/button';
import { FormInput } from '../../../components/form-input/form-input';
import { Dropdown } from '../../../components/dropdown/dropdown';
import { RegisterModel } from '../../../models/auth';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, Button, FormInput, Dropdown],
  templateUrl: './register.html',
})
export class Register {
  @Input() isSubmitting = false;
  @Output() backToLogin = new EventEmitter<void>();
  @Output() registerSubmit = new EventEmitter<RegisterModel>();

  protected email = '';
  protected password = '';
  protected confirmPassword = '';
  protected role = '';

  protected emailError = '';
  protected passwordError = '';
  protected confirmPasswordError = '';
  protected roleError = '';

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

  protected onPasswordChange(val: string): void {
    this.password = val;
    
    if (!this.password) {
      this.passwordError = 'Password is required';
    } else if (!this.hasMinLength) {
      this.passwordError = 'Password must be at least 8 characters long.';
    } else if (!this.hasUppercase || !this.hasLowercase || !this.hasNumber) {
      this.passwordError = 'Password must contain at least one uppercase letter, one lowercase letter, and one number.';
    } else {
      this.passwordError = '';
    }

    if (this.confirmPassword && this.password !== this.confirmPassword) {
      this.confirmPasswordError = 'Passwords do not match';
    } else {
      this.confirmPasswordError = '';
    }
  }

  protected onConfirmPasswordChange(val: string): void {
    this.confirmPassword = val;
    if (this.password !== this.confirmPassword) {
      this.confirmPasswordError = 'Passwords do not match';
    } else {
      this.confirmPasswordError = '';
    }
  }

  protected onRoleChange(val: string): void {
    this.role = val;
    if (!this.role) {
      this.roleError = 'Please select a role';
    } else {
      this.roleError = '';
    }
  }

  protected isPasswordFocused = false;

  protected get hasMinLength(): boolean {
    return this.password.length >= 8;
  }

  protected get hasUppercase(): boolean {
    return /[A-Z]/.test(this.password);
  }

  protected get hasLowercase(): boolean {
    return /[a-z]/.test(this.password);
  }

  protected get hasNumber(): boolean {
    return /\d/.test(this.password);
  }

  protected get allRequirementsMet(): boolean {
    return this.hasMinLength && this.hasUppercase && this.hasLowercase && this.hasNumber;
  }

  protected onSubmit(event: Event): void {
    event.preventDefault();
    this.emailError = '';
    this.passwordError = '';
    this.confirmPasswordError = '';
    this.roleError = '';

    let isValid = true;

    // Validate email
    if (!this.email) {
      this.emailError = 'Email address is required';
      isValid = false;
    } else {
      const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
      if (!emailRegex.test(this.email)) {
        this.emailError = 'Please enter a valid email address';
        isValid = false;
      }
    }

    // Validate password
    if (!this.password) {
      this.passwordError = 'Password is required';
      isValid = false;
    } else if (!this.hasMinLength) {
      this.passwordError = 'Password must be at least 8 characters long.';
      isValid = false;
    } else if (!this.hasUppercase || !this.hasLowercase || !this.hasNumber) {
      this.passwordError = 'Password must contain at least one uppercase letter, one lowercase letter, and one number.';
      isValid = false;
    }

    // Validate confirm password
    if (this.password !== this.confirmPassword) {
      this.confirmPasswordError = 'Passwords do not match';
      isValid = false;
    }

    // Validate role
    if (!this.role) {
      this.roleError = 'Please select a role';
      isValid = false;
    }

    if (isValid) {
      this.registerSubmit.emit({
        email: this.email,
        password: this.password,
        role: this.role,
      });
    }
  }
}
