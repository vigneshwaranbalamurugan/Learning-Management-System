import { Component, OnInit, signal, computed, inject, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { ToastService } from '@services/toast.service';
import { CertificateResponse } from '@models/certificate';
import { untilDestroyed } from '../../rxjs/until-destroyed';
import { CertificateService } from '@services/certificate.service';

@Component({
  selector: 'app-verify-certificate',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './verify-certificate.html'
})
export class VerifyCertificatePage implements OnInit {
  private certificateService = inject(CertificateService);
  private toastService = inject(ToastService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private destroyRef = inject(DestroyRef);
  private sanitizer = inject(DomSanitizer);

  protected inputId = signal('');
  protected certificate = signal<CertificateResponse | null>(null);
  protected isVerifying = signal(false);
  protected hasAttempted = signal(false);
  protected errorMsg = signal<string | null>(null);

  protected safePdfUrl = computed(() => {
    const cert = this.certificate();
    if (cert && cert.certificateImageUrl) {
      const url = cert.certificateImageUrl + '#toolbar=0&navpanes=0&scrollbar=0';
      return this.sanitizer.bypassSecurityTrustResourceUrl(url);
    }
    return null;
  });

  ngOnInit(): void {
    this.route.paramMap
      .pipe(untilDestroyed(this.destroyRef))
      .subscribe(params => {
        const id = params.get('id');
        if (id) {
          this.inputId.set(id);
          this.performVerification(id);
        }
      });
  }

  protected onVerify(): void {
    const guid = this.inputId().trim();
    if (!guid) {
      this.toastService.showError('Please enter a valid certificate ID.');
      return;
    }
    this.performVerification(guid);
  }

  private performVerification(guid: string): void {
    this.isVerifying.set(true);
    this.hasAttempted.set(true);
    this.errorMsg.set(null);
    this.certificate.set(null);

    this.certificateService.verifyCertificate(guid)
      .pipe(untilDestroyed(this.destroyRef))
      .subscribe({
        next: (data) => {
          this.certificate.set(data);
          this.isVerifying.set(false);
        },
        error: (err) => {
          this.isVerifying.set(false);
          this.errorMsg.set(err.error?.message || 'Certificate verification failed. Please verify the ID and try again.');
        }
      });
  }

  protected viewCertificatePdf(): void {
    const cert = this.certificate();
    if (cert && cert.certificateImageUrl) {
      window.open(cert.certificateImageUrl, '_blank');
    } else {
      this.toastService.showError('Certificate file URL is not available.');
    }
  }

  protected clearResult(): void {
    this.certificate.set(null);
    this.hasAttempted.set(false);
    this.errorMsg.set(null);
    this.inputId.set('');
    this.router.navigate(['/verify-certificate']);
  }
}
