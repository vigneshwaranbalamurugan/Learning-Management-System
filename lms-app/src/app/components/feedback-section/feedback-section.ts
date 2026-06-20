import { Component, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Button } from '@components/button/button';
import { FormInput } from '@components/form-input/form-input';
import { ToastService } from '@services/toast.service';

@Component({
  selector: 'app-feedback-section',
  standalone: true,
  imports: [CommonModule, Button, FormInput],
  templateUrl: './feedback-section.html',
})
export class FeedbackSection {
  private toastService = inject(ToastService);

  protected email = '';
  protected rating = 0;
  protected comment = '';

  protected emailError = '';
  protected ratingError = '';
  protected commentError = '';

  protected hoveredRating = 0;
  protected isSubmitting = signal(false);
  protected isSuccess = signal(false);

  protected setRating(stars: number): void {
    this.rating = stars;
    this.ratingError = '';
  }

  protected hoverStars(stars: number): void {
    this.hoveredRating = stars;
  }

  protected clearHoverStars(): void {
    this.hoveredRating = 0;
  }

  protected onEmailChange(val: string): void {
    this.email = val;
    this.validateEmail(val);
  }

  protected onCommentInput(event: Event): void {
    this.comment = (event.target as HTMLTextAreaElement).value;
    this.commentError = '';
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

  protected onSubmit(event: Event): void {
    event.preventDefault();
    this.emailError = '';
    this.ratingError = '';
    this.commentError = '';

    let isValid = true;

    if (!this.validateEmail(this.email)) {
      isValid = false;
    }

    if (this.rating === 0) {
      this.ratingError = 'Please provide a star rating';
      isValid = false;
    }

    if (!this.comment) {
      this.commentError = 'Feedback comment is required';
      isValid = false;
    }

    if (!isValid) {
      this.toastService.showWarning('Please correct the validation errors in the form.');
      return;
    }

    if (isValid) {
      this.isSubmitting.set(true);
      setTimeout(() => {
        this.isSubmitting.set(false);
        this.isSuccess.set(true);
        this.toastService.showSuccess('Feedback submitted successfully!');
        
        // Reset form fields
        this.email = '';
        this.rating = 0;
        this.comment = '';
        
        // Auto-close success message after 5 seconds
        setTimeout(() => this.isSuccess.set(false), 5000);
      }, 1500);
    }
  }
}

