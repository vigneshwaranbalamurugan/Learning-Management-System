export interface CertificateResponse {
  id: number;
  certificateId: string;
  courseId: number;
  courseName: string;
  userId: number;
  learnerName: string;
  instructorName: string;
  certificateImageUrl: string;
  issuedAt: string;
  courseDescription: string;
  courseThumbnailUrl: string;
  courseLevel: string;
  courseDurationHours: number;
  categoryName: string;
}
