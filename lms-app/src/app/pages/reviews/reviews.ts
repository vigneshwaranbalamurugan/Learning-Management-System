import { Component, OnInit, signal, WritableSignal, inject, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ToastService } from '@services/toast.service';
import { AuthService } from '@services/auth.service';
import { EnrollmentResponse } from '@models/enrollment';
import { ReviewResponse } from '@models/review';
import { untilDestroyed } from '../../rxjs/until-destroyed';
import { forkJoin, of } from 'rxjs';
import { catchError, finalize } from 'rxjs/operators';
import { EnrollmentService } from '@services/enrollment.service';
import { ReviewService } from '@services/review.service';

interface CourseFeedbackState {
  course: EnrollmentResponse;
  myReview: ReviewResponse | null;
  isSubmitting: WritableSignal<boolean>;
  ratingForm: number;
  textForm: string;
}

@Component({
  selector: 'app-reviews',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './reviews.html'
})
export class ReviewsPage implements OnInit {
  private reviewService = inject(ReviewService);
  private enrollmentService = inject(EnrollmentService);
  private toastService = inject(ToastService);
  private authService = inject(AuthService);
  private destroyRef = inject(DestroyRef);

  protected isLoading = signal(true);
  protected feedbackStates = signal<CourseFeedbackState[]>([]);

  // Array used to render 5 stars
  protected stars = [1, 2, 3, 4, 5];

  ngOnInit(): void {
    this.loadEligibleCourses();
  }

  private loadEligibleCourses(): void {
    this.isLoading.set(true);

    this.enrollmentService.getMyEnrollments()
      .pipe(untilDestroyed(this.destroyRef))
      .subscribe({
        next: (enrollments) => {
          // Filter to only enrolled courses with progress > 0
          const eligible = (enrollments || []).filter(e => e.progressPercentage > 0 || e.isCompleted);
          
          if (eligible.length === 0) {
            this.feedbackStates.set([]);
            this.isLoading.set(false);
            return;
          }

          const currentUserEmail = this.authService.currentUser()?.email;

          // Fetch reviews for all eligible courses to see if user has already reviewed them
          const reviewRequests = eligible.map(course => 
            this.reviewService.getCourseReviews(course.courseId).pipe(
              catchError(() => of([] as ReviewResponse[]))
            )
          );

          forkJoin(reviewRequests).pipe(untilDestroyed(this.destroyRef)).subscribe({
            next: (reviewsArray) => {
              const states: CourseFeedbackState[] = eligible.map((course, index) => {
                const reviews = reviewsArray[index];
                const myReview = reviews.find(r => r.userName === currentUserEmail) || null;

                return {
                  course,
                  myReview,
                  isSubmitting: signal(false),
                  ratingForm: myReview ? myReview.rating : 0,
                  textForm: myReview ? myReview.reviewText : ''
                };
              });

              // Sort states: Courses without reviews first, then by recent enrollment
              states.sort((a, b) => {
                if (a.myReview && !b.myReview) return 1;
                if (!a.myReview && b.myReview) return -1;
                return new Date(b.course.enrolledAt).getTime() - new Date(a.course.enrolledAt).getTime();
              });

              this.feedbackStates.set(states);
              this.isLoading.set(false);
            },
            error: (err) => {
              this.toastService.showApiError(err, 'Failed to load reviews.');
              this.isLoading.set(false);
            }
          });
        },
        error: (err) => {
          this.toastService.showApiError(err, 'Failed to load your courses.');
          this.isLoading.set(false);
        }
      });
  }

  protected setRating(state: CourseFeedbackState, rating: number): void {
    state.ratingForm = rating;
  }

  protected submitFeedback(state: CourseFeedbackState): void {
    if (state.ratingForm < 1 || state.ratingForm > 5) {
      this.toastService.showWarning('Please select a rating between 1 and 5 stars.');
      return;
    }
    if (!state.textForm || state.textForm.trim().length === 0) {
      this.toastService.showWarning('Please enter a review text.');
      return;
    }

    state.isSubmitting.set(true);

    if (state.myReview) {
      // Update existing review
      this.reviewService.updateReview(state.myReview.id, {
        rating: state.ratingForm,
        reviewText: state.textForm.trim()
      }).pipe(
        finalize(() => state.isSubmitting.set(false)),
        untilDestroyed(this.destroyRef)
      ).subscribe({
        next: (updatedReview) => {
          state.myReview = updatedReview;
          this.toastService.showSuccess('Review updated successfully!');
        },
        error: (err) => {
          this.toastService.showApiError(err, 'Failed to update review.');
        }
      });
    } else {
      // Create new review
      this.reviewService.submitReview({
        courseId: state.course.courseId,
        rating: state.ratingForm,
        reviewText: state.textForm.trim()
      }).pipe(
        finalize(() => state.isSubmitting.set(false)),
        untilDestroyed(this.destroyRef)
      ).subscribe({
        next: (newReview) => {
          state.myReview = newReview;
          this.toastService.showSuccess('Thank you for your feedback!');
        },
        error: (err) => {
          this.toastService.showApiError(err, 'Failed to submit review.');
        }
      });
    }
  }

  protected formatDuration(isoStr: string): string {
    if (!isoStr) return '';
    const parts = isoStr.split(':');
    const h = parseInt(parts[0] ?? '0', 10);
    const m = parseInt(parts[1] ?? '0', 10);
    if (h > 0 && m > 0) return `${h}h ${m}m`;
    if (h > 0) return `${h}h`;
    return `${m}m`;
  }
}
