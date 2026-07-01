import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { SettingsService } from '@services/settings.service';
import { ToastService } from '@services/toast.service';
import { FeeCategory, FeeType, SetPlatformFeeRequest, PlatformFeeResponse } from '@models/platform-fee';

@Component({
  selector: 'app-admin-settings',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './settings.html'
})
export class AdminSettings implements OnInit {
  private settingsService = inject(SettingsService);
  private toastService = inject(ToastService);

  protected isLoading = signal<boolean>(true);
  protected isSaving = signal<boolean>(false);
  protected currentFee = signal<PlatformFeeResponse | null>(null);

  protected feePercentage: number = 0;

  ngOnInit() {
    this.loadCurrentFee();
  }

  private loadCurrentFee() {
    this.isLoading.set(true);
    this.settingsService.getPlatformFee(FeeCategory.CourseFee).subscribe({
      next: (res) => {
        // If there's a response with a value, use it. Sometimes message is returned if not set.
        if (res && res.id) {
          this.currentFee.set(res);
          this.feePercentage = res.value;
        } else {
          // Default if no fee configured
          this.feePercentage = 10;
        }
        this.isLoading.set(false);
      },
      error: (err) => {
        this.toastService.showApiError(err, 'Failed to load platform fee settings.');
        this.isLoading.set(false);
      }
    });
  }

  protected saveFee() {
    if (this.feePercentage < 0 || this.feePercentage > 100) {
      this.toastService.showError('Fee percentage must be between 0 and 100.');
      return;
    }

    this.isSaving.set(true);
    const request: SetPlatformFeeRequest = {
      category: FeeCategory.CourseFee,
      feeType: FeeType.Percentage,
      value: this.feePercentage
    };

    this.settingsService.setPlatformFee(request).subscribe({
      next: (res) => {
        this.currentFee.set(res);
        this.toastService.showSuccess('Platform fee updated successfully!');
        this.isSaving.set(false);
      },
      error: (err) => {
        this.toastService.showApiError(err, 'Failed to update platform fee.');
        this.isSaving.set(false);
      }
    });
  }
}
