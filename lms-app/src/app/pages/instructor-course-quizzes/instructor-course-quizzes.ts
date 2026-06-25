import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { InstructorCourseLayout } from '../instructor-course-layout/instructor-course-layout';
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
  private quizService = inject(QuizService);
  private router = inject(Router);

  // Modal states
  protected showQuizModal = signal(false);
  protected quizTitle = '';
  protected quizDescription = '';
  protected quizTimeLimit = '00:30:00';
  protected quizPassingPercentage = 60;
  protected quizMaxAttempts = 3;
  protected quizSectionId: number | null = null;
  protected quizDeadlineInDays = 0;

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
    this.quizTitle = '';
    this.quizDescription = '';
    this.quizTimeLimit = '00:30:00';
    this.quizPassingPercentage = 60;
    this.quizMaxAttempts = 3;
    this.quizDeadlineInDays = 0;
    this.showQuizModal.set(true);
  }

  protected closeQuizModal() {
    this.showQuizModal.set(false);
  }

  protected saveQuiz() {
    if (!this.course || !this.quizSectionId) return;
    if (!this.quizTitle.trim()) {
      this.toastService.showError('Quiz title is required.');
      return;
    }

    const data = {
      courseSectionId: this.quizSectionId,
      title: this.quizTitle,
      description: this.quizDescription,
      timeLimit: this.quizTimeLimit,
      passingPercentage: this.quizPassingPercentage,
      maxAttempts: this.quizMaxAttempts,
      order: 1,
      deadlineInDays: this.quizDeadlineInDays
    };

    this.quizService.createQuiz(data).subscribe({
      next: () => {
        this.toastService.showSuccess('Quiz created successfully.');
        this.layout.loadCourse(this.course!.id);
        this.closeQuizModal();
      },
      error: (err) => this.toastService.showApiError(err, 'Failed to create quiz.')
    });
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
    this.router.navigate(['/instructor/courses', this.course.slug, 'quizzes', quizId, 'questions']);
  }
}
