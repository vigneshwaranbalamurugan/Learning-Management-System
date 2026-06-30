import { Component, OnInit, signal, inject, DestroyRef } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../environments/environment';
import { ToastService } from '../../../services/toast.service';
import { untilDestroyed } from '../../../rxjs/until-destroyed';
import { CertificateTemplateResponse } from '../../../models/certificate';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-admin-certificates',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './certificates.html',
  providers: [DatePipe]
})
export class AdminCertificatesComponent implements OnInit {
  private http = inject(HttpClient);
  private destroyRef = inject(DestroyRef);
  private toast = inject(ToastService);

  protected isLoading = signal(true);
  protected templates = signal<CertificateTemplateResponse[]>([]);
  
  // Modal state
  protected showModal = signal(false);
  protected isSubmitting = signal(false);
  
  // Form data
  protected formData = {
    name: '',
    description: '',
    aspectRatioWidth: 16,
    aspectRatioHeight: 9
  };
  protected selectedFile: File | null = null;
  protected filePreview = signal<string | null>(null);

  ngOnInit() {
    this.loadTemplates();
  }

  private loadTemplates() {
    this.isLoading.set(true);
    this.http.get<CertificateTemplateResponse[]>(`${environment.apiUrl}/certificates/templates`)
      .pipe(untilDestroyed(this.destroyRef))
      .subscribe({
        next: (data) => {
          this.templates.set(data || []);
          this.isLoading.set(false);
        },
        error: (err) => {
          console.error('Failed to load certificate templates', err);
          this.toast.showError('Failed to load templates');
          this.templates.set([]);
          this.isLoading.set(false);
        }
      });
  }

  protected toggleActiveStatus(template: CertificateTemplateResponse) {
    const payload = { isActive: !template.isActive };
    this.http.patch(`${environment.apiUrl}/certificates/templates/${template.id}`, payload)
      .subscribe({
        next: () => {
          this.toast.showSuccess(`Template ${payload.isActive ? 'activated' : 'deactivated'} successfully`);
          this.loadTemplates();
        },
        error: (err) => {
          console.error('Failed to update template status', err);
          this.toast.showError('Failed to update template status');
        }
      });
  }

  protected openCreateModal() {
    this.formData = {
      name: '',
      description: '',
      aspectRatioWidth: 16,
      aspectRatioHeight: 9
    };
    this.selectedFile = null;
    this.filePreview.set(null);
    this.showModal.set(true);
  }

  protected closeModal() {
    this.showModal.set(false);
  }

  protected onFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      const file = input.files[0];
      if (!file.type.startsWith('image/')) {
        this.toast.showError('Please select an image file');
        input.value = '';
        return;
      }
      this.selectedFile = file;

      // Create preview
      const reader = new FileReader();
      reader.onload = (e) => {
        this.filePreview.set(e.target?.result as string);
      };
      reader.readAsDataURL(file);
    }
  }

  protected onSubmit() {
    if (!this.formData.name.trim()) {
      this.toast.showError('Template name is required');
      return;
    }
    if (!this.selectedFile) {
      this.toast.showError('Background image is required');
      return;
    }

    this.isSubmitting.set(true);
    const form = new FormData();
    form.append('name', this.formData.name);
    if (this.formData.description) {
      form.append('description', this.formData.description);
    }
    form.append('aspectRatioWidth', this.formData.aspectRatioWidth.toString());
    form.append('aspectRatioHeight', this.formData.aspectRatioHeight.toString());
    form.append('backgroundImage', this.selectedFile);

    this.http.post(`${environment.apiUrl}/certificates/templates`, form)
      .subscribe({
        next: () => {
          this.toast.showSuccess('Template created successfully');
          this.isSubmitting.set(false);
          this.closeModal();
          this.loadTemplates();
        },
        error: (err) => {
          console.error('Failed to create template', err);
          this.toast.showError(err.error?.message || 'Failed to create template');
          this.isSubmitting.set(false);
        }
      });
  }
}
