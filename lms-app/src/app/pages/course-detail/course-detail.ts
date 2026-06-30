import { Component, OnInit, signal, computed, inject, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { ToastService } from '@services/toast.service';
import { CourseDetailResponse } from '@models/course';
import { EnrollmentResponse } from '@models/enrollment';
import { AuthService } from '@services/auth.service';
import { untilDestroyed } from '../../rxjs/until-destroyed';
import { forkJoin } from 'rxjs';
import { DomSanitizer, SafeResourceUrl, SafeHtml } from '@angular/platform-browser';
import { marked } from 'marked';
import { VideoPlayer } from '../../components/video-player/video-player';
import { CourseService } from '@services/course.service';
import { CourseBuilderService } from '@services/course-builder.service';
import { EnrollmentService } from '@services/enrollment.service';
import { ReviewService } from '@services/review.service';
import { ReviewResponse } from '@models/review';
import { environment } from '@environments/environment';
import { ConfirmModal } from '../../components/confirm-modal/confirm-modal';
import { ConfettiComponent } from '../../components/confetti/confetti';
import { Loader } from '../../components/loader/loader';

@Component({
  selector: 'app-course-detail',
  standalone: true,
  imports: [CommonModule, RouterModule, VideoPlayer, ConfirmModal, ConfettiComponent, Loader],
  templateUrl: './course-detail.html'
})
export class CourseDetail implements OnInit {
  private enrollmentService = inject(EnrollmentService);
  private courseBuilderService = inject(CourseBuilderService);
  private courseService = inject(CourseService);
  private toastService     = inject(ToastService);
  private route            = inject(ActivatedRoute);
  private router           = inject(Router);
  private destroyRef       = inject(DestroyRef);
  private authService      = inject(AuthService);
  private reviewService    = inject(ReviewService);

  // ── Data ─────────────────────────────────────────────────────────────────
  protected course            = signal<CourseDetailResponse | null>(null);
  protected enrollment        = signal<EnrollmentResponse | null>(null);
  protected isEnrolled        = signal(false);
  protected enrollmentProgress = signal(0);
  protected isInstructor      = signal(false);
  protected isAdmin           = signal(false);
  protected reviews           = signal<ReviewResponse[]>([]);

  // ── Preview Modal State ──────────────────────────────────────────────────
  protected previewUrl     = signal<SafeResourceUrl | null>(null);
  protected previewContent = signal<SafeHtml | null>(null);
  protected previewTitle   = signal<string>('');
  protected previewType    = signal<'video' | 'pdf' | 'article' | 'link' | null>(null);
  private sanitizer        = inject(DomSanitizer);

  // ── Video Progress Persistence ──────────────────────────────────────────
  protected currentVideoUrl = signal<string | null>(null);
  protected currentVideoProgress = signal<number>(0);
  private videoProgressMap = new Map<string, number>();

  // ── Computed Markdown ──────────────────────────────────────────────────
  protected descriptionHtml = computed(() => {
    const c = this.course();
    if (!c?.description) return null;
    return this.sanitizer.bypassSecurityTrustHtml(marked.parse(c.description, { async: false }) as string);
  });

  protected learningOutcomesHtml = computed(() => {
    const c = this.course();
    if (!c?.learningOutcomes) return null;
    return this.sanitizer.bypassSecurityTrustHtml(marked.parse(c.learningOutcomes, { async: false }) as string);
  });

  protected requirementsHtml = computed(() => {
    const c = this.course();
    if (!c?.requirements) return null;
    return this.sanitizer.bypassSecurityTrustHtml(marked.parse(c.requirements, { async: false }) as string);
  });

  // ── UI State ─────────────────────────────────────────────────────────────
  protected isLoading   = signal(true);
  protected isEnrolling = signal(false);
  protected expandedSections = signal<Set<number>>(new Set());
  protected showEnrollConfirmModal = signal(false);
  protected showConfetti = signal(false);
  protected isVerifyingPayment = signal(false);
  protected isInitializingPayment = signal(false);

  // ── Stored course ID from router state ────────────────────────────────────
  private courseId: number | null = null;

  ngOnInit(): void {
    if (this.authService.userRole() === 'Instructor') {
      this.isInstructor.set(true);
    } else if (this.authService.userRole() === 'Admin') {
      this.isAdmin.set(true);
    }

    this.route.paramMap.pipe(untilDestroyed(this.destroyRef)).subscribe(params => {
      const slug = params.get('slug');
      if (!slug) {
        this.goBack();
        return;
      }

      // Read from query params first
      const qCourseId = this.route.snapshot.queryParamMap.get('courseId');
      if (qCourseId) {
        this.courseId = Number(qCourseId);
      } else {
        // Fallback to history state (in case of old navigation)
        const historyState = history.state;
        if (historyState?.['courseId']) {
          this.courseId = Number(historyState['courseId']);
        }
      }

      if (this.courseId) {
        this.loadCourse(this.courseId);
      } else {
        // State lost (e.g. direct URL navigation / refresh) — go back with message
        this.toastService.showError('Course not found. Please navigate from the course catalog.');
        this.goBack();
      }
    });
  }

  private loadCourse(courseId: number): void {
    this.isLoading.set(true);

    forkJoin({
      course: this.courseService.getCourseById(courseId),
      enrollments: this.enrollmentService.getMyEnrollments(),
      reviews: this.reviewService.getCourseReviews(courseId)
    }).pipe(untilDestroyed(this.destroyRef)).subscribe({
      next: ({ course, enrollments, reviews }) => {
        this.course.set(course);
        this.reviews.set(reviews);

        // Expand first section by default
        if (course.sections?.length > 0) {
          this.expandedSections.update(s => {
            const next = new Set(s);
            next.add(course.sections[0].id);
            return next;
          });
        }
        
        // Find enrollment for this course
        const found = enrollments.find(e => e.courseId === courseId);
        // Unlocks content for enrolled students, instructors, and admins
        if (found || this.isInstructor() || this.isAdmin()) {
          this.isEnrolled.set(true);
        }

        if (found) {
          this.enrollmentProgress.set(found.progressPercentage ?? 0);
          this.enrollment.set(found);
        }

        this.isLoading.set(false);
      },
      error: (err) => {
        this.toastService.showApiError(err, 'Failed to load course details.');
        this.isLoading.set(false);
        this.goBack();
      }
    });
  }

  protected enroll(): void {
    const c = this.course();
    if (!c) return;
    if (this.isEnrolled()) {
      this.toastService.showInfo('You are already enrolled in this course.');
      return;
    }
    this.showEnrollConfirmModal.set(true);
  }

  protected confirmEnrollment(): void {
    const c = this.course();
    if (!c) return;
    
    this.showEnrollConfirmModal.set(false);

    if (c.isPremium) {
      this.purchasePremiumCourse(c);
      return;
    }

    this.isEnrolling.set(true);
    this.enrollmentService.enrollFreeCourse(c.id).pipe(untilDestroyed(this.destroyRef)).subscribe({
      next: () => {
        this.isEnrolled.set(true);
        this.isEnrolling.set(false);
        this.showConfetti.set(true);
        this.loadCourse(c.id);
      },
      error: (err) => {
        this.toastService.showApiError(err, 'Enrollment failed.');
        this.isEnrolling.set(false);
      }
    });
  }

  protected closeEnrollConfirmModal(): void {
    this.showEnrollConfirmModal.set(false);
  }

  private loadRazorpayScript(): Promise<boolean> {
    return new Promise(resolve => {
      if (document.getElementById('razorpay-checkout-js')) {
        resolve(true);
        return;
      }
      const script = document.createElement('script');
      script.id = 'razorpay-checkout-js';
      script.src = 'https://checkout.razorpay.com/v1/checkout.js';
      script.onload = () => resolve(true);
      script.onerror = () => resolve(false);
      document.body.appendChild(script);
    });
  }

  protected async purchasePremiumCourse(course: CourseDetailResponse) {
    this.isEnrolling.set(true);
    this.isInitializingPayment.set(true);
    
    this.enrollmentService.enrollPremiumCourse(course.id, 'Razorpay')
      .pipe(untilDestroyed(this.destroyRef))
      .subscribe({
        next: async (res) => {
          const loaded = await this.loadRazorpayScript();
          this.isInitializingPayment.set(false);
          if (!loaded) {
            this.toastService.showError('Failed to load payment gateway. Please check your connection.');
            this.isEnrolling.set(false);
            return;
          }

          const options = {
            key: environment.razorpayKey,
            order_id: res.providerOrderId,
            name: 'CourseHub LMS',
            description: `Purchase ${course.title}`,
            handler: (response: any) => {
              this.verifyRazorpayPayment(course.id, response);
            },
            modal: {
              ondismiss: () => {
                this.isEnrolling.set(false);
                this.toastService.showWarning('Payment cancelled.');
              }
            },
            prefill: {
              name: this.authService.currentUser()?.fullName || '',
              email: this.authService.currentUser()?.email || ''
            },
            theme: {
              color: '#1C1C7B'
            }
          };

          const rzp = new (window as any).Razorpay(options);
          rzp.open();
        },
        error: (err) => {
          this.toastService.showApiError(err, 'Failed to initiate payment.');
          this.isEnrolling.set(false);
          this.isInitializingPayment.set(false);
        }
      });
  }

  private verifyRazorpayPayment(courseId: number, paymentData: any) {
    this.isVerifyingPayment.set(true);
    const requestPayload = {
      providerName: 'Razorpay',
      providerOrderId: paymentData.razorpay_order_id,
      providerPaymentId: paymentData.razorpay_payment_id,
      providerSignature: paymentData.razorpay_signature
    };

    this.enrollmentService.verifyPayment(courseId, requestPayload)
      .pipe(untilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.isEnrolled.set(true);
          this.isEnrolling.set(false);
          this.isVerifyingPayment.set(false);
          this.showConfetti.set(true);
          this.loadCourse(courseId);
        },
        error: (err) => {
          this.toastService.showApiError(err, 'Payment verification failed.');
          this.isEnrolling.set(false);
          this.isVerifyingPayment.set(false);
        }
      });
  }

  protected toggleSection(sectionId: number): void {
    this.expandedSections.update(s => {
      const next = new Set(s);
      if (next.has(sectionId)) {
        next.delete(sectionId);
      } else {
        next.add(sectionId);
      }
      return next;
    });
  }

  protected isSectionExpanded(sectionId: number): boolean {
    return this.expandedSections().has(sectionId);
  }

  protected expandAll(): void {
    const c = this.course();
    if (!c) return;
    this.expandedSections.set(new Set(c.sections.map(s => s.id)));
  }

  protected collapseAll(): void {
    this.expandedSections.set(new Set());
  }

  protected goBack(): void {
    if (this.isInstructor()) {
      // If there's history we could use location.back(), but just redirect to instructor dashboard
      this.router.navigate(['/instructor/courses']);
    } else if (this.isAdmin()) {
      this.router.navigate(['/admin/courses']);
    } else {
      this.router.navigate(['/learner/explore']);
    }
  }

  protected navigateToCourses(): void {
    if (this.isAdmin()) {
      this.router.navigate(['/admin/courses']);
    } else {
      this.router.navigate(['/learner/courses']);
    }
  }

  // ── Helpers ───────────────────────────────────────────────────────────────

  protected async openPreview(
    url: string | undefined, 
    content: string | undefined, 
    title: string, 
    typeValue: string | number
  ): Promise<void> {
    const typeStr = this.getLessonTypeName(typeValue).toLowerCase();

    this.previewTitle.set(title);
    
    if (typeStr === 'video') {
      this.currentVideoUrl.set(url || null);
      const savedTime = url ? (this.videoProgressMap.get(url) || 0) : 0;
      this.currentVideoProgress.set(savedTime);
      this.previewUrl.set(url ? this.sanitizer.bypassSecurityTrustResourceUrl(url) : null);
      this.previewContent.set(null);
      this.previewType.set('video');
    } else if (typeStr === 'pdf') {
      this.previewUrl.set(url ? this.sanitizer.bypassSecurityTrustResourceUrl(url) : null);
      this.previewContent.set(null);
      this.previewType.set('pdf');
    } else if (typeStr === 'article') {
      this.previewUrl.set(null);
      if (content) {
        // Parse markdown content and then sanitize
        const { marked } = await import('marked');
        const parsedHtml = await marked.parse(content);
        this.previewContent.set(this.sanitizer.bypassSecurityTrustHtml(parsedHtml));
      } else {
        this.previewContent.set(null);
      }
      this.previewType.set('article');
    } else if (typeStr === 'link') {
      // Use URL or content string depending on where the link is saved
      const targetUrl = url || content || '';
      if (targetUrl) {
        window.open(targetUrl, '_blank', 'noopener,noreferrer');
      }
    }
  }

  protected closePreview(): void {
    this.previewUrl.set(null);
    this.previewContent.set(null);
    this.previewTitle.set('');
    this.previewType.set('video');
    this.currentVideoUrl.set(null);
    this.currentVideoProgress.set(0);
  }

  protected onTimeWatchedUpdate(event: { currentTime: number, maxTimeWatched: number }): void {
    const url = this.currentVideoUrl();
    if (url) {
      this.videoProgressMap.set(url, event.maxTimeWatched);
      this.currentVideoProgress.set(event.maxTimeWatched);
    }
  }

  protected getLevelName(level: number | string): string {
    switch (Number(level)) {
      case 1: return 'Beginner';
      case 2: return 'Intermediate';
      case 3: return 'Advanced';
      default: return 'All Levels';
    }
  }

  protected getLevelColor(level: number | string): string {
    switch (Number(level)) {
      case 1: return 'emerald';
      case 2: return 'amber';
      case 3: return 'red';
      default: return 'gray';
    }
  }

  protected formatDuration(isoStr: string | null | undefined): string {
    if (!isoStr) return '';
    const parts = isoStr.split(':');
    const h = parseInt(parts[0] ?? '0', 10);
    const m = parseInt(parts[1] ?? '0', 10);
    if (h > 0 && m > 0) return `${h}h ${m}m`;
    if (h > 0) return `${h}h`;
    if (m > 0) return `${m}m`;
    const s = parseInt(parts[2] ?? '0', 10);
    return s > 0 ? `${s}s` : '';
  }

  protected getLessonTypeName(type: number | string): string {
    switch (String(type).toLowerCase()) {
      case '0': case 'video':        return 'Video';
      case '1': case 'pdf':          return 'PDF';
      case '2': case 'article':      return 'Article';
      case '3': case 'externallink': return 'Link';
      case '4': case 'quiz':         return 'Quiz';
      default:                       return 'Lesson';
    }
  }

  protected getTotalLessonsCount(): number {
    return this.course()?.sections?.reduce((acc, s) => acc + (s.lessons?.length ?? 0), 0) ?? 0;
  }

  protected parseOutcomes(raw: string | null | undefined): string[] {
    if (!raw) return [];
    return raw.split(/\n|;/).map(l => l.trim()).filter(l => l.length > 0);
  }

  protected parseRequirements(raw: string | null | undefined): string[] {
    if (!raw) return [];
    return raw.split(/\n|;/).map(l => l.trim()).filter(l => l.length > 0);
  }

  protected maskEmail(email: string | undefined): string {
    if (!email) return '';
    const parts = email.split('@');
    if (parts.length !== 2) return email;
    const name = parts[0];
    const domain = parts[1];
    
    if (name.length <= 3) {
      return name[0] + '***@' + domain;
    } else if (name.length <= 5) {
      return name.substring(0, 2) + '***' + name.substring(name.length - 1) + '@' + domain;
    } else {
      return name.substring(0, 3) + '***' + name.substring(name.length - 2) + '@' + domain;
    }
  }

  protected starsArray(rating: number): boolean[] {
    return [1, 2, 3, 4, 5].map(s => s <= Math.round(rating));
  }
}
