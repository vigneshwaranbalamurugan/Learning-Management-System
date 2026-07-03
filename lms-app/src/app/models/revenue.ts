export interface InstructorPayoutResponse {
  id: number;
  paymentId: number;
  courseName: string;
  studentName?: string;
  amount: number;
  status: string;
  razorpayTransferId?: string;
  failureReason?: string;
  createdAt: string;
  updatedAt: string;
}

export interface InstructorRevenueSummaryResponse {
  instructorId: number;
  instructorName: string;
  totalEarned: number;
  pendingAmount: number;
  totalPayouts: number;
  payouts: InstructorPayoutResponse[];
}

export interface PagedInstructorRevenueSummaryResponse extends InstructorRevenueSummaryResponse {
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}
