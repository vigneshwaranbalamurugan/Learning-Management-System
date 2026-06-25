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
  | 'PaymentDispute'    // payment.dispute.* webhook events
  | 'PaymentDowntime'  // payment.downtime.* webhook events (admin)
  | 'Settlement'       // settlement.processed webhook event (admin)
  | 'ProductRoute'     // product.route.* webhook events (instructor)
  | 'BatchAnnouncement'
  | 'CoursePublished'
  | 'General';

export interface NotificationPage {
  items: Notification[];
  hasMore: boolean;
  page: number;
}
