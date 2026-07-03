import { Component, OnInit, signal, inject, DestroyRef } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { ActivatedRoute, Router } from '@angular/router';
import { forkJoin } from 'rxjs';

import { environment } from '@environments/environment';
import { SettingsService } from '@services/settings.service';
import { ToastService } from '@services/toast.service';
import { untilDestroyed } from '../../../rxjs/until-destroyed';

import { FeeCategory, FeeType, SetPlatformFeeRequest, PlatformFeeResponse } from '@models/platform-fee';
import { CertificateTemplateResponse } from '@models/certificate';

type Tab = 'fees' | 'certificates';

@Component({
  selector: 'app-admin-settings',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './settings.html',
  providers: [DatePipe]
})
export class AdminSettings implements OnInit {
  private settingsService = inject(SettingsService);
  private toastService = inject(ToastService);
  private http = inject(HttpClient);
  private destroyRef = inject(DestroyRef);
  private route = inject(ActivatedRoute);
  private router = inject(Router);

  // --- Tab State ---
  protected activeTab = signal<Tab>('fees');

  // --- Platform Fees State ---
  protected feesLoading = signal<boolean>(true);
  protected platformFees = signal<PlatformFeeResponse[]>([]);
  
  // Use getters to make enums available to template
  public get FeeCategory() { return FeeCategory; }
  public get FeeType() { return FeeType; }

  protected feeCategories = [
    { id: FeeCategory.CourseFee, label: 'Course Fee', description: 'Percentage or flat fee deducted from course sales.' },
    { id: FeeCategory.CertificateFee, label: 'Certificate Fee', description: 'Fee charged for issuing premium certificates.' }
  ];

  // Fee Form Modal
  protected showFeeModal = signal<boolean>(false);
  protected isSavingFee = signal<boolean>(false);
  protected isEditingFee = signal<boolean>(false);
  
  protected feeForm = {
    category: FeeCategory.CourseFee,
    feeType: FeeType.Percentage,
    value: 0
  };

  // Fee History Modal
  protected showHistoryModal = signal<boolean>(false);
  protected historyLoading = signal<boolean>(false);
  protected feeHistory = signal<PlatformFeeResponse[]>([]);
  protected historyCategoryLabel = signal<string>('');

  // --- Certificates State ---
  protected certsLoading = signal<boolean>(true);
  protected templates = signal<CertificateTemplateResponse[]>([]);
  
  // Certificate Modal
  protected showCertModal = signal<boolean>(false);
  protected isSubmittingCert = signal<boolean>(false);
  protected isEditingCert = signal<boolean>(false);
  protected editingCertId = signal<number | null>(null);
  
  protected certForm = {
    name: '',
    description: '',
    aspectRatioWidth: 16,
    aspectRatioHeight: 9
  };
  protected selectedFile: File | null = null;
  protected filePreview = signal<string | null>(null);

  ngOnInit() {
    this.route.queryParams.pipe(untilDestroyed(this.destroyRef)).subscribe(params => {
      if (params['tab'] === 'certificates') {
        this.activeTab.set('certificates');
        if (this.templates().length === 0) {
          this.loadTemplates();
        }
      } else {
        this.activeTab.set('fees');
        if (this.platformFees().length === 0) {
          this.loadFees();
        }
      }
    });
  }

  protected switchTab(tab: Tab) {
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { tab: tab === 'fees' ? null : tab },
      queryParamsHandling: 'merge'
    });
  }

  // ==========================================
  // PLATFORM FEES LOGIC
  // ==========================================
  private loadFees() {
    this.feesLoading.set(true);
    
    forkJoin({
      course: this.settingsService.getPlatformFee(FeeCategory.CourseFee),
      cert: this.settingsService.getPlatformFee(FeeCategory.CertificateFee)
    }).subscribe({
      next: (res) => {
        const fees: PlatformFeeResponse[] = [];
        if (res.course && res.course.id) fees.push(res.course);
        if (res.cert && res.cert.id) fees.push(res.cert);
        
        this.platformFees.set(fees);
        this.feesLoading.set(false);
      },
      error: (err) => {
        this.toastService.showApiError(err, 'Failed to load platform fees.');
        this.feesLoading.set(false);
      }
    });
  }

  protected getFeeForCategory(category: FeeCategory): PlatformFeeResponse | undefined {
    // The backend returns Category as string ("CourseFee", "CertificateFee")
    const categoryStr = category === FeeCategory.CourseFee ? 'CourseFee' : 'CertificateFee';
    return this.platformFees().find(f => f.category === categoryStr && f.isActive);
  }

  protected openAddFeeModal(category: FeeCategory) {
    this.feeForm = {
      category: category,
      feeType: FeeType.Percentage,
      value: 10
    };
    this.isEditingFee.set(false);
    this.showFeeModal.set(true);
  }

  protected openEditFeeModal(category: FeeCategory, fee: PlatformFeeResponse) {
    this.feeForm = {
      category: category,
      feeType: fee.feeType.toLowerCase() === 'percentage' ? FeeType.Percentage : FeeType.Flat,
      value: fee.value
    };
    this.isEditingFee.set(true);
    this.showFeeModal.set(true);
  }

  protected closeFeeModal() {
    this.showFeeModal.set(false);
  }

  protected saveFee() {
    // Typecast to ensure proper check (sometimes Angular forms convert to string)
    const feeType = Number(this.feeForm.feeType);
    if (feeType === FeeType.Percentage && (this.feeForm.value < 0 || this.feeForm.value > 100)) {
      this.toastService.showError('Percentage must be between 0 and 100.');
      return;
    }
    if (this.feeForm.value < 0) {
      this.toastService.showError('Fee value cannot be negative.');
      return;
    }

    this.isSavingFee.set(true);
    const request: SetPlatformFeeRequest = { 
        category: this.feeForm.category,
        feeType: feeType,
        value: this.feeForm.value
    };
    
    const obs$ = this.isEditingFee() 
      ? this.settingsService.updatePlatformFee(request)
      : this.settingsService.setPlatformFee(request);

    obs$.subscribe({
      next: () => {
        this.toastService.showSuccess(`Platform fee ${this.isEditingFee() ? 'updated' : 'created'} successfully!`);
        this.isSavingFee.set(false);
        this.closeFeeModal();
        this.loadFees();
      },
      error: (err) => {
        this.toastService.showApiError(err, 'Failed to save platform fee.');
        this.isSavingFee.set(false);
      }
    });
  }

  protected deleteFee(category: FeeCategory) {
    if (confirm('Are you sure you want to remove this active fee configuration? The system will fall back to defaults.')) {
      this.settingsService.deletePlatformFee(category).subscribe({
        next: () => {
          this.toastService.showSuccess('Platform fee configuration removed.');
          this.loadFees();
        },
        error: (err) => {
          this.toastService.showApiError(err, 'Failed to remove platform fee.');
        }
      });
    }
  }

  protected openHistoryModal(category: FeeCategory) {
    this.historyCategoryLabel.set(category === FeeCategory.CourseFee ? 'Course Fee' : 'Certificate Fee');
    this.showHistoryModal.set(true);
    this.historyLoading.set(true);
    this.feeHistory.set([]);
    
    this.settingsService.getFeeHistory(category).subscribe({
      next: (data) => {
        this.feeHistory.set(data || []);
        this.historyLoading.set(false);
      },
      error: (err) => {
        this.toastService.showApiError(err, 'Failed to load fee history.');
        this.historyLoading.set(false);
      }
    });
  }
  
  protected closeHistoryModal() {
    this.showHistoryModal.set(false);
  }

  // ==========================================
  // CERTIFICATES LOGIC
  // ==========================================
  private loadTemplates() {
    this.certsLoading.set(true);
    this.http.get<CertificateTemplateResponse[]>(`${environment.apiUrl}/certificates/templates`)
      .pipe(untilDestroyed(this.destroyRef))
      .subscribe({
        next: (data) => {
          this.templates.set(data || []);
          this.certsLoading.set(false);
        },
        error: (err) => {
          this.toastService.showApiError(err, 'Failed to load certificate templates');
          this.templates.set([]);
          this.certsLoading.set(false);
        }
      });
  }

  protected toggleTemplateStatus(template: CertificateTemplateResponse) {
    const payload = { isActive: !template.isActive };
    this.http.patch(`${environment.apiUrl}/certificates/templates/${template.id}`, payload)
      .subscribe({
        next: () => {
          this.toastService.showSuccess(`Template ${payload.isActive ? 'activated' : 'deactivated'} successfully`);
          this.loadTemplates();
        },
        error: (err) => {
          this.toastService.showApiError(err, 'Failed to update template status');
        }
      });
  }

  protected openCertModal() {
    this.certForm = {
      name: '',
      description: '',
      aspectRatioWidth: 16,
      aspectRatioHeight: 9
    };
    this.selectedFile = null;
    this.filePreview.set(null);
    this.isEditingCert.set(false);
    this.editingCertId.set(null);
    this.showCertModal.set(true);
  }

  protected openEditCertModal(template: CertificateTemplateResponse) {
    this.certForm = {
      name: template.name,
      description: template.description || '',
      aspectRatioWidth: template.aspectRatioWidth,
      aspectRatioHeight: template.aspectRatioHeight
    };
    this.selectedFile = null;
    this.filePreview.set(template.templateBackgroundUrl);
    this.isEditingCert.set(true);
    this.editingCertId.set(template.id);
    this.showCertModal.set(true);
  }

  protected closeCertModal() {
    this.showCertModal.set(false);
  }

  protected onCertFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      const file = input.files[0];
      if (!file.type.startsWith('image/')) {
        this.toastService.showError('Please select an image file');
        input.value = '';
        return;
      }
      this.selectedFile = file;

      const reader = new FileReader();
      reader.onload = (e) => {
        this.filePreview.set(e.target?.result as string);
      };
      reader.readAsDataURL(file);
    }
  }

  protected saveCertificate() {
    if (!this.certForm.name.trim()) {
      this.toastService.showError('Template name is required');
      return;
    }
    
    if (!this.isEditingCert() && !this.selectedFile) {
      this.toastService.showError('Background image is required');
      return;
    }

    this.isSubmittingCert.set(true);

    if (this.isEditingCert()) {
      // For editing, we only update name and description. Background image and aspect ratio are not editable.
      const payload = {
        name: this.certForm.name,
        description: this.certForm.description
      };
      
      this.http.patch(`${environment.apiUrl}/certificates/templates/${this.editingCertId()}`, payload)
        .subscribe({
          next: () => {
            this.toastService.showSuccess('Template updated successfully');
            this.isSubmittingCert.set(false);
            this.closeCertModal();
            this.loadTemplates();
          },
          error: (err) => {
            this.toastService.showApiError(err, 'Failed to update template');
            this.isSubmittingCert.set(false);
          }
        });
    } else {
      const form = new FormData();
      form.append('name', this.certForm.name);
      if (this.certForm.description) {
        form.append('description', this.certForm.description);
      }
      form.append('aspectRatioWidth', this.certForm.aspectRatioWidth.toString());
      form.append('aspectRatioHeight', this.certForm.aspectRatioHeight.toString());
      form.append('backgroundImage', this.selectedFile!);

      this.http.post(`${environment.apiUrl}/certificates/templates`, form)
        .subscribe({
          next: () => {
            this.toastService.showSuccess('Template created successfully');
            this.isSubmittingCert.set(false);
            this.closeCertModal();
            this.loadTemplates();
          },
          error: (err) => {
            this.toastService.showApiError(err, 'Failed to create template');
            this.isSubmittingCert.set(false);
          }
        });
    }
  }
}
