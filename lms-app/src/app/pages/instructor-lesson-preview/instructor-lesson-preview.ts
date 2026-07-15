import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule, Location } from '@angular/common';
import { Router, ActivatedRoute, RouterModule } from '@angular/router';
import { ToastService } from '@services/toast.service';
import { CourseBuilderService } from '@services/course-builder.service';
import { DomSanitizer, SafeHtml, SafeResourceUrl } from '@angular/platform-browser';
import { marked } from 'marked';
import { Button } from '@components/button/button';
import { Loader } from '@components/loader/loader';
import { LessonDiscussions } from '@components/lesson-discussions/lesson-discussions';
import { SecureMediaService } from '@services/secure-media.service';
import { InstructorCourseLayout } from '../instructor-course-layout/instructor-course-layout';

@Component({
  selector: 'app-instructor-lesson-preview',
  standalone: true,
  imports: [CommonModule, RouterModule, Button, Loader, LessonDiscussions],
  templateUrl: './instructor-lesson-preview.html'
})
export class InstructorLessonPreview implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private location = inject(Location);
  private courseBuilderService = inject(CourseBuilderService);
  private toastService = inject(ToastService);
  private sanitizer = inject(DomSanitizer);
  private secureMediaService = inject(SecureMediaService);
  protected layout = inject(InstructorCourseLayout);

  protected courseSlug = '';
  protected lessonId: number | null = null;
  protected lesson: any = null;
  protected isLoading = signal(true);

  // Parsed content
  protected parsedMarkdown: SafeHtml | null = null;
  protected safeMediaUrl: SafeResourceUrl | null = null;
  
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
    this.courseBuilderService.getLesson(id).subscribe({
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
          if (normalized === 'Video' || normalized === 'Pdf') {
            this.secureMediaService.getSecureUrl(this.lesson.contentUrl, this.layout.course()?.id || 0).subscribe({
              next: (res) => {
                this.safeMediaUrl = this.sanitizer.bypassSecurityTrustResourceUrl(res.url);
              },
              error: () => {
                this.toastService.showError('Could not load secure media.');
                this.safeMediaUrl = null;
              }
            });
          } else {
             this.safeMediaUrl = this.sanitizer.bypassSecurityTrustResourceUrl(this.lesson.contentUrl);
          }
        } else {
           this.safeMediaUrl = null;
        }

        this.isLoading.set(false);
      },
      error: (err) => {
        this.toastService.showApiError(err, 'Failed to load lesson details.');
        this.navigateBack();
      }
    });
  }

  private normalizeType(type: any): string {
    if (typeof type === 'string') return type;
    switch(type) {
      case 0: return 'None';
      case 1: return 'Video';
      case 2: return 'Article';
      case 3: return 'Pdf';
      case 4: return 'ExternalLink';
      default: return 'Unknown';
    }
  }

  protected toggleNativeFullscreen(container: HTMLElement) {
    if (!document.fullscreenElement) {
      container.requestFullscreen().catch(err => {
        console.error(`Error attempting to enable full-screen mode: ${err.message} (${err.name})`);
      });
      this.isNativeFullscreen = true;
    } else {
      document.exitFullscreen();
      this.isNativeFullscreen = false;
    }
  }

  navigateBack() {
    this.location.back();
  }
}
