import { Component, OnInit, signal, inject, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { DomSanitizer, SafeUrl } from '@angular/platform-browser';
import { CertificateResponse } from '@models/certificate';
import { CertificateService } from '@services/certificate.service';
import { untilDestroyed } from '../../rxjs/until-destroyed';
import { Loader } from '@components/loader/loader';
import { EmptyState } from '@components/empty-state/empty-state';

@Component({
  selector: 'app-shared-certificate',
  standalone: true,
  imports: [CommonModule, RouterModule, Loader, EmptyState],
  templateUrl: './shared-certificate.html'
})
export class SharedCertificatePage implements OnInit {
  private route = inject(ActivatedRoute);
  private certificateService = inject(CertificateService);
  private sanitizer = inject(DomSanitizer);
  private destroyRef = inject(DestroyRef);

  protected isLoading = signal(true);
  protected certificate = signal<CertificateResponse | null>(null);
  protected error = signal<string | null>(null);
  protected sanitizedPdfUrl = signal<SafeUrl | null>(null);

  ngOnInit(): void {
    this.route.paramMap
      .pipe(untilDestroyed(this.destroyRef))
      .subscribe(params => {
        const token = params.get('token');
        if (token) {
          this.loadSharedCertificate(token);
        } else {
          this.error.set('Invalid link format.');
          this.isLoading.set(false);
        }
      });
  }

  private loadSharedCertificate(token: string): void {
    this.isLoading.set(true);
    this.error.set(null);
    
    this.certificateService.getSharedCertificate(token)
      .pipe(untilDestroyed(this.destroyRef))
      .subscribe({
        next: (cert) => {
          this.certificate.set(cert);
          if (cert.certificateImageUrl) {
            const pdfViewUrl = `${cert.certificateImageUrl}#toolbar=0&navpanes=0&scrollbar=0&view=FitH`;
            this.sanitizedPdfUrl.set(this.sanitizer.bypassSecurityTrustResourceUrl(pdfViewUrl));
          }
          this.isLoading.set(false);
        },
        error: (err) => {
          if (err.status === 404) {
            this.error.set('This certificate link has expired or is invalid.');
          } else {
            this.error.set('Failed to load the certificate. Please try again later.');
          }
          this.isLoading.set(false);
        }
      });
  }

  protected downloadPdf(): void {
    const cert = this.certificate();
    if (cert?.certificateImageUrl) {
      window.open(cert.certificateImageUrl, '_blank');
    }
  }
}
