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

export interface CertificateTemplateResponse {
  id: number;
  name: string;
  description?: string;
  templateBackgroundUrl: string;
  aspectRatioWidth: number;
  aspectRatioHeight: number;
  isActive: boolean;
  createdAt: string;
}

export interface CreateCertificateTemplateRequest {
  name: string;
  description?: string;
  aspectRatioWidth: number;
  aspectRatioHeight: number;
}

export interface UpdateCertificateTemplateRequest {
  name?: string;
  description?: string;
  isActive?: boolean;
}

export interface PagedCertificateResponse {
  certificates: CertificateResponse[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export interface PagedCertificateTemplateResponse {
  templates: CertificateTemplateResponse[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}
