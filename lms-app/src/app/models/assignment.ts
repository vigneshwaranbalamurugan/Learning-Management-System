import { AssignmentAttachmentType } from '../enums/assignment-attachment-type.enum';

export interface AssignmentResponse {
  id: number;
  courseSectionId: number;
  title: string;
  description?: string;
  instructions?: string;
  isCompulsory: boolean;
  totalMarks: number;
  passingMarks: number;
  attachmentType: AssignmentAttachmentType | number; // AssignmentAttachmentType
  attachmentUrl?: string;
  deadlineInDays: number;
  deadlineDate?: string;
  maxSubmissions: number;
  isLateSubmissionAllowed: boolean;
  status: number;
  createdAt: string;
}

export interface AssignmentSubmissionResponse {
  id: number;
  assignmentId: number;
  studentId: number;
  submissionText?: string;
  attachmentType?: AssignmentAttachmentType | number; // AssignmentAttachmentType
  submittedAssignmentUrl?: string;
  marksAwarded?: number;
  feedback?: string;
  submittedAt: string;
  gradedAt?: string;
  status: string; // "Submitted", "UnderReview", "Graded", etc.
  isPassed?: boolean;
  attemptNumber: number;
  studentName?: string;
  studentEmail?: string;
  isLate?: boolean;
  studentDeadline?: string;
  courseTitle?: string;
  sectionTitle?: string;
  assignmentTitle?: string;
}

export interface AssignmentStatusResponse {
  assignmentId: number;
  studentId: number;
  attemptsMade: number;
  maxSubmissions: number;
  remainingAttempts: number;
  isPassed?: boolean;
  latestStatus?: string;
  deadline?: string;
}

export interface InstructorAssignmentSummaryDto {
  id: number;
  courseSectionId: number;
  title: string;
  courseTitle: string;
  sectionTitle: string;
  totalMarks: number;
  deadlineInDays: number;
  deadlineDate?: string;
  pendingSubmissionsCount: number;
  status: number;
  createdAt: string;
}

export interface PagedInstructorAssignmentResponse {
  assignments: InstructorAssignmentSummaryDto[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  totalPendingCount: number;
  fullyGradedCount: number;
  uniqueCourseCount: number;
}
