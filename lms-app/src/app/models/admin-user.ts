export interface AdminUserResponse {
  id: number;
  email: string;
  firstName: string;
  lastName: string;
  role: string;
  isActive: boolean;
  createdAt: string;
  lastLoginAt: string | null;
}

export interface CreateUserRequest {
  email: string;
  password?: string;
  role: string;
}

export interface UserSearchQuery {
  pageNumber?: number;
  pageSize?: number;
  search?: string;
  roleId?: number | null;
  isActive?: boolean | null;
}

export interface PagedUserListResponse {
  users: AdminUserResponse[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}
