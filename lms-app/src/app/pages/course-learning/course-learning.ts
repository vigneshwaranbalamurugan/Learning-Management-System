import { Component, OnInit, OnDestroy, signal, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, ActivatedRoute } from '@angular/router';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { environment } from '@environments/environment';

import { CourseService } from '@services/course.service';
import { ProgressService } from '@services/progress.service';
import { LearningService } from '@services/learning.service';
import { VideoProgressSignalRService } from '@services/video-progress-signalr.service';
import { QuizProgressSignalRService } from '@services/quiz-progress-signalr.service';
import { AssignmentService } from '@services/assignment.service';
import { EnrollmentService } from '@services/enrollment.service';
import { SecureMediaService } from '@services/secure-media.service';
import { ToastService } from '@services/toast.service';

import { CourseDetailResponse, LessonSummary } from '@models/course';
import { CourseProgressResponse, LessonProgressResponse, QuizProgressResponse, AssignmentProgressResponse } from '@models/progress';
import { QuizStudentDetailResponse, QuizAttemptResponse, GetRemainingAttemptsResponse } from '@models/quiz';
import { AssignmentResponse, AssignmentStatusResponse, AssignmentSubmissionResponse } from '@models/assignment';
import { LessonType } from '../../enums/lesson-types.enum';
import { AssignmentAttachmentType } from '../../enums/assignment-attachment-type.enum';

import { Button } from '@components/button/button';
import { Loader } from '@components/loader/loader';
import { FormInput } from '@components/form-input/form-input';
import { ConfirmModal } from '@components/confirm-modal/confirm-modal';
import { ConfettiComponent } from '@components/confetti/confetti';
import { VideoPlayer } from '@components/video-player/video-player';
import { PdfViewer } from '@components/pdf-viewer/pdf-viewer';

import { LessonDiscussions } from '@components/lesson-discussions/lesson-discussions';
import { AiTutorChat } from '@components/ai-tutor-chat/ai-tutor-chat';
import { AiLessonSummary } from '@components/ai-lesson-summary/ai-lesson-summary';

// Marked for markdown
import { marked } from 'marked';

interface FlatItem {
  type: 'lesson' | 'quiz' | 'assignment';
  id: number;
  sectionId: number;
  sectionTitle: string;
  title: string;
  duration?: string;
  lessonType?: number;
  isCompleted: boolean;
  isLocked: boolean;
  item: any;
}

@Component({
  selector: 'app-course-learning',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule,
    Button, Loader, FormInput, ConfirmModal, ConfettiComponent, VideoPlayer, PdfViewer,
    LessonDiscussions, AiTutorChat, AiLessonSummary
  ],
  templateUrl: './course-learning.html',
  styleUrl: './course-learning.css'
})
export class CourseLearning implements OnInit, OnDestroy {
  private courseService = inject(CourseService);
  private progressService = inject(ProgressService);
  private learningService = inject(LearningService);
  private videoSignalR = inject(VideoProgressSignalRService);
  private quizSignalR = inject(QuizProgressSignalRService);
  private assignmentService = inject(AssignmentService);
  private enrollmentService = inject(EnrollmentService);
  private secureMediaService = inject(SecureMediaService);
  private toast = inject(ToastService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private sanitizer = inject(DomSanitizer);
  private fb = inject(FormBuilder);

  // Core State
  isLoading = signal(true);
  courseId = signal<number>(0);
  course = signal<CourseDetailResponse | null>(null);
  progress = signal<CourseProgressResponse | null>(null);

  // Sidebar State
  sidebarOpen = signal(true);
  searchQuery = signal('');

  // Navigation State
  flatCurriculum = signal<FlatItem[]>([]);
  activeItemIndex = signal<number>(-1);
  activeItem = computed(() => {
    const items = this.flatCurriculum();
    const idx = this.activeItemIndex();
    return idx >= 0 && idx < items.length ? items[idx] : null;
  });

  // Action State
  isMarkingComplete = signal<boolean>(false);
  showExitModal = signal(false);
  showUpdateModal = signal(false);
  isUpdatingVersion = signal(false);

  // ─── Computed Progress ────────────────────────────────────────────────────

  lessonProgress = computed(() => {
    const items = this.flatCurriculum().filter(x => x.type === 'lesson');
    return {
      completed: items.filter(x => x.isCompleted).length,
      total: items.length,
      percentage: items.length ? Math.round((items.filter(x => x.isCompleted).length / items.length) * 100) : 0
    };
  });

  quizProgress = computed(() => {
    const items = this.flatCurriculum().filter(x => x.type === 'quiz');
    return {
      completed: items.filter(x => x.isCompleted).length,
      total: items.length,
      percentage: items.length ? Math.round((items.filter(x => x.isCompleted).length / items.length) * 100) : 0
    };
  });

  assignmentProgress = computed(() => {
    const items = this.flatCurriculum().filter(x => x.type === 'assignment');
    return {
      completed: items.filter(x => x.isCompleted).length,
      total: items.length,
      percentage: items.length ? Math.round((items.filter(x => x.isCompleted).length / items.length) * 100) : 0
    };
  });

  // Completion State
  showConfetti = signal(false);
  confettiTitle = signal('🎉 Course Completed!');
  confettiMessage = signal('Congratulations! You have completed the course.');

  // Resource State
  absolutePdfUrl = signal<string | null>(null);
  sanitizedLinkUrl = signal<SafeResourceUrl | null>(null);
  parsedArticleHtml = signal<string>('');

  // Video State
  videoResumeSecond = signal(0);
  videoMaxWatchedSecond = signal(0);
  parsedVideoDescriptionHtml = signal<string>('');
  secureVideoUrl = signal<string | null>(null);
  private lastSignalREmit = 0;

  // Accordion State
  expandedSections = signal<Set<number>>(new Set());

  // Quiz State
  quizDetail = signal<QuizStudentDetailResponse | null>(null);
  remainingAttempts = signal<GetRemainingAttemptsResponse | null>(null);
  quizAttemptId = signal<number | null>(null);
  quizTimeLeft = signal<number>(0); // in seconds
  selectedAnswers = signal<Map<number, number>>(new Map());
  isSubmittingQuiz = signal(false);
  quizResult = signal<QuizAttemptResponse | null>(null);
  private quizTimer: any;

  // Assignment State
  assignmentDetail = signal<AssignmentResponse | null>(null);
  assignmentStatus = signal<AssignmentStatusResponse | null>(null);
  mySubmissions = signal<AssignmentSubmissionResponse[]>([]);
  assignmentForm: FormGroup;
  isSubmittingAssignment = signal(false);
  selectedAssignmentFile: File | null = null;

  isFullScreen = signal(false);

  protected readonly LessonType = LessonType;
  protected readonly AssignmentAttachmentType = AssignmentAttachmentType;

  constructor() {
    this.assignmentForm = this.fb.group({
      submissionType: ['none'],
      submissionText: [''],
      attachmentUrl: ['']
    });
  }

  ngOnInit() {
    this.route.paramMap.subscribe(params => {
      const courseIdStr = params.get('courseId');
      if (courseIdStr) {
        this.courseId.set(+courseIdStr);
        this.loadCourseData();
      }
    });

    // Handle SignalR Video Progress
    this.videoSignalR.resumePosition$.subscribe(data => {
      this.videoResumeSecond.set(data.lastWatchedSecond);
      this.videoMaxWatchedSecond.set(data.maxWatchedSecond);
    });

    this.videoSignalR.lessonCompleted$.subscribe(data => {
      if (data && data.lessonId) {
        // Auto-mark complete by re-fetching progress
        this.fetchLatestProgress();
      }
    });
  }

  async ngOnDestroy() {
    await this.videoSignalR.disconnect();
    await this.quizSignalR.disconnect();
    this.stopQuizTimer();
  }

  // ─── Data Loading ─────────────────────────────────────────────────────────

  private async loadCourseData() {
    this.isLoading.set(true);
    try {
      const [courseRes, progressRes] = await Promise.all([
        this.courseService.getCourseById(this.courseId()).toPromise(),
        this.progressService.getCourseProgress(this.courseId()).toPromise()
      ]);

      this.course.set(courseRes!);
      this.progress.set(progressRes!);

      // Expand all sections by default
      this.expandedSections.set(new Set(courseRes!.sections.map(s => s.id)));

      this.buildFlatCurriculum();
      this.resolveActiveRouteItem();
    } catch (err) {
      console.error('Failed to load course data', err);
    } finally {
      this.isLoading.set(false);
    }
  }

  private buildFlatCurriculum() {
    const c = this.course();
    const p = this.progress();
    if (!c || !p) return;

    const flat: FlatItem[] = [];
    let previousCompleted = true; // First item is always unlocked

    for (const section of c.sections) {
      const secProg = p.sections.find(s => s.sectionId === section.id);

      // Merge lessons, quizzes, assignments into one sorted array
      const items: any[] = [];
      if (section.lessons) items.push(...section.lessons.map(l => ({ ...l, _type: 'lesson', _sort: l.sortOrder })));
      if (section.quizzes) items.push(...section.quizzes.map(q => ({ ...q, _type: 'quiz', _sort: q.order })));
      if (section.assignments) items.push(...section.assignments.map(a => ({ ...a, _type: 'assignment', _sort: a.id })));

      items.sort((a, b) => a._sort - b._sort);

      for (const item of items) {
        let isCompleted = false;

        if (item._type === 'lesson') {
          isCompleted = !!secProg?.lessons.find(l => l.lessonId === item.id)?.isCompleted;
        } else if (item._type === 'quiz') {
          isCompleted = !!secProg?.quizzes.find(q => q.quizId === item.id)?.isPassed;
        } else if (item._type === 'assignment') {
          isCompleted = !!secProg?.assignments.find(a => a.assignmentId === item.id)?.isPassed;
        }

        const isLocked = !previousCompleted;

        flat.push({
          type: item._type,
          id: item.id,
          sectionId: section.id,
          sectionTitle: section.title,
          title: item.title,
          duration: item.durationInMinutes || item.timeLimit || '',
          lessonType: item.type,
          isCompleted,
          isLocked,
          item
        });

        // Enforce sequential logic
        previousCompleted = isCompleted;
      }
    }

    this.flatCurriculum.set(flat);
  }

  private resolveActiveRouteItem() {
    const url = this.router.url;
    let typeToFind: string | null = null;
    let idToFind: number | null = null;

    if (url.includes('/lesson/')) {
      typeToFind = 'lesson';
      idToFind = +this.route.snapshot.paramMap.get('lessonId')!;
    } else if (url.includes('/quiz/')) {
      typeToFind = 'quiz';
      idToFind = +this.route.snapshot.paramMap.get('quizId')!;
    } else if (url.includes('/assignment/')) {
      typeToFind = 'assignment';
      idToFind = +this.route.snapshot.paramMap.get('assignmentId')!;
    }

    const items = this.flatCurriculum();
    if (items.length === 0) return;

    let idx = 0;
    if (typeToFind && idToFind) {
      const foundIdx = items.findIndex(i => i.type === typeToFind && i.id === idToFind);
      if (foundIdx !== -1) idx = foundIdx;
    }

    this.navigateToItem(idx);
  }

  // ─── Navigation ───────────────────────────────────────────────────────────



  openUpdateModal() {
    this.showUpdateModal.set(true);
  }

  closeUpdateModal() {
    this.showUpdateModal.set(false);
  }

  updateVersion() {
    if (!this.courseId() || this.isUpdatingVersion()) return;
    
    this.isUpdatingVersion.set(true);
    this.enrollmentService.updateToLatestVersion(this.courseId()).subscribe({
      next: () => {
        this.isUpdatingVersion.set(false);
        this.closeUpdateModal();
        // Reload course details to get the new version
        this.loadCourseData();
      },
      error: (err) => {
        console.error('Failed to update version:', err);
        this.isUpdatingVersion.set(false);
        this.closeUpdateModal();
        alert('Failed to update to the latest version. Please try again.');
      }
    });
  }

  // ─── Quiz Navigation ───────────────────────────────────────────────────────────

  navigateToItem(index: number) {
    const items = this.flatCurriculum();
    if (index < 0 || index >= items.length) return;

    const item = items[index];
    if (item.isLocked) return;

    this.activeItemIndex.set(index);
    this.router.navigate(['/learner/learn', this.courseId(), item.type, item.id], { replaceUrl: true });

    this.prepareItemContent(item);
  }

  goToNext() {
    this.navigateToItem(this.activeItemIndex() + 1);
  }

  goToPrev() {
    this.navigateToItem(this.activeItemIndex() - 1);
  }

  get nextItemCard() {
    const items = this.flatCurriculum();
    const idx = this.activeItemIndex() + 1;
    return idx < items.length ? items[idx] : null;
  }

  get prevItemCard() {
    const items = this.flatCurriculum();
    const idx = this.activeItemIndex() - 1;
    return idx >= 0 ? items[idx] : null;
  }

  // ─── Content Preparation ──────────────────────────────────────────────────

  private async prepareItemContent(item: FlatItem) {
    // Reset states
    this.absolutePdfUrl.set(null);
    this.sanitizedLinkUrl.set(null);
    this.parsedArticleHtml.set('');
    this.parsedVideoDescriptionHtml.set('');
    this.quizDetail.set(null);
    this.assignmentDetail.set(null);
    this.quizAttemptId.set(null);
    this.quizResult.set(null);
    this.stopQuizTimer();
    
    if (item.type !== 'quiz') {
      await this.quizSignalR.disconnect();
    }

    if (item.type === 'lesson') {
      const lesson = item.item as LessonSummary;

      this.secureVideoUrl.set(null);
      this.absolutePdfUrl.set(null);

      if (lesson.type === LessonType.Video || lesson.type === String(LessonType.Video)) { // Video
        const prog = this.progress()?.sections.flatMap(s => s.lessons).find(l => l.lessonId === lesson.id);
        if (prog) {
          this.videoResumeSecond.set(prog.lastWatchedSecond || 0);
          this.videoMaxWatchedSecond.set(prog.maxWatchedSecond || 0);
        } else {
          this.videoResumeSecond.set(0);
          this.videoMaxWatchedSecond.set(0);
        }

        if (lesson.contentUrl) {
          this.secureMediaService.getSecureUrl(lesson.contentUrl, this.courseId()!).subscribe({
            next: (res) => this.secureVideoUrl.set(res.url),
            error: () => this.toast.showError('Could not load secure video.')
          });
        }

        await this.videoSignalR.connect();
        await this.videoSignalR.getResumePosition(lesson.id);
      } else {
        await this.videoSignalR.disconnect();
      }

      if (lesson.description) {
        this.parsedVideoDescriptionHtml.set(await marked(lesson.description));
      }

      if ((lesson.type === LessonType.Pdf || lesson.type === String(LessonType.Pdf)) && lesson.contentUrl) { // PDF
        this.secureMediaService.getSecureUrl(lesson.contentUrl, this.courseId()!).subscribe({
          next: (res) => {
            const rawUrl = res.url;
            const apiBase = environment.apiUrl.replace('/api/v1', '');
            const absoluteUrl = rawUrl.startsWith('http://') || rawUrl.startsWith('https://')
              ? rawUrl
              : `${apiBase}${rawUrl.startsWith('/') ? '' : '/'}${rawUrl}`;
            this.absolutePdfUrl.set(absoluteUrl);
          },
          error: () => this.toast.showError('Could not load secure PDF.')
        });
      } else if ((lesson.type === LessonType.Article || lesson.type === String(LessonType.Article)) && lesson.content) { // Article
        this.parsedArticleHtml.set(await marked(lesson.content));
      } else if ((lesson.type === LessonType.ExternalLink || lesson.type === String(LessonType.ExternalLink)) && lesson.contentUrl) { // ExternalLink
        this.sanitizedLinkUrl.set(this.sanitizer.bypassSecurityTrustResourceUrl(lesson.contentUrl));
      }
    }
    else if (item.type === 'quiz') {
      await this.loadQuizDetails(item.id);
    }
    else if (item.type === 'assignment') {
      await this.loadAssignmentDetails(item.id);
    }
  }

  onVideoTimeUpdate(event: { currentTime: number, maxTimeWatched: number, duration: number }) {
    const item = this.activeItem();
    if (item?.type === 'lesson') {
      const now = Date.now();
      if (now - this.lastSignalREmit > 5000) {
        this.videoSignalR.updateProgress(item.id, Math.floor(event.currentTime), Math.floor(event.maxTimeWatched), Math.floor(event.duration));
        this.lastSignalREmit = now;
      }
    }
  }

  // ─── Mark Complete ────────────────────────────────────────────────────────

  async markLessonComplete() {
    const item = this.activeItem();
    if (!item || item.type !== 'lesson') return;

    this.isMarkingComplete.set(true);
    try {
      await this.learningService.markLessonComplete(item.id).toPromise();
      await this.fetchLatestProgress();
    } catch (err) {
      console.error('Failed to mark complete', err);
    } finally {
      this.isMarkingComplete.set(false);
    }
  }

  private async fetchLatestProgress() {
    try {
      const wasCompleted = this.progress()?.progressPercentage === 100;
      const progressRes = await this.progressService.getCourseProgress(this.courseId()).toPromise();
      this.progress.set(progressRes!);
      this.buildFlatCurriculum();
      this.checkCourseCompletion(wasCompleted);
    } catch (err) {
      console.error('Failed to fetch latest progress', err);
    }
  }

  private checkCourseCompletion(wasCompletedBefore: boolean) {
    const p = this.progress();
    if (p && p.completedLessonsCount >= p.totalLessonsCount && !wasCompletedBefore) {
      this.confettiTitle.set('🎉 Course Completed!');
      this.confettiMessage.set('Congratulations! You have successfully completed the course.');
      this.showConfetti.set(true);
    }
  }

  // ─── Quizzes ──────────────────────────────────────────────────────────────

  private async loadQuizDetails(quizId: number) {
    try {
      // Do NOT call getQuizForStudent here, because the backend throws if no active attempt exists.
      const rem = await this.learningService.getRemainingAttempts(quizId).toPromise();
      this.remainingAttempts.set(rem!);

      // Populate quizDetail using the active item's metadata so the Start page has details
      const item = this.activeItem()?.item;
      if (item) {
        this.quizDetail.set({
          ...item,
          questions: []
        } as any);
      }

      // Also check if they already passed it
      try {
        const attempts = await this.learningService.getPreviousAttempts(quizId).toPromise();
        const passed = attempts?.find(a => a.isPassed);
        if (passed) {
          this.quizResult.set(passed);
        }
      } catch (e) { }

    } catch (err) {
      console.error('Failed to load quiz details', err);
    }
  }

  async startQuiz() {
    const qId = this.activeItem()?.item?.id;
    if (!qId) return;

    try {
      const startRes = await this.learningService.startQuizAttempt(qId).toPromise();
      this.quizAttemptId.set(startRes!.attemptId);
      this.selectedAnswers.set(new Map());

      // Parse TimeSpan "HH:MM:SS" to seconds
      const parts = startRes!.timeLimit.split(':');
      const seconds = (+parts[0]) * 3600 + (+parts[1]) * 60 + (+parts[2]);
      this.quizTimeLeft.set(seconds);

      // NOW fetch the questions without backend error
      const detail = await this.learningService.getQuizForStudent(qId).toPromise();
      this.quizDetail.set(detail!);

      await this.quizSignalR.connect();
      this.startQuizTimer();
    } catch (err) {
      console.error('Failed to start quiz', err);
    }
  }

  selectQuizOption(questionId: number, optionId: number) {
    const map = new Map(this.selectedAnswers());
    map.set(questionId, optionId);
    this.selectedAnswers.set(map);
    
    const attemptId = this.quizAttemptId();
    if (attemptId) {
      this.quizSignalR.updateAnswer(attemptId, questionId, optionId);
    }
  }

  async submitQuiz() {
    const qId = this.quizDetail()?.id;
    if (!qId) return;

    this.isSubmittingQuiz.set(true);
    this.stopQuizTimer();
    await this.quizSignalR.disconnect();

    const answers = Array.from(this.selectedAnswers().entries()).map(([qId, oId]) => ({
      questionId: qId,
      selectedOptionId: oId
    }));

    try {
      const res = await this.learningService.submitQuiz(qId, { answers }).toPromise();
      this.quizResult.set(res!);

      if (res?.isPassed) {
        await this.fetchLatestProgress();
        this.confettiTitle.set('🎉 Quiz Passed!');
        this.confettiMessage.set(`You scored ${res.obtainedScore} / ${res.totalScore}`);
        this.showConfetti.set(true);
      }
    } catch (err) {
      console.error('Failed to submit quiz', err);
    } finally {
      this.isSubmittingQuiz.set(false);
    }
  }

  private startQuizTimer() {
    this.stopQuizTimer();
    this.quizTimer = setInterval(() => {
      const current = this.quizTimeLeft();
      if (current > 0) {
        this.quizTimeLeft.set(current - 1);
      } else {
        this.submitQuiz(); // auto-submit when time is up
      }
    }, 1000);
  }

  private stopQuizTimer() {
    if (this.quizTimer) clearInterval(this.quizTimer);
  }

  formatTime(seconds: number): string {
    const m = Math.floor(seconds / 60);
    const s = seconds % 60;
    return `${m}:${s < 10 ? '0' : ''}${s}`;
  }

  // ─── Assignments ──────────────────────────────────────────────────────────

  private async loadAssignmentDetails(assignmentId: number) {
    try {
      const context = await this.assignmentService.getLearnerContext(assignmentId).toPromise();
      this.assignmentDetail.set(context.assignment);
      this.assignmentStatus.set(context.status);
      this.mySubmissions.set(context.submissions || []);
    } catch (err) {
      console.error('Failed to load assignment', err);
    }
  }

  onAssignmentFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      this.selectedAssignmentFile = input.files[0];
    }
  }

  async submitAssignment() {
    const a = this.assignmentDetail();
    if (!a) return;

    this.isSubmittingAssignment.set(true);
    try {
      const formData = new FormData();
      formData.append('assignmentId', a.id.toString());
      formData.append('submissionText', this.assignmentForm.value.submissionText || '');

      const type = this.assignmentForm.value.submissionType;
      
      if (type === 'link') {
        formData.append('attachmentType', '1'); // Link
        formData.append('submittedAssignmentUrl', this.assignmentForm.value.attachmentUrl || '');
      } else if (type === 'file') {
        formData.append('attachmentType', '0'); // File
        if (this.selectedAssignmentFile) {
          formData.append('attachmentFile', this.selectedAssignmentFile);
        }
      }

      await this.assignmentService.submitAssignment(formData).toPromise();

      // Reload assignment status
      await this.loadAssignmentDetails(a.id);
      await this.fetchLatestProgress(); // locally mark submitted

      this.assignmentForm.reset({ submissionType: 'none', submissionText: '', attachmentUrl: '' });
      this.selectedAssignmentFile = null;
    } catch (err) {
      console.error('Failed to submit assignment', err);
    } finally {
      this.isSubmittingAssignment.set(false);
    }
  }

  // ─── UI Actions ───────────────────────────────────────────────────────────

  toggleSidebar() {
    this.sidebarOpen.set(!this.sidebarOpen());
  }

  toggleFullscreen() {
    if (!document.fullscreenElement) {
      document.documentElement.requestFullscreen().catch(err => {
        console.error('Error attempting to enable fullscreen:', err.message);
      });
      this.isFullScreen.set(true);
    } else {
      document.exitFullscreen();
      this.isFullScreen.set(false);
    }
  }

  exitCourse() {
    this.router.navigate(['/learner/courses']);
  }

  toggleSection(sectionId: number) {
    const s = new Set(this.expandedSections());
    if (s.has(sectionId)) s.delete(sectionId);
    else s.add(sectionId);
    this.expandedSections.set(s);
  }

  collapseAllSections() {
    this.expandedSections.set(new Set());
  }

  onConfettiClose() {
    this.showConfetti.set(false);
    if (this.course()?.hasCertificate && this.progress()?.progressPercentage === 100) {
      this.router.navigate(['/learner/certificates']);
    }
  }

  get filteredSections() {
    const q = this.searchQuery().toLowerCase();
    const items = this.flatCurriculum();
    if (!q) return this.course()?.sections || [];

    // Simple filter for UI accordion
    return this.course()?.sections.filter(sec =>
      sec.title.toLowerCase().includes(q) ||
      items.filter(i => i.sectionId === sec.id).some(i => i.title.toLowerCase().includes(q))
    ) || [];
  }

  getItemsForSection(sectionId: number) {
    return this.flatCurriculum().filter(i => i.sectionId === sectionId);
  }
}
