export interface DiscussionResponse {
  id: number;
  courseId: number;
  lessonId: number;
  userId: number;
  userName: string;
  userEmail: string;
  title: string;
  content: string;
  isPinned: boolean;
  isLocked: boolean;
  createdAt: string;
  updatedAt: string;
  replyCount: number;
  isLikedByUser: boolean;
  likeCount: number;
}

export interface ReplyResponse {
  id: number;
  discussionId: number;
  userId: number;
  userName: string;
  userEmail: string;
  replyText: string;
  createdAt: string;
  updatedAt: string;
}

export interface DiscussionDetailResponse extends DiscussionResponse {
  replies: ReplyResponse[];
}

export interface CreateDiscussionRequest {
  lessonId: number;
  title: string;
  content: string;
}

export interface UpdateDiscussionRequest {
  title?: string;
  content?: string;
}

export interface CreateReplyRequest {
  replyText: string;
}

export interface UpdateReplyRequest {
  replyText: string;
}
