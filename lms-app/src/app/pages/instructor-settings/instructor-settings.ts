import { Component, inject, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { InstructorOnboardingService } from '@services/instructor-onboarding.service';
import { ToastService } from '@services/toast.service';
import {
  OnboardingStatusResponse,
  CreateLinkedAccountRequest,
  CreateStakeholderRequest,
  ConfigureBankRequest
} from '@models/onboarding';
import { Loader } from '@components/loader/loader';
import { FormInput } from '@components/form-input/form-input';
import { Dropdown } from '@components/dropdown/dropdown';
import { Button } from '@components/button/button';

@Component({
  selector: 'app-instructor-settings',
  standalone: true,
  imports: [CommonModule, FormsModule, Loader, FormInput, Dropdown, Button],
  templateUrl: './instructor-settings.html'
})
export class InstructorSettings implements OnInit {
  private onboardingService = inject(InstructorOnboardingService);
  private toastService = inject(ToastService);

  protected isLoading = signal(true);
  protected isSubmitting = signal(false);
  protected status = signal<OnboardingStatusResponse | null>(null);

  // Form states for steps
  protected accountForm = signal<CreateLinkedAccountRequest>({
    email: '',
    phone: '',
    legalBusinessName: '',
    contactName: '',
    businessType: 'individual',
    profileCategory: 'education',
    profileSubcategory: 'professional_courses',
    street1: '',
    street2: '',
    city: '',
    state: '',
    postalCode: '',
    country: 'IN',
    pan: null,
    gst: null
  });

  protected stakeholderForm = signal<CreateStakeholderRequest>({
    name: '',
    email: ''
  });

  protected bankForm = signal<ConfigureBankRequest>({
    accountNumber: '',
    ifscCode: '',
    beneficiaryName: ''
  });

  // Dropdown options
  protected businessTypeOptions = [{ value: 'individual', label: 'Individual' }];

  // UI state
  protected editMode = signal<'none' | 'account' | 'stakeholder' | 'bank'>('none');

  ngOnInit() {
    this.fetchStatus();
  }

  private fetchStatus() {
    this.isLoading.set(true);
    this.onboardingService.getStatus().subscribe({
      next: (res) => {
        this.status.set(res);
        this.initializeForms(res);
        this.isLoading.set(false);
      },
      error: (err) => {
        this.toastService.showError('Failed to load settings');
        this.isLoading.set(false);
      }
    });
  }

  private initializeForms(status: OnboardingStatusResponse) {
    if (status.account) {
      this.accountForm.set({
        email: status.account.email || '',
        phone: status.account.phone || '',
        legalBusinessName: status.account.legalBusinessName || '',
        contactName: status.account.contactName || '',
        businessType: 'individual',
        profileCategory: 'education',
        profileSubcategory: 'professional_courses',
        street1: '', // Address not returned in summary, user will have to re-enter if updating, or we can just leave it blank if they don't click edit
        street2: '',
        city: '',
        state: '',
        postalCode: '',
        country: 'IN',
        pan: null,
        gst: null
      });
    }

    if (status.stakeholder) {
      this.stakeholderForm.set({
        name: status.stakeholder.name || '',
        email: status.stakeholder.email || ''
      });
    }

    if (status.product) {
      this.bankForm.set({
        accountNumber: '',
        ifscCode: status.product.ifscCode || '',
        beneficiaryName: status.product.beneficiaryName || ''
      });
    }
  }

  protected submitAccount() {
    this.isSubmitting.set(true);
    // Ensure constrained fields
    const payload = { 
      ...this.accountForm(), 
      pan: null, 
      gst: null, 
      businessType: 'individual',
      profileCategory: 'education',
      profileSubcategory: 'professional_courses'
    };

    const isUpdate = !!this.status()?.account;
    const request$ = isUpdate 
      ? this.onboardingService.updateAccount(payload) 
      : this.onboardingService.createAccount(payload);

    request$.subscribe({
      next: () => {
        this.toastService.showSuccess(`Account ${isUpdate ? 'updated' : 'created'} successfully!`);
        this.editMode.set('none');
        this.fetchStatus();
      },
      error: (err) => {
        this.toastService.showError(err.error?.message || 'Failed to save account');
        this.isSubmitting.set(false);
      }
    });
  }

  protected submitStakeholder() {
    this.isSubmitting.set(true);
    const payload = this.stakeholderForm();
    const isUpdate = !!this.status()?.stakeholder;

    const request$ = isUpdate 
      ? this.onboardingService.updateStakeholder(payload) 
      : this.onboardingService.createStakeholder(payload);

    request$.subscribe({
      next: () => {
        this.toastService.showSuccess(`Stakeholder ${isUpdate ? 'updated' : 'created'} successfully!`);
        this.editMode.set('none');
        this.fetchStatus();
      },
      error: (err) => {
        this.toastService.showError(err.error?.message || 'Failed to save stakeholder');
        this.isSubmitting.set(false);
      }
    });
  }

  protected requestProduct() {
    this.isSubmitting.set(true);
    this.onboardingService.requestProduct().subscribe({
      next: () => {
        this.toastService.showSuccess('Payout capabilities requested!');
        this.fetchStatus();
      },
      error: (err) => {
        this.toastService.showError(err.error?.message || 'Failed to request product');
        this.isSubmitting.set(false);
      }
    });
  }

  protected submitBank() {
    this.isSubmitting.set(true);
    this.onboardingService.configureBank(this.bankForm()).subscribe({
      next: () => {
        this.toastService.showSuccess('Bank details saved!');
        this.editMode.set('none');
        this.fetchStatus();
      },
      error: (err) => {
        this.toastService.showError(err.error?.message || 'Failed to save bank details');
        this.isSubmitting.set(false);
      }
    });
  }

  protected updateAccountField(field: keyof CreateLinkedAccountRequest, value: any) {
    this.accountForm.update(prev => ({ ...prev, [field]: value }));
  }

  protected updateStakeholderField(field: keyof CreateStakeholderRequest, value: any) {
    this.stakeholderForm.update(prev => ({ ...prev, [field]: value }));
  }

  protected updateBankField(field: keyof ConfigureBankRequest, value: any) {
    this.bankForm.update(prev => ({ ...prev, [field]: value }));
  }
}
