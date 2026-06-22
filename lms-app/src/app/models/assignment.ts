export interface AssignmentResponse {
  id: number;
  courseSectionId: number;
  title: string;
  description?: string;
  instructions?: string;
  isCompulsory: boolean;
  totalMarks: number;
  passingMarks: number;
  attachmentType: number; // 0 = None, 1 = File, 2 = Link
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
  attachmentType?: number; // 0 = File, 1 = Link
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
