export interface ReviewResponse {
  id: number;
  courseId: number;
  userId: number;
  userName: string;
  rating: number;
  reviewText: string;
  createdAt: string;
  updatedAt: string;
}

export interface CreateReviewRequest {
  courseId: number;
  rating: number;
  reviewText: string;
}

export interface UpdateReviewRequest {
  rating?: number;
  reviewText?: string;
}
