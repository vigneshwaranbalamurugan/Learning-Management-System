import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { InstructorCourseLayout } from '../instructor-course-layout/instructor-course-layout';
import { ToastService } from '@services/toast.service';
import { AuthService } from '@services/auth.service';
import { ConfirmModal } from '@components/confirm-modal/confirm-modal';
import { Button } from '@components/button/button';
import { AssignmentService } from '@services/assignment.service';

@Component({
  selector: 'app-instructor-course-assignments',
  standalone: true,
  imports: [CommonModule, RouterModule, ConfirmModal, Button],
  templateUrl: './instructor-course-assignments.html'
})
export class InstructorCourseAssignments {
  protected layout = inject(InstructorCourseLayout);
  private toastService = inject(ToastService);
  private authService = inject(AuthService);

  protected get routePrefix(): string {
    return this.authService.userRole()?.toLowerCase() || 'instructor';
  }
  private assignmentService = inject(AssignmentService);
  private router = inject(Router);

  protected showDeleteModal = false;
  protected assignmentToDelete: number | null = null;

  protected get course() {
    return this.layout.course();
  }

  protected openAddAssignment() {
    if (this.course?.sections && this.course.sections.length > 0) {
      this.router.navigate([`/${this.routePrefix}/courses`, this.course.slug, 'assignments', 'new']);
    } else {
      this.toastService.showError('Please create a section first before adding an assignment.');
    }
  }

  protected confirmDeleteAssignment(assignmentId: number) {
    this.assignmentToDelete = assignmentId;
    this.showDeleteModal = true;
  }

  protected deleteAssignment() {
    if (!this.course || this.assignmentToDelete === null) return;
    this.assignmentService.deleteAssignment(this.assignmentToDelete).subscribe({
      next: () => {
        this.toastService.showSuccess('Assignment deleted successfully.');
        this.layout.loadCourse(this.course!.id);
        this.closeDeleteModal();
      },
      error: (err) => {
        this.toastService.showApiError(err, 'Failed to delete assignment.');
        this.closeDeleteModal();
      }
    });
  }

  protected closeDeleteModal() {
    this.showDeleteModal = false;
    this.assignmentToDelete = null;
  }

  protected editAssignment(assignmentId: number) {
    if (!this.course) return;
    this.router.navigate([`/${this.routePrefix}/courses`, this.course.slug, 'assignments', assignmentId, 'edit']);
  }
}
