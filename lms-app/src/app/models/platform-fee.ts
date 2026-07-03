export enum FeeCategory {
  CourseFee = 0,
  CertificateFee = 1
}

export enum FeeType {
  Percentage = 0,
  Flat = 1
}

export interface SetPlatformFeeRequest {
  category: FeeCategory;
  feeType: FeeType;
  value: number;
}

export interface PlatformFeeResponse {
  id: number;
  category: string;
  feeType: string;
  value: number;
  description: string;
  effectiveFrom: string;
  createdByAdminEmail: string;
  createdAt: string;
  isActive: boolean;
}
