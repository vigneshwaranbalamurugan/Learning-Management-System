export interface AdminRevenueSummary {
  totalRevenue: number;
  totalPlatformFees: number;
  totalInstructorPayouts: number;
  totalPendingPayouts: number;
  totalTransactions: number;
  byInstructor: AdminInstructorSummary[];
  pendingManualReviews: AdminPayoutItem[];
}

export interface AdminTransactionItem {
  id: number;
  learnerName: string;
  learnerEmail: string;
  courseName: string;
  instructorName: string;
  grossAmount: number;
  platformFeeAmount: number;
  instructorAmount: number;
  currency: string;
  status: string;
  disputeStatus: string | null;
  paidAt: string | null;
  createdAt: string;
  providerPaymentId: string | null;
}

export interface PagedAdminTransactionResponse {
  items: AdminTransactionItem[];
  totalRevenue: number;
  totalPlatformFees: number;
  totalInstructorShare: number;
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export interface AdminPayoutItem {
  id: number;
  instructorName: string;
  instructorEmail: string;
  courseName: string;
  learnerName: string | null;
  amount: number;
  status: string;
  razorpayTransferId: string | null;
  failureReason: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface PagedAdminPayoutResponse {
  items: AdminPayoutItem[];
  totalPaidOut: number;
  totalPending: number;
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export interface RevenueFilters {
  search: string;
  status: string;
  dateFrom: string;
  dateTo: string;
  page: number;
  pageSize: number;
}

export interface AdminInstructorSummary {
  instructorId: number;
  instructorName: string;
  totalEarned: number;
  pendingAmount: number;
  totalPayouts: number;
  payouts: AdminPayoutItem[];
}
