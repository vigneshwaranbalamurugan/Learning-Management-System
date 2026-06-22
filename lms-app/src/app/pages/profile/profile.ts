import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ProfileService, UserProfile } from '@services/profile.service';
import { ToastService } from '@services/toast.service';
import { AuthService } from '@services/auth.service';

@Component({
  selector: 'app-profile-page',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './profile.html'
})
export class Profile implements OnInit {
  private profileService = inject(ProfileService);
  private toastService = inject(ToastService);
  private authService = inject(AuthService);

  protected profile = signal<UserProfile | null>(null);
  protected isSaving = signal(false);
  protected profileImageLimitInMB = signal<number>(5);

  // Edit fields
  protected firstName = '';
  protected lastName = '';
  protected bio = '';
  protected dateOfBirth = '';
  protected location = '';

  protected firstNameError = '';
  protected lastNameError = '';
  protected bioError = '';

  ngOnInit() {
    this.loadProfile();
    this.loadFileLimits();
  }

  private loadFileLimits() {
    this.profileService.getFileLimits().subscribe({
      next: (data: any) => {
        if (data && data.profileImageInMB) {
          this.profileImageLimitInMB.set(data.profileImageInMB);
        }
      }
    });
  }

  private loadProfile() {
    const cached = this.authService.currentUser();
    if (cached) {
      this.setProfileData(cached);
    } else {
      this.profileService.getProfile().subscribe({
        next: (data) => {
          const role = localStorage.getItem('user_role') || 'Learner';
          const email = localStorage.getItem('user_email') || '';
          const updated = { ...data, email, role };
          this.authService.currentUser.set(updated);
          localStorage.setItem('user_profile', JSON.stringify(updated));
          this.setProfileData(updated);
        },
        error: (err) => {
          this.toastService.showApiError(err, 'Failed to load profile details.');
        }
      });
    }
  }

  private setProfileData(data: UserProfile) {
    this.profile.set(data);
    this.firstName = data.firstName || '';
    this.lastName = data.lastName || '';
    this.bio = data.bio || '';
    this.dateOfBirth = data.dateOfBirth ? data.dateOfBirth.split('T')[0] : '';
    this.location = data.location || '';
  }

  protected onSubmit(event: Event) {
    event.preventDefault();
    this.firstNameError = '';
    this.lastNameError = '';
    this.bioError = '';

    let isValid = true;
    if (!this.firstName || this.firstName.length < 2) {
      this.firstNameError = 'First name must be at least 2 characters.';
      isValid = false;
    }
    if (!this.lastName || this.lastName.length < 1) {
      this.lastNameError = 'Last name is required.';
      isValid = false;
    }
    if (!this.bio || this.bio.length < 10) {
      this.bioError = 'Bio must be at least 10 characters.';
      isValid = false;
    }

    if (!isValid) {
      this.toastService.showWarning('Please resolve validation errors.');
      return;
    }

    this.isSaving.set(true);
    const updateData = {
      firstName: this.firstName,
      lastName: this.lastName,
      bio: this.bio,
      dateOfBirth: this.dateOfBirth,
      location: this.location
    };

    this.profileService.updateProfile(updateData).subscribe({
      next: (res) => {
        this.isSaving.set(false);
        this.toastService.showSuccess('Profile updated successfully!');
        
        // Refresh local cache & update auth user signal
        const updated = {
          ...this.profile(),
          ...updateData,
          fullName: `${this.firstName} ${this.lastName}`
        } as UserProfile;
        this.profile.set(updated);
        
        // Propagate changes to AuthState and LocalStorage backup
        const currentAuthUser = this.authService.currentUser();
        if (currentAuthUser) {
          const updatedAuthUser = { ...currentAuthUser, ...updated };
          this.authService.currentUser.set(updatedAuthUser);
          localStorage.setItem('user_profile', JSON.stringify(updatedAuthUser));
        }
      },
      error: (err) => {
        this.isSaving.set(false);
        this.toastService.showApiError(err, 'Failed to update profile.');
      }
    });
  }

  protected isUploadingPicture = signal(false);

  protected onFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files[0]) {
      const file = input.files[0];
      
      if (!file.type.startsWith('image/')) {
        this.toastService.showError('Please select a valid image file.');
        return;
      }
      if (file.size > this.profileImageLimitInMB() * 1024 * 1024) {
        this.toastService.showError(`Image size must be less than ${this.profileImageLimitInMB()}MB.`);
        return;
      }

      this.isUploadingPicture.set(true);
      this.profileService.updateProfileImage(file).subscribe({
        next: (res: UserProfile) => {
          this.isUploadingPicture.set(false);
          this.toastService.showSuccess('Profile picture updated successfully!');
          
          const updatedProfile = { ...this.profile(), ...res };
          this.profile.set(updatedProfile);
          
          // Propagate changes to AuthState and LocalStorage backup
          const currentAuthUser = this.authService.currentUser();
          if (currentAuthUser) {
            const updatedAuthUser = { ...currentAuthUser, ...updatedProfile };
            this.authService.currentUser.set(updatedAuthUser);
            localStorage.setItem('user_profile', JSON.stringify(updatedAuthUser));
          }
        },
        error: (err) => {
          this.isUploadingPicture.set(false);
          this.toastService.showApiError(err, 'Failed to upload profile picture.');
        }
      });
    }
  }

  protected get avatarInitial(): string {
    return this.firstName ? this.firstName.charAt(0).toUpperCase() : 'U';
  }
}
