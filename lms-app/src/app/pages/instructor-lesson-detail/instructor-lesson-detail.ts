import { Component, inject, OnInit, OnDestroy, signal, HostListener } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { Router, ActivatedRoute, RouterModule } from '@angular/router';
import { ToastService } from '@services/toast.service';
import { DashboardService } from '@services/dashboard.service';
import { Button } from '@components/button/button';
import { Loader } from '@components/loader/loader';
import { ConfirmModal } from '@components/confirm-modal/confirm-modal';
import { DomSanitizer, SafeHtml, SafeResourceUrl } from '@angular/platform-browser';
import { marked } from 'marked';
import { InstructorCourseLayout } from '../instructor-course-layout/instructor-course-layout';

@Component({
  selector: 'app-instructor-lesson-detail',
  standalone: true,
  imports: [CommonModule, RouterModule, Button, Loader, ConfirmModal, DatePipe],
  templateUrl: './instructor-lesson-detail.html'
})
export class InstructorLessonDetail implements OnInit, OnDestroy {
  private toastService = inject(ToastService);
  private dashboardService = inject(DashboardService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private sanitizer = inject(DomSanitizer);
  protected layout = inject(InstructorCourseLayout);

  protected courseSlug = '';
  protected lessonId: number | null = null;
  protected lesson: any = null;
  protected isLoading = signal(true);

  // Parsed content
  protected parsedMarkdown: SafeHtml | null = null;
  protected safeMediaUrl: SafeResourceUrl | null = null;

  // Navigation
  protected prevLesson: any = null;
  protected nextLesson: any = null;

  // Delete modal state
  protected showDeleteModal = false;

  // Fullscreen / Theater Mode State
  protected isTheaterMode = false;
  protected isNativeFullscreen = false;

  ngOnInit() {
    this.route.parent?.paramMap.subscribe(parentParams => {
      this.courseSlug = parentParams.get('slug') || '';
      this.loadRouteParams();
    });
  }

  private loadRouteParams() {
    this.route.paramMap.subscribe(params => {
      const id = params.get('lessonId');
      if (id) {
        this.lessonId = Number(id);
        this.loadLesson(this.lessonId);
      } else {
        this.toastService.showError('Invalid lesson ID.');
        this.navigateBack();
      }
    });
  }

  private loadLesson(id: number) {
    this.isLoading.set(true);
    this.isTheaterMode = false;
    document.body.style.overflow = '';
    this.dashboardService.getLesson(id).subscribe({
      next: async (data) => {
        this.lesson = data;
        
        // Normalize the lesson type to a standard string
        const normalized = this.normalizeType(this.lesson.type);
        this.lesson.type = normalized;

        // Parse markdown if article
        if (normalized === 'Article' && this.lesson.content) {
          const rawHtml = await marked.parse(this.lesson.content);
          this.parsedMarkdown = this.sanitizer.bypassSecurityTrustHtml(rawHtml);
        } else {
          this.parsedMarkdown = null;
        }

        // Sanitize URL for iframe/video if it exists
        if (this.lesson.contentUrl) {
           this.safeMediaUrl = this.sanitizer.bypassSecurityTrustResourceUrl(this.lesson.contentUrl);
        } else {
           this.safeMediaUrl = null;
        }

        this.computeNavigation();

        this.isLoading.set(false);
      },
      error: (err) => {
        this.toastService.showApiError(err, 'Failed to load lesson details.');
        this.navigateBack();
      }
    });
  }

  private computeNavigation() {
    const course = this.layout.course();
    if (!course || !course.sections) return;

    let allLessons: any[] = [];
    course.sections.forEach((s: any) => {
      if (s.lessons) {
        allLessons = allLessons.concat(s.lessons);
      }
    });

    const currentIndex = allLessons.findIndex(l => l.id === this.lessonId);
    if (currentIndex > 0) {
      this.prevLesson = allLessons[currentIndex - 1];
    } else {
      this.prevLesson = null;
    }

    if (currentIndex >= 0 && currentIndex < allLessons.length - 1) {
      this.nextLesson = allLessons[currentIndex + 1];
    } else {
      this.nextLesson = null;
    }
  }

  protected navigateToLesson(id: number) {
    if (!this.courseSlug) return;
    this.router.navigate(['/instructor/courses', this.courseSlug, 'lessons', id, 'detail']);
  }

  protected previewLesson() {
    this.toastService.showSuccess('Preview functionality coming soon.');
  }

  protected lessonTypeLabel(type: any): string {
    const normalized = this.normalizeType(type);
    if (normalized === 'Video') return 'Video';
    if (normalized === 'Article') return 'Article';
    if (normalized === 'Pdf') return 'PDF';
    if (normalized === 'ExternalLink') return 'External Link';
    return String(type);
  }

  protected normalizeType(type: any): string {
    if (type === 0 || type === '0' || type === 'Video') return 'Video';
    if (type === 1 || type === '1' || type === 'Pdf' || type === 'PDF') return 'Pdf';
    if (type === 2 || type === '2' || type === 'Article') return 'Article';
    if (type === 3 || type === '3' || type === 'ExternalLink') return 'ExternalLink';
    return String(type);
  }



  protected navigateBack() {
    this.router.navigate(['/instructor/courses', this.courseSlug, 'builder']);
  }

  protected navigateToEdit() {
    this.router.navigate(['/instructor/courses', this.courseSlug, 'lessons', this.lessonId, 'edit']);
  }

  protected confirmDelete() {
    this.showDeleteModal = true;
  }

  protected closeDeleteModal() {
    this.showDeleteModal = false;
  }

  protected deleteLesson() {
    if (!this.lessonId) return;
    this.dashboardService.deleteLesson(this.lessonId).subscribe({
      next: () => {
        this.toastService.showSuccess('Lesson deleted successfully.');
        this.navigateBack();
      },
      error: (err) => {
        this.toastService.showApiError(err, 'Failed to delete lesson.');
        this.closeDeleteModal();
      }
    });
  }

  ngOnDestroy() {
    document.body.style.overflow = '';
  }

  protected toggleTheaterMode() {
    this.isTheaterMode = !this.isTheaterMode;
    if (this.isTheaterMode) {
      document.body.style.overflow = 'hidden';
    } else {
      document.body.style.overflow = '';
    }
  }

  protected toggleNativeFullscreen(container: HTMLElement) {
    if (!document.fullscreenElement) {
      container.requestFullscreen().catch((err: any) => {
        this.toastService.showError('Error attempting to enable fullscreen mode: ' + err.message);
      });
    } else {
      document.exitFullscreen();
    }
  }

  @HostListener('document:fullscreenchange')
  @HostListener('document:webkitfullscreenchange')
  @HostListener('document:mozfullscreenchange')
  @HostListener('document:MSFullscreenChange')
  onFullscreenChange() {
    this.isNativeFullscreen = !!document.fullscreenElement;
  }

  @HostListener('document:keydown.escape')
  onEscapeKey() {
    if (this.isTheaterMode) {
      this.isTheaterMode = false;
      document.body.style.overflow = '';
    }
  }
}
