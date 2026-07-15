import { Component, OnInit, signal, computed, inject, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { DomSanitizer } from '@angular/platform-browser';
import { ToastService } from '@services/toast.service';
import { CertificateResponse } from '@models/certificate';
import { untilDestroyed } from '../../rxjs/until-destroyed';

import { Loader } from '@components/loader/loader';
import { PaginationComponent } from '@components/pagination/pagination.component';
import { ConfirmModal } from '@components/confirm-modal/confirm-modal';
import { CertificateService } from '@services/certificate.service';
import { AuthService } from '@services/auth.service';

@Component({
  selector: 'app-certificates-page',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, Loader, PaginationComponent, ConfirmModal],
  templateUrl: './certificates.html'
})
export class CertificatesPage implements OnInit {
  private certificateService = inject(CertificateService);
  private toastService = inject(ToastService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private destroyRef = inject(DestroyRef);
  private sanitizer = inject(DomSanitizer);

  protected certificates = signal<CertificateResponse[]>([]);
  protected isLoading = signal(true);
  protected searchQuery = signal('');

  // Regeneration state
  private authService = inject(AuthService);
  protected canRegenerate = signal(false);
  protected lastRegeneratedAt = signal<string | null>(null);
  protected nextAllowedAt = signal<string | null>(null);
  protected isRegenerating = signal(false);
  protected showRegenerateModal = signal(false);

  protected nameHasChanged = signal(false);
  protected currentPage = signal(1);
  protected pageSize = signal(10);
  protected totalItems = signal(0);
  protected totalPages = signal(0);

  // Sharing state
  protected showShareModal = signal(false);
  protected selectedCertForShare = signal<CertificateResponse | null>(null);
  protected shareMinutes = signal(30);
  protected generatedShareUrl = signal<string | null>(null);
  protected isGeneratingShare = signal(false);

  // Client-side search filtering (only applies to current page now)
  protected filteredCertificates = computed(() => {
    let list = this.certificates();
    const query = this.searchQuery().toLowerCase().trim();

    if (query) {
      list = list.filter(c =>
        c.courseName.toLowerCase().includes(query) ||
        (c.instructorName && c.instructorName.toLowerCase().includes(query))
      );
    }
    return list;
  });

  ngOnInit(): void {
    this.loadCertificates();
    this.loadRegenerationStatus();
  }

  private loadRegenerationStatus(): void {
    this.certificateService.getCertificateRegenerationStatus()
      .pipe(untilDestroyed(this.destroyRef))
      .subscribe({
        next: (status) => {
          this.canRegenerate.set(status.canRegenerate);
          this.lastRegeneratedAt.set(status.lastRegeneratedAt);
          this.nextAllowedAt.set(status.nextAllowedAt);
          this.nameHasChanged.set(status.nameHasChanged);
        },
        error: (err) => {
          console.error('Failed to load regeneration status', err);
          // Fallback for development if backend endpoint is not yet available
          this.canRegenerate.set(true);
        }
      });
  }

  protected loadCertificates(page: number = 1): void {
    this.isLoading.set(true);
    this.certificateService.getMyCertificates(page, this.pageSize())
      .pipe(untilDestroyed(this.destroyRef))
      .subscribe({
        next: (data) => {
          this.certificates.set(data?.certificates ?? []);
          this.currentPage.set(data?.pageNumber ?? 1);
          this.pageSize.set(data?.pageSize ?? 10);
          this.totalItems.set(data?.totalCount ?? 0);
          this.totalPages.set(data?.totalPages ?? 0);
          this.isLoading.set(false);

          // Check if redirect query param exists
          this.route.queryParams
            .pipe(untilDestroyed(this.destroyRef))
            .subscribe(params => {
              const courseIdParam = params['courseId'];
              if (courseIdParam) {
                const found = (data?.certificates ?? []).find(c => c.courseId === Number(courseIdParam));
                if (found) {
                  this.router.navigate(['/learner/certificates', found.certificateId]);
                }
              }
            });
        },
        error: (err) => {
          this.toastService.showApiError(err, 'Failed to load your certificates.');
          this.isLoading.set(false);
        }
      });
  }

  protected onPageChange(page: number): void {
    this.loadCertificates(page);
  }

  protected viewCertificatePdf(cert: CertificateResponse): void {
    if (cert.certificateImageUrl) {
      window.open(cert.certificateImageUrl, '_blank');
    } else {
      this.toastService.showError('Certificate file URL is not available.');
    }
  }


  protected navigateToExplore(): void {
    this.router.navigate(['/learner/explore']);
  }

  protected triggerRegeneration(): void {
    if (!this.canRegenerate() || !this.nameHasChanged()) {
      return;
    }
    this.showRegenerateModal.set(true);
  }

  protected confirmRegeneration(): void {
    this.showRegenerateModal.set(false);
    this.isRegenerating.set(true);
    this.certificateService.regenerateAllCertificates()
      .pipe(untilDestroyed(this.destroyRef))
      .subscribe({
        next: (res) => {
          this.toastService.showSuccess(`Successfully queued ${res.regeneratedCount} certificates for regeneration.`);
          this.loadCertificates(1);
          this.loadRegenerationStatus();
          this.isRegenerating.set(false);
        },
        error: (err) => {
          this.toastService.showApiError(err, 'Failed to regenerate certificates.');
          this.isRegenerating.set(false);
        }
      });
  }

  protected cancelRegeneration(): void {
    this.showRegenerateModal.set(false);
  }

  // Sharing logic
  protected openShareModal(cert: CertificateResponse): void {
    this.selectedCertForShare.set(cert);
    this.shareMinutes.set(30); // reset to default
    this.generatedShareUrl.set(null);
    this.showShareModal.set(true);
  }

  protected closeShareModal(): void {
    this.showShareModal.set(false);
    this.selectedCertForShare.set(null);
    this.generatedShareUrl.set(null);
  }

  protected generateShareLink(): void {
    const cert = this.selectedCertForShare();
    if (!cert) return;

    if (this.shareMinutes() < 5 || this.shareMinutes() > 60) {
      this.toastService.showError('Minutes must be between 5 and 60');
      return;
    }

    this.isGeneratingShare.set(true);
    this.certificateService.shareCertificate(cert.certificateId, { minutes: this.shareMinutes() })
      .pipe(untilDestroyed(this.destroyRef))
      .subscribe({
        next: (res) => {
          this.generatedShareUrl.set(res.shareUrl);
          this.isGeneratingShare.set(false);
        },
        error: (err) => {
          this.toastService.showApiError(err, 'Failed to generate share link');
          this.isGeneratingShare.set(false);
        }
      });
  }

  protected copyShareLink(): void {
    const url = this.generatedShareUrl();
    if (url) {
      navigator.clipboard.writeText(url).then(() => {
        this.toastService.showSuccess('Link copied to clipboard!');
      });
    }
  }
}
