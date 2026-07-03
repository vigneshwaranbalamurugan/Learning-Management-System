import { Component, inject, NgZone, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute, RouterModule } from '@angular/router';
import { AuthService } from '@services/auth.service';
import { ToastService } from '@services/toast.service';
import { ConfirmModal } from '@components/confirm-modal/confirm-modal';
import { FormInput } from '@components/form-input/form-input';
import { Dropdown } from '@components/dropdown/dropdown';
import { Loader } from '@components/loader/loader';
import { QuizService } from '@services/quiz.service';
import { QuizQuestionType } from '../../enums/quiz-question-type.enum';

interface QuizOption {
  id?: number;
  optionText: string;
  isCorrect: boolean;
}

interface QuizQuestion {
  id?: number;
  questionText: string;
  questionType: number;
  mark: number;
  explanation: string;
  sortOrder: number;
  options: QuizOption[];
}

@Component({
  selector: 'app-instructor-quiz-questions',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, ConfirmModal, FormInput, Dropdown, Loader],
  templateUrl: './instructor-quiz-questions.html',
  styleUrl: './instructor-quiz-questions.css'
})
export class InstructorQuizQuestions implements OnInit {
  private toastService = inject(ToastService);
  private quizService = inject(QuizService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private authService = inject(AuthService);

  protected get routePrefix(): string {
    return this.authService.userRole()?.toLowerCase() || 'instructor';
  }
  private ngZone = inject(NgZone);

  protected courseSlug = '';
  protected quizId: number | null = null;
  protected quizDetails: any = null;

  protected showQuestionModal = false;
  protected isEditingQuestion = false;
  protected isSaving = signal(false);
  protected isLoading = signal(true);
  protected currentQuestion: QuizQuestion = this.getEmptyQuestion();

  protected showUnsavedModal = signal(false);
  private unsavedResolve: ((val: boolean) => void) | null = null;

  protected isLocked = signal(false);
  protected get isDirty(): boolean {
    return this.showQuestionModal;
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

  protected questionTypeOptions = [
    { value: String(QuizQuestionType.MultipleChoice), label: 'Multiple Choice' },
    { value: String(QuizQuestionType.TrueFalse), label: 'True/False' }
  ];

  protected readonly QuizQuestionType = QuizQuestionType;

  // Confirmation Modal state
  protected showDeleteModal = false;
  protected questionToDelete: number | null = null;

  // Drag-and-drop state
  protected dragIndex: number | null = null;
  protected dragOverIndex: number | null = null;

  // Auto-scroll during drag
  private autoScrollRafId: number | null = null;
  private dragClientY = 0;
  private readonly SCROLL_ZONE = 100;  // px from edge to trigger scroll
  private readonly SCROLL_SPEED = 12; // max px per frame

  ngOnInit() {
    this.route.queryParams.subscribe(params => {
      this.isLocked.set(params['locked'] === 'true');
    });

    this.route.parent?.paramMap.subscribe(parentParams => {
      this.courseSlug = parentParams.get('slug') || '';
    });

    this.route.paramMap.subscribe(params => {
      this.quizId = Number(params.get('quizId'));
      if (this.quizId) {
        this.loadQuiz(this.quizId);
      }
    });
  }

  private loadQuiz(quizId: number) {
    this.isLoading.set(true);
    this.quizService.getQuiz(quizId).subscribe({
      next: (quiz) => {
        if (quiz && quiz.questions) {
          quiz.questions.sort((a: any, b: any) => a.sortOrder - b.sortOrder);
        }
        this.quizDetails = quiz;
        this.isLoading.set(false);
      },
      error: (err) => {
        this.toastService.showApiError(err, 'Failed to load quiz details.');
        this.isLoading.set(false);
      }
    });
  }

  protected navigateBack() {
    if (this.courseSlug) {
      this.router.navigate([`/${this.routePrefix}/courses`, this.courseSlug, 'quizzes']);
    }
  }

  private getEmptyQuestion(): QuizQuestion {
    return {
      questionText: '',
      questionType: QuizQuestionType.MultipleChoice,
      mark: 1,
      explanation: '',
      sortOrder: 1,
      options: [
        { optionText: '', isCorrect: false },
        { optionText: '', isCorrect: false },
        { optionText: '', isCorrect: false },
        { optionText: '', isCorrect: false }
      ]
    };
  }

  protected openAddQuestion() {
    this.currentQuestion = this.getEmptyQuestion();
    const questions = this.quizDetails?.questions || [];
    const maxSortOrder = questions.reduce((max: number, q: any) => q.sortOrder > max ? q.sortOrder : max, 0);
    this.currentQuestion.sortOrder = maxSortOrder + 1;
    
    this.isEditingQuestion = false;
    this.showQuestionModal = true;
  }

  protected editQuestion(question: any) {
    this.currentQuestion = {
      id: question.id,
      questionText: question.questionText,
      questionType: question.questionType,
      mark: question.mark,
      explanation: question.explanation || '',
      sortOrder: question.sortOrder,
      options: question.options.map((o: any) => ({ ...o }))
    };
    this.isEditingQuestion = true;
    this.showQuestionModal = true;
  }

  protected closeQuestionModal() {
    this.showQuestionModal = false;
  }

  protected addOption() {
    if (this.currentQuestion.questionType == QuizQuestionType.MultipleChoice) {
      if (this.currentQuestion.options.length < 4) {
        this.currentQuestion.options.push({ optionText: '', isCorrect: false });
      } else {
        this.toastService.showError('A multiple choice question can only have exactly 4 options.');
      }
    } else {
      this.currentQuestion.options.push({ optionText: '', isCorrect: false });
    }
  }

  protected removeOption(index: number) {
    if (this.currentQuestion.questionType == QuizQuestionType.MultipleChoice) {
      this.toastService.showError('A multiple choice question must have exactly 4 options.');
      return;
    }
    
    if (this.currentQuestion.options.length > 2) {
      this.currentQuestion.options.splice(index, 1);
    } else {
      this.toastService.showError('A question must have at least 2 options.');
    }
  }

  protected onQuestionTypeChange() {
    if (this.currentQuestion.questionType == QuizQuestionType.TrueFalse) {
      this.currentQuestion.options = [
        { optionText: 'True', isCorrect: true },
        { optionText: 'False', isCorrect: false }
      ];
    } else {
      // Reset back to exactly 4 Multiple choice options
      this.currentQuestion.options = [
        { optionText: '', isCorrect: false },
        { optionText: '', isCorrect: false },
        { optionText: '', isCorrect: false },
        { optionText: '', isCorrect: false }
      ];
    }
  }

  protected setQuestionType(val: string) {
    this.currentQuestion.questionType = parseInt(val, 10);
    this.onQuestionTypeChange();
  }

  protected saveQuestion() {
    if (!this.quizId) return;
    if (!this.currentQuestion.questionText.trim()) {
      this.toastService.showError('Question text is required.');
      return;
    }
    
    const hasCorrectOption = this.currentQuestion.options.some(o => o.isCorrect);
    if (!hasCorrectOption) {
      this.toastService.showError('Please mark at least one option as correct.');
      return;
    }
    
    const hasEmptyOption = this.currentQuestion.options.some(o => !o.optionText.trim());
    if (hasEmptyOption) {
      this.toastService.showError('Option text cannot be empty.');
      return;
    }

    const payload = {
      quizId: this.quizId,
      questionText: this.currentQuestion.questionText,
      questionType: Number(this.currentQuestion.questionType),
      mark: Number(this.currentQuestion.mark),
      explanation: this.currentQuestion.explanation,
      sortOrder: Number(this.currentQuestion.sortOrder),
      options: this.currentQuestion.options.map(o => ({
        optionText: o.optionText,
        isCorrect: o.isCorrect
      }))
    };

    this.isSaving.set(true);

    if (this.isEditingQuestion && this.currentQuestion.id) {
      this.quizService.updateQuizQuestion(this.currentQuestion.id, payload).subscribe({
        next: () => {
          this.toastService.showSuccess('Question updated successfully.');
          this.loadQuiz(this.quizId!);
          this.closeQuestionModal();
          this.isSaving.set(false);
        },
        error: (err) => {
          this.toastService.showApiError(err, 'Failed to update question.');
          this.isSaving.set(false);
        }
      });
    } else {
      this.quizService.addQuizQuestion(this.quizId, payload).subscribe({
        next: () => {
          this.toastService.showSuccess('Question added successfully.');
          this.loadQuiz(this.quizId!);
          this.closeQuestionModal();
          this.isSaving.set(false);
        },
        error: (err) => {
          this.toastService.showApiError(err, 'Failed to add question.');
          this.isSaving.set(false);
        }
      });
    }
  }

  protected confirmDeleteQuestion(questionId: number) {
    this.questionToDelete = questionId;
    this.showDeleteModal = true;
  }

  protected closeDeleteModal() {
    this.showDeleteModal = false;
    this.questionToDelete = null;
  }

  protected deleteQuestion() {
    if (this.questionToDelete !== null) {
      this.quizService.deleteQuizQuestion(this.questionToDelete).subscribe({
        next: () => {
          this.toastService.showSuccess('Question deleted successfully.');
          this.loadQuiz(this.quizId!);
          this.closeDeleteModal();
        },
        error: (err) => {
          this.toastService.showApiError(err, 'Failed to delete question.');
          this.closeDeleteModal();
        }
      });
    }
  }

  protected reorderQuestion(index: number, direction: 'up' | 'down') {
    const questions = [...this.quizDetails.questions];
    const targetIndex = direction === 'up' ? index - 1 : index + 1;
    if (targetIndex < 0 || targetIndex >= questions.length) return;

    // Swap the two items locally
    [questions[index], questions[targetIndex]] = [questions[targetIndex], questions[index]];

    // Reassign sequential sort orders
    questions.forEach((q, i) => { q.sortOrder = i + 1; });

    // Optimistic update
    this.quizDetails = { ...this.quizDetails, questions };

    // Persist via bulk reorder endpoint
    this.reorderViaApi(questions);
  }

  // ── Drag & Drop ───────────────────────────────────────────────────────────

  protected onDragStart(index: number) {
    this.dragIndex = index;
    this.startAutoScroll();
  }

  protected onDragOver(event: DragEvent, index: number) {
    event.preventDefault();
    this.dragOverIndex = index;
    this.dragClientY = event.clientY;
  }

  protected onDragLeave() {
    this.dragOverIndex = null;
  }

  protected onDrop(event: DragEvent, dropIndex: number) {
    event.preventDefault();
    const fromIndex = this.dragIndex;
    this.dragIndex = null;
    this.dragOverIndex = null;

    if (fromIndex === null || fromIndex === dropIndex) return;

    const questions = [...this.quizDetails.questions];
    const [moved] = questions.splice(fromIndex, 1);
    questions.splice(dropIndex, 0, moved);

    // Reassign sequential sortOrders based on new positions
    questions.forEach((q, i) => { q.sortOrder = i + 1; });

    // Optimistic UI update
    this.quizDetails = { ...this.quizDetails, questions };

    // Persist via bulk reorder endpoint (no collision issues)
    this.reorderViaApi(questions);
  }

  protected onDragEnd() {
    this.dragIndex = null;
    this.dragOverIndex = null;
    this.stopAutoScroll();
  }

  private startAutoScroll() {
    this.stopAutoScroll();
    // Run outside Angular zone to avoid unnecessary CD cycles
    this.ngZone.runOutsideAngular(() => {
      const loop = () => {
        const y = this.dragClientY;
        const vh = window.innerHeight;
        const scrollEl = document.scrollingElement ?? document.documentElement;

        if (y < this.SCROLL_ZONE) {
          // Near top — scroll up
          const ratio = 1 - (y / this.SCROLL_ZONE);
          scrollEl.scrollTop -= this.SCROLL_SPEED * ratio;
        } else if (y > vh - this.SCROLL_ZONE) {
          // Near bottom — scroll down
          const ratio = (y - (vh - this.SCROLL_ZONE)) / this.SCROLL_ZONE;
          scrollEl.scrollTop += this.SCROLL_SPEED * ratio;
        }

        this.autoScrollRafId = requestAnimationFrame(loop);
      };
      this.autoScrollRafId = requestAnimationFrame(loop);
    });
  }

  private stopAutoScroll() {
    if (this.autoScrollRafId !== null) {
      cancelAnimationFrame(this.autoScrollRafId);
      this.autoScrollRafId = null;
    }
  }

  private reorderViaApi(questions: any[]) {
    if (!this.quizId) return;
    const items = questions.map(q => ({ questionId: q.id, sortOrder: q.sortOrder }));
    this.isSaving.set(true);
    this.quizService.reorderQuizQuestions(this.quizId, items).subscribe({
      next: () => {
        this.isSaving.set(false);
        this.toastService.showSuccess('Question order saved.');
      },
      error: (err) => {
        this.toastService.showApiError(err, 'Failed to save order. Refreshing...');
        this.loadQuiz(this.quizId!);
        this.isSaving.set(false);
      }
    });
  }
}
