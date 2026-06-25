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
  protected categoryOptions = [{ value: 'education', label: 'Education' }];
  protected subCategoryOptions = [{ value: 'professional_courses', label: 'Professional Courses' }];

  // UI state
  protected editMode = signal<'none' | 'account' | 'stakeholder' | 'bank'>('none');
  protected currentStepIndex = signal(0);
  protected isInitialLoad = true;

  protected isVerified = computed(() => {
    const s = this.status();
    return s?.accountStatus === 'activated' || s?.accountStatus === 'instantly_activated';
  });

  protected steps = [
    { id: 'account', title: 'Basic Details', icon: 'business' },
    { id: 'stakeholder', title: 'Stakeholder', icon: 'person' },
    { id: 'product', title: 'Enable Payouts', icon: 'payments' },
    { id: 'bank', title: 'Bank Account', icon: 'account_balance' }
  ];

  ngOnInit() {
    this.fetchStatus();
  }

  private fetchStatus() {
    this.isLoading.set(true);
    this.onboardingService.getStatus().subscribe({
      next: (res) => {
        this.status.set(res);
        this.initializeForms(res);
        if (this.isInitialLoad) {
          this.setInitialStep(res);
          this.isInitialLoad = false;
        }
        this.isLoading.set(false);
      },
      error: (err) => {
        this.toastService.showError('Failed to load settings');
        this.isLoading.set(false);
      }
    });
  }

  private setInitialStep(status: OnboardingStatusResponse) {
    if (status.currentStep === 'account') this.currentStepIndex.set(0);
    else if (status.currentStep === 'stakeholder') this.currentStepIndex.set(1);
    else if (status.currentStep === 'product') this.currentStepIndex.set(2);
    else if (status.currentStep === 'bank') this.currentStepIndex.set(3);
    else if (status.currentStep === 'completed') this.currentStepIndex.set(3);
  }

  protected nextStep() {
    if (this.currentStepIndex() < this.steps.length - 1) {
      this.currentStepIndex.update(i => i + 1);
      this.editMode.set('none');
    }
  }

  protected prevStep() {
    if (this.currentStepIndex() > 0) {
      this.currentStepIndex.update(i => i - 1);
      this.editMode.set('none');
    }
  }

  protected goToStep(index: number) {
    const statusObj = this.status();
    if (!statusObj) return;

    let highestStep = 0;
    if (statusObj.account) highestStep = 1;
    if (statusObj.stakeholder) highestStep = 2;
    if (statusObj.product) highestStep = 3;
    if (statusObj.currentStep === 'completed') highestStep = 3;

    if (index <= highestStep) {
      this.currentStepIndex.set(index);
      this.editMode.set('none');
    }
  }

  protected getHighestStepAllowed(): number {
    const statusObj = this.status();
    if (!statusObj) return 0;

    let highestStep = 0;
    if (statusObj.account) highestStep = 1;
    if (statusObj.stakeholder) highestStep = 2;
    if (statusObj.product) highestStep = 3;
    if (statusObj.currentStep === 'completed') highestStep = 3;

    return highestStep;
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
        street1: status.account.street1 || '',
        street2: status.account.street2 || '',
        city: status.account.city || '',
        state: status.account.state || '',
        postalCode: status.account.postalCode || '',
        country: status.account.country || 'IN',
        pan: status.account.pan || null,
        gst: status.account.gst || null
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
        accountNumber: status.product.accountNumber || '',
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
        if (!isUpdate) this.nextStep();
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
        if (!isUpdate) this.nextStep();
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
        this.nextStep();
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
