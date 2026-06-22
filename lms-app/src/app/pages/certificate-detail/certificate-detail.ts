import { Component, OnInit, signal, computed, inject, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { DomSanitizer } from '@angular/platform-browser';
import { DashboardService } from '@services/dashboard.service';
import { ToastService } from '@services/toast.service';
import { CertificateResponse } from '@models/dashboard';
import { untilDestroyed } from '../../rxjs/until-destroyed';

@Component({
  selector: 'app-certificate-detail-page',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './certificate-detail.html'
})
export class CertificateDetailPage implements OnInit {
  private dashboardService = inject(DashboardService);
  private toastService = inject(ToastService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private destroyRef = inject(DestroyRef);
  private sanitizer = inject(DomSanitizer);

  protected isLoading = signal(true);
  protected certificate = signal<CertificateResponse | null>(null);

  protected safePdfUrl = computed(() => {
    const cert = this.certificate();
    if (cert && cert.certificateImageUrl) {
      const url = cert.certificateImageUrl + '#toolbar=0&navpanes=0&scrollbar=0';
      return this.sanitizer.bypassSecurityTrustResourceUrl(url);
    }
    return null;
  });

  ngOnInit(): void {
    const certId = this.route.snapshot.paramMap.get('id');
    if (certId) {
      this.loadCertificate(certId);
    } else {
      this.goBack();
    }
  }

  private loadCertificate(certId: string): void {
    this.isLoading.set(true);
    // Alternatively, we could get all and filter, but verifyCertificate is perfect
    this.dashboardService.verifyCertificate(certId)
      .pipe(untilDestroyed(this.destroyRef))
      .subscribe({
        next: (data) => {
          this.certificate.set(data);
          this.isLoading.set(false);
        },
        error: (err) => {
          this.toastService.showApiError(err, 'Failed to load certificate details.');
          this.isLoading.set(false);
          this.goBack();
        }
      });
  }

  protected viewCertificatePdf(cert: CertificateResponse): void {
    if (cert.certificateImageUrl) {
      window.open(cert.certificateImageUrl, '_blank');
    } else {
      this.toastService.showError('Certificate file URL is not available.');
    }
  }

  protected goBack(): void {
    this.router.navigate(['/learner/certificates']);
  }
}
