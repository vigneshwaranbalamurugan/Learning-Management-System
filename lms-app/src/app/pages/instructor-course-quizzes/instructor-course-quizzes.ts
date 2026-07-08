import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute, RouterModule } from '@angular/router';
import { InstructorCourseLayout } from '../instructor-course-layout/instructor-course-layout';
import { AuthService } from '@services/auth.service';
import { ToastService } from '@services/toast.service';
import { ConfirmModal } from '@components/confirm-modal/confirm-modal';
import { FormInput } from '@components/form-input/form-input';
import { Dropdown } from '@components/dropdown/dropdown';
import { Button } from '@components/button/button';
import { QuizService } from '@services/quiz.service';
@Component({
  selector: 'app-instructor-course-quizzes',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, ConfirmModal, FormInput, Dropdown, Button],
  templateUrl: './instructor-course-quizzes.html'
})
export class InstructorCourseQuizzes {
  protected layout = inject(InstructorCourseLayout);
  private toastService = inject(ToastService);
  private authService = inject(AuthService);

  protected get routePrefix(): string {
    return this.authService.userRole()?.toLowerCase() || 'instructor';
  }
  private quizService = inject(QuizService);
  private router = inject(Router);

  // Modal states
  protected showQuizModal = signal(false);
  protected editingQuizId: number | null = null;
  protected quizTitle = '';
  protected quizDescription = '';
  protected quizTimeLimit = '00:30:00';
  protected quizPassingPercentage = 60;
  protected quizMaxAttempts = 3;
  protected quizSectionId: number | null = null;
  protected quizDeadlineInDays = 0;

  protected showUnsavedModal = signal(false);
  private unsavedResolve: ((val: boolean) => void) | null = null;
  private initialFormState = '';
  protected isSaving = signal(false);
  protected errors: Record<string, string> = {};

  private captureInitialState() {
    this.initialFormState = JSON.stringify({
      quizSectionId: this.quizSectionId,
      quizTitle: this.quizTitle,
      quizDescription: this.quizDescription,
      quizTimeLimit: this.quizTimeLimit,
      quizPassingPercentage: this.quizPassingPercentage,
      quizMaxAttempts: this.quizMaxAttempts,
      quizDeadlineInDays: this.quizDeadlineInDays
    });
  }

  protected get isDirty(): boolean {
    if (!this.showQuizModal()) return false;
    const currentState = JSON.stringify({
      quizSectionId: this.quizSectionId,
      quizTitle: this.quizTitle,
      quizDescription: this.quizDescription,
      quizTimeLimit: this.quizTimeLimit,
      quizPassingPercentage: this.quizPassingPercentage,
      quizMaxAttempts: this.quizMaxAttempts,
      quizDeadlineInDays: this.quizDeadlineInDays
    });
    return currentState !== this.initialFormState;
  }

  async canDeactivate(): Promise<boolean> {
    if (!this.isDirty || this.isSaving()) return true;

    return new Promise<boolean>((resolve) => {
      this.unsavedResolve = resolve;
      this.showUnsavedModal.set(true);
    });
  }

  protected confirmLeave(): void {
    this.showUnsavedModal.set(false);
    if (this.unsavedResolve) {
      this.unsavedResolve(true);
      this.unsavedResolve = null;
    }
  }

  protected cancelLeave(): void {
    this.showUnsavedModal.set(false);
    if (this.unsavedResolve) {
      this.unsavedResolve(false);
      this.unsavedResolve = null;
    }
  }

  protected get quizSectionIdStr(): string {
    return this.quizSectionId ? String(this.quizSectionId) : '';
  }

  protected setQuizSectionIdStr(val: string) {
    this.quizSectionId = val ? Number(val) : null;
  }

  protected get sectionOptions() {
    return (this.course?.sections || []).map(sec => ({
      value: String(sec.id),
      label: sec.title
    }));
  }

  protected get quizPassingPercentageStr(): string {
    return String(this.quizPassingPercentage);
  }

  protected set quizPassingPercentageStr(val: string) {
    this.quizPassingPercentage = Number(val);
  }

  protected get quizMaxAttemptsStr(): string {
    return String(this.quizMaxAttempts);
  }

  protected set quizMaxAttemptsStr(val: string) {
    this.quizMaxAttempts = Number(val);
  }

  protected get quizDeadlineInDaysStr(): string {
    return String(this.quizDeadlineInDays);
  }

  protected set quizDeadlineInDaysStr(val: string) {
    this.quizDeadlineInDays = Number(val);
  }

  // Confirmation Modal state
  protected showDeleteModal = false;
  protected quizToDelete: number | null = null;

  protected get course() {
    return this.layout.course();
  }

  protected openAddQuiz() {
    if (this.course?.sections && this.course.sections.length > 0) {
      this.quizSectionId = this.course.sections[0].id;
    } else {
      this.toastService.showError('Please create a section first before adding a quiz.');
      return;
    }
    this.editingQuizId = null;
    this.quizTitle = '';
    this.quizDescription = '';
    this.quizTimeLimit = '00:30:00';
    this.quizPassingPercentage = 60;
    this.quizMaxAttempts = 3;
    this.quizDeadlineInDays = 0;
    this.captureInitialState();
    this.showQuizModal.set(true);
  }

  protected async closeQuizModal() {
    const canClose = await this.canDeactivate();
    if (canClose) {
      this.showQuizModal.set(false);
    }
  }

  protected openEditQuiz(quiz: any, sectionId: number) {
    this.editingQuizId = quiz.id;
    this.quizSectionId = sectionId;
    this.quizTitle = quiz.title;
    this.quizDescription = quiz.description || '';
    this.quizTimeLimit = quiz.timeLimit || '00:30:00';
    this.quizPassingPercentage = quiz.passingPercentage || 60;
    this.quizMaxAttempts = quiz.maxAttempts || 3;
    this.quizDeadlineInDays = quiz.deadlineInDays || 0;
    this.captureInitialState();
    this.showQuizModal.set(true);
  }

  protected saveQuiz() {
    this.errors = {};
    if (!this.course || !this.quizSectionId) return;

    let isValid = true;
    
    if (!this.quizTitle.trim()) {
      this.toastService.showError('Quiz title is required.');
      this.errors['title'] = 'Title is required.';
      isValid = false;
    }
    const timeLimitRegex = /^([01]?[0-9]|2[0-3]):[0-5][0-9]:[0-5][0-9]$/;
    if (!timeLimitRegex.test(this.quizTimeLimit)) {
      this.toastService.showError('Time limit must be in HH:mm:ss format.');
      this.errors['timeLimit'] = 'Format: HH:mm:ss';
      isValid = false;
    } else if (this.quizTimeLimit === '00:00:00' || this.quizTimeLimit === '00:00') {
      this.toastService.showError('Time limit must be greater than 0.');
      this.errors['timeLimit'] = 'Must be > 00:00:00';
      isValid = false;
    }
    if (this.quizPassingPercentage <= 0 || this.quizPassingPercentage > 100) {
      this.toastService.showError('Passing percentage must be between 1 and 100.');
      this.errors['passingPercentage'] = 'Must be 1-100';
      isValid = false;
    }
    if (this.quizMaxAttempts < 1) {
      this.toastService.showError('Max attempts must be at least 1.');
      this.errors['maxAttempts'] = 'At least 1';
      isValid = false;
    }
    if (this.quizDeadlineInDays < 0) {
      this.toastService.showError('Deadline in days cannot be negative.');
      this.errors['deadlineInDays'] = 'Cannot be negative';
      isValid = false;
    }

    if (!isValid) return;

    let data: any = {};
    if (this.editingQuizId) {
      const initialState = this.initialFormState ? JSON.parse(this.initialFormState) : {};
      if (this.quizSectionId !== initialState.quizSectionId) data.courseSectionId = this.quizSectionId;
      if (this.quizTitle !== initialState.quizTitle) data.title = this.quizTitle;
      if (this.quizDescription !== initialState.quizDescription) data.description = this.quizDescription;
      if (this.quizTimeLimit !== initialState.quizTimeLimit) data.timeLimit = this.quizTimeLimit;
      if (this.quizPassingPercentage !== initialState.quizPassingPercentage) data.passingPercentage = this.quizPassingPercentage;
      if (this.quizMaxAttempts !== initialState.quizMaxAttempts) data.maxAttempts = this.quizMaxAttempts;
      if (this.quizDeadlineInDays !== initialState.quizDeadlineInDays) data.deadlineInDays = this.quizDeadlineInDays;
    } else {
      data = {
        courseSectionId: this.quizSectionId,
        title: this.quizTitle,
        description: this.quizDescription,
        timeLimit: this.quizTimeLimit,
        passingPercentage: this.quizPassingPercentage,
        maxAttempts: this.quizMaxAttempts,
        order: 1,
        deadlineInDays: this.quizDeadlineInDays
      };
    }

    if (this.editingQuizId) {
      this.isSaving.set(true);
      this.quizService.updateQuiz(this.editingQuizId, data).subscribe({
        next: () => {
          this.isSaving.set(false);
          this.toastService.showSuccess('Quiz updated successfully.');
          this.layout.loadCourse(this.course!.id);
          this.closeQuizModal();
        },
        error: (err) => {
          this.isSaving.set(false);
          this.toastService.showApiError(err, 'Failed to update quiz.');
        }
      });
    } else {
      this.isSaving.set(true);
      this.quizService.createQuiz(data).subscribe({
        next: () => {
          this.isSaving.set(false);
          this.toastService.showSuccess('Quiz created successfully.');
          this.layout.loadCourse(this.course!.id);
          this.closeQuizModal();
        },
        error: (err) => {
          this.isSaving.set(false);
          this.toastService.showApiError(err, 'Failed to create quiz.');
        }
      });
    }
  }

  protected confirmDeleteQuiz(quizId: number) {
    this.quizToDelete = quizId;
    this.showDeleteModal = true;
  }

  protected deleteQuiz() {
    if (!this.course || this.quizToDelete === null) return;
    this.quizService.deleteQuiz(this.quizToDelete).subscribe({
      next: () => {
        this.toastService.showSuccess('Quiz deleted successfully.');
        this.layout.loadCourse(this.course!.id);
        this.closeDeleteModal();
      },
      error: (err) => {
        this.toastService.showApiError(err, 'Failed to delete quiz.');
        this.closeDeleteModal();
      }
    });
  }

  protected closeDeleteModal() {
    this.showDeleteModal = false;
    this.quizToDelete = null;
  }

  protected editQuiz(quizId: number) {
    if (!this.course) return;
    this.router.navigate([`/${this.routePrefix}/courses`, this.course.slug, 'quizzes', quizId, 'questions'], {
      queryParams: { locked: this.course.hasNonExpiredEnrollments ? 'true' : 'false' }
    });
  }
}
