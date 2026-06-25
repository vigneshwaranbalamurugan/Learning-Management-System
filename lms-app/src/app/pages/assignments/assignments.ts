import { Component, OnInit, signal, computed, inject, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, Router } from '@angular/router';
import { ToastService } from '@services/toast.service';
import { untilDestroyed } from '../../rxjs/until-destroyed';
import { Loader } from '@components/loader/loader';
import { forkJoin, of } from 'rxjs';
import { catchError, map, switchMap } from 'rxjs/operators';
import { AssignmentResponse, AssignmentStatusResponse } from '@models/assignment';
import { CourseService } from '@services/course.service';
import { EnrollmentService } from '@services/enrollment.service';
import { AssignmentService } from '@services/assignment.service';

interface EnrichedAssignment {
  id: number;
  title: string;
  courseTitle: string;
  sectionTitle: string;
  isCompulsory: boolean;
  totalMarks: number;
  passingMarks: number;
  deadlineInDays: number;
  deadlineDate?: string;
  maxSubmissions: number;
  status: string; // e.g. "Pending", "Submitted", "UnderReview", "Graded", "Passed", "Failed"
  isPassed: boolean | null;
  attemptsMade: number;
  remainingAttempts: number;
  deadline?: string;
}

@Component({
  selector: 'app-assignments-page',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, Loader],
  templateUrl: './assignments.html'
})
export class AssignmentsPage implements OnInit {
  private assignmentService = inject(AssignmentService);
  private enrollmentService = inject(EnrollmentService);
  private courseService = inject(CourseService);
  private toastService = inject(ToastService);
  private router = inject(Router);
  private destroyRef = inject(DestroyRef);

  protected assignments = signal<EnrichedAssignment[]>([]);
  protected isLoading = signal(true);
  protected searchQuery = signal('');

  // Statistics
  protected totalCount = computed(() => this.assignments().length);
  protected pendingCount = computed(() => this.assignments().filter(a => a.status === 'Pending' || a.status === 'Submitted' || a.status === 'UnderReview').length);
  protected passedCount = computed(() => this.assignments().filter(a => a.isPassed === true).length);
  protected failedCount = computed(() => this.assignments().filter(a => a.isPassed === false).length);

  // Client-side search filtering
  protected filteredAssignments = computed(() => {
    let list = this.assignments();
    const query = this.searchQuery().toLowerCase().trim();

    if (query) {
      list = list.filter(a => 
        (a.title && a.title.toLowerCase().includes(query)) ||
        (a.courseTitle && a.courseTitle.toLowerCase().includes(query)) ||
        (a.sectionTitle && a.sectionTitle.toLowerCase().includes(query))
      );
    }
    return list;
  });

  ngOnInit(): void {
    this.loadAssignments();
  }

  private loadAssignments(): void {
    this.isLoading.set(true);

    this.enrollmentService.getMyEnrollments()
      .pipe(
        untilDestroyed(this.destroyRef),
        switchMap(enrollments => {
          if (!enrollments || enrollments.length === 0) {
            return of([]);
          }

          // Fetch full details (sections) for all enrolled courses
          const courseDetailObs = enrollments.map(e => 
            this.courseService.getCourseById(e.courseId).pipe(
              catchError(() => of(null))
            )
          );

          return forkJoin(courseDetailObs).pipe(
            switchMap(courseDetails => {
              const validDetails = courseDetails.filter(c => c !== null);
              if (validDetails.length === 0) {
                return of([]);
              }

              // Collect all sections
              const sectionsList: { sectionId: number; courseTitle: string; sectionTitle: string }[] = [];
              for (const course of validDetails) {
                if (course.sections) {
                  for (const sec of course.sections) {
                    sectionsList.push({
                      sectionId: sec.id,
                      courseTitle: course.title,
                      sectionTitle: sec.title
                    });
                  }
                }
              }

              if (sectionsList.length === 0) {
                return of([]);
              }

              // Fetch assignments for each section
              const assignmentsObs = sectionsList.map(sec => 
                this.assignmentService.getAssignmentsBySection(sec.sectionId).pipe(
                  map(assignments => (assignments || []).map(a => ({
                    ...a,
                    courseTitle: sec.courseTitle,
                    sectionTitle: sec.sectionTitle
                  }))),
                  catchError(() => of([]))
                )
              );

              return forkJoin(assignmentsObs).pipe(
                map(results => results.flat()),
                switchMap(allAssignments => {
                  if (allAssignments.length === 0) {
                    return of([]);
                  }

                  // Fetch status for each assignment
                  const statusObs = allAssignments.map(a => 
                    this.assignmentService.getAssignmentStatus(a.id).pipe(
                      map(status => ({
                        ...a,
                        statusInfo: status
                      })),
                      catchError(() => of({
                        ...a,
                        statusInfo: null
                      }))
                    )
                  );

                  return forkJoin(statusObs);
                })
              );
            })
          );
        })
      )
      .subscribe({
        next: (data: any[]) => {
          const enriched: EnrichedAssignment[] = data.map(item => {
            const statusInfo: AssignmentStatusResponse | null = item.statusInfo;
            return {
              id: item.id,
              title: item.title,
              courseTitle: item.courseTitle,
              sectionTitle: item.sectionTitle,
              isCompulsory: item.isCompulsory,
              totalMarks: item.totalMarks,
              passingMarks: item.passingMarks,
              deadlineInDays: item.deadlineInDays,
              deadlineDate: item.deadlineDate,
              maxSubmissions: item.maxSubmissions,
              status: statusInfo?.latestStatus || 'Pending',
              isPassed: statusInfo?.isPassed !== undefined ? statusInfo.isPassed : null,
              attemptsMade: statusInfo?.attemptsMade || 0,
              remainingAttempts: statusInfo?.remainingAttempts !== undefined ? statusInfo.remainingAttempts : item.maxSubmissions,
              deadline: statusInfo?.deadline
            };
          });

          this.assignments.set(enriched);
          this.isLoading.set(false);
        },
        error: (err) => {
          this.toastService.showApiError(err, 'Failed to load assignments.');
          this.isLoading.set(false);
        }
      });
  }

  protected viewAssignmentDetail(assignmentId: number): void {
    this.router.navigate(['/learner/assignments', assignmentId]);
  }

  protected navigateToExplore(): void {
    this.router.navigate(['/learner/explore']);
  }
}
