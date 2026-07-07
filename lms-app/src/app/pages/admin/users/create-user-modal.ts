import { Component, EventEmitter, Output, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { AdminUserService } from '../../../services/admin-user.service';
import { ToastService } from '../../../services/toast.service';
import { Button } from '../../../components/button/button';
import { Dropdown } from '../../../components/dropdown/dropdown';

@Component({
  selector: 'app-create-user-modal',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, Button, Dropdown],
  template: `
    <div class="fixed inset-0 bg-slate-900/50 backdrop-blur-sm z-50 flex items-center justify-center p-4">
      <div class="bg-white rounded-2xl shadow-xl w-full max-w-md overflow-hidden flex flex-col max-h-[90vh]">
        
        <!-- Header -->
        <div class="px-6 py-4 border-b border-slate-100 flex items-center justify-between">
          <h2 class="text-xl font-bold text-slate-800">Create New User</h2>
          <button (click)="close.emit()" class="text-slate-400 hover:text-slate-600 transition-colors">
            <svg class="w-6 h-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
            </svg>
          </button>
        </div>

        <!-- Form Body -->
        <div class="p-6 overflow-y-auto">
          <form [formGroup]="userForm" (ngSubmit)="onSubmit()" class="space-y-4">

            <div>
              <label class="block text-sm font-medium text-slate-700 mb-1">Email Address</label>
              <input type="email" formControlName="email" placeholder="e.g. jane@example.com" class="block w-full px-3 py-2 border border-slate-200 rounded-lg text-sm placeholder-slate-400 focus:outline-none focus:ring-1 focus:ring-[#1C1C7B] focus:border-[#1C1C7B]">
            </div>

            <div>
              <label class="block text-sm font-medium text-slate-700 mb-1">Password</label>
              <input type="password" formControlName="password" placeholder="Minimum 8 characters" class="block w-full px-3 py-2 border border-slate-200 rounded-lg text-sm placeholder-slate-400 focus:outline-none focus:ring-1 focus:ring-[#1C1C7B] focus:border-[#1C1C7B]">
            </div>

            <div>
              <label class="block text-sm font-medium text-slate-700 mb-1">Role</label>
              <app-dropdown
                [options]="roleOptions"
                [value]="userForm.controls.role.value"
                (valueChange)="userForm.controls.role.setValue($event)"
                placeholder="Select Role"
                class="w-full">
              </app-dropdown>
              <p *ngIf="userForm.controls.role.touched && userForm.controls.role.invalid" class="text-red-500 text-xs mt-1">
                Role is required
              </p>
            </div>

          </form>
        </div>

        <!-- Footer -->
        <div class="px-6 py-4 bg-slate-50 border-t border-slate-100 flex items-center justify-end gap-3">
          <app-button variant="outline" (click)="close.emit()" [disabled]="isSubmitting()">
            Cancel
          </app-button>
          <app-button variant="primary" (click)="onSubmit()" [disabled]="userForm.invalid || isSubmitting()" [loading]="isSubmitting()">
            Create User
          </app-button>
        </div>
      </div>
    </div>
  `
})
export class CreateUserModal {
  @Output() close = new EventEmitter<void>();
  @Output() userCreated = new EventEmitter<void>();

  private fb = inject(FormBuilder);
  private adminUserService = inject(AdminUserService);
  private toast = inject(ToastService);

  isSubmitting = signal(false);

  roleOptions = [
    { value: 'Learner', label: 'Learner' },
    { value: 'Instructor', label: 'Instructor' },
    { value: 'Admin', label: 'Admin' }
  ];

  userForm = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(8)]],
    role: ['Learner', [Validators.required]]
  });

  onSubmit() {
    if (this.userForm.invalid) {
      this.userForm.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);
    
    this.adminUserService.createUser(this.userForm.getRawValue()).subscribe({
      next: () => {
        this.toast.showSuccess('User created successfully');
        this.isSubmitting.set(false);
        this.userCreated.emit();
      },
      error: (err) => {
        this.toast.showError(err.error?.Message || 'Failed to create user');
        this.isSubmitting.set(false);
      }
    });
  }
}
