import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '@environments/environment';

export interface UserProfile {
  fullName: string;
  firstName: string;
  lastName: string;
  bio: string;
  dateOfBirth: string;
  location: string;
  profilePictureUrl: string;
  email?: string;
  role?: string;
}

@Injectable({
  providedIn: 'root',
})
export class ProfileService {
  private http = inject(HttpClient);
  private readonly baseUrl = environment.apiUrl;

  getProfile(): Observable<UserProfile> {
    return this.http.get<UserProfile>(`${this.baseUrl}/profile/get-profile`);
  }

  updateProfile(profile: Partial<UserProfile>): Observable<any> {
    return this.http.put<any>(`${this.baseUrl}/profile/update-profile`, profile);
  }

  updateProfileImage(file: File): Observable<any> {
    const formData = new FormData();
    formData.append('File', file);
    return this.http.post<any>(`${this.baseUrl}/profile/update-profile-image`, formData);
  }

  getFileLimits(): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/profile/file-limits`);
  }
}
