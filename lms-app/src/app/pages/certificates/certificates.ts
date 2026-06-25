import { Component, OnInit, signal, computed, inject, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { DomSanitizer } from '@angular/platform-browser';
import { ToastService } from '@services/toast.service';
import { CertificateResponse } from '@models/certificate';
import { untilDestroyed } from '../../rxjs/until-destroyed';

import { Loader } from '@components/loader/loader';
import { CertificateService } from '@services/certificate.service';

@Component({
  selector: 'app-certificates-page',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, Loader],
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

  // Client-side search filtering
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
  }

  private loadCertificates(): void {
    this.isLoading.set(true);
    this.certificateService.getMyCertificates()
      .pipe(untilDestroyed(this.destroyRef))
      .subscribe({
        next: (data) => {
          this.certificates.set(data ?? []);
          this.isLoading.set(false);

          // Check if redirect query param exists
          this.route.queryParams
            .pipe(untilDestroyed(this.destroyRef))
            .subscribe(params => {
              const courseIdParam = params['courseId'];
              if (courseIdParam) {
                const found = (data ?? []).find(c => c.courseId === Number(courseIdParam));
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
}
