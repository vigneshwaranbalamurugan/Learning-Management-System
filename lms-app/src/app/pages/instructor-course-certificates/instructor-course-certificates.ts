import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { InstructorCourseLayout } from '../instructor-course-layout/instructor-course-layout';
import { ToastService } from '@services/toast.service';

@Component({
  selector: 'app-instructor-course-certificates',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './instructor-course-certificates.html'
})
export class InstructorCourseCertificates {
  protected layout = inject(InstructorCourseLayout);
  private toastService = inject(ToastService);

  protected get course() {
    return this.layout.course();
  }

  protected configureCertificate() {
    this.toastService.showInfo('Certificate template editor modal triggered.');
  }

  protected issueCertificate(studentName: string) {
    this.toastService.showSuccess(`Certificate issued successfully for: ${studentName}`);
  }
}
