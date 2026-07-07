import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { AdminUserResponse, CreateUserRequest, PagedUserListResponse, UserSearchQuery } from '../models/admin-user';

@Injectable({
  providedIn: 'root'
})
export class AdminUserService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/AdminUsers`;

  getUsers(query: UserSearchQuery): Observable<PagedUserListResponse> {
    let params = new HttpParams();
    
    if (query.pageNumber) params = params.set('PageNumber', query.pageNumber);
    if (query.pageSize) params = params.set('PageSize', query.pageSize);
    if (query.search) params = params.set('Search', query.search);
    if (query.roleId) params = params.set('RoleId', query.roleId);
    if (query.isActive !== undefined && query.isActive !== null) params = params.set('IsActive', query.isActive);

    return this.http.get<PagedUserListResponse>(this.apiUrl, { params });
  }

  createUser(request: CreateUserRequest): Observable<AdminUserResponse> {
    return this.http.post<AdminUserResponse>(this.apiUrl, request);
  }
}
