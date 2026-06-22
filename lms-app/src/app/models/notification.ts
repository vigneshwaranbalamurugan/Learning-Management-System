export interface Notification {
  id: number;
  userId: number;
  title: string;
  message: string;
  type: NotificationType;
  redirectUrl?: string;
  isRead: boolean;
  createdAt: string;
  readAt?: string;
}

export type NotificationType =
  | 'CourseEnrollment'
  | 'AssignmentCreated'
  | 'AssignmentDeadline'
  | 'AssignmentGraded'
  | 'QuizCreated'
  | 'QuizResult'
  | 'CertificateIssued'
  | 'PaymentSuccess'
  | 'PaymentFailed'
  | 'BatchAnnouncement'
  | 'CoursePublished'
  | 'General';

export interface NotificationPage {
  items: Notification[];
  hasMore: boolean;
  page: number;
}
