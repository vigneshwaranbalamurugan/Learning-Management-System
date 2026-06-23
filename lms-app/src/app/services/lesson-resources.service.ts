import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { 
  ResourceResponse, 
  CreateResourceRequest, 
  UpdateResourceRequest, 
  ReorderResourcesRequest 
} from '@models/dashboard';

@Injectable({
  providedIn: 'root'
})
export class LessonResourcesService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/LessonResources`;

  getResourcesByLesson(lessonId: number): Observable<ResourceResponse[]> {
    return this.http.get<ResourceResponse[]>(`${this.apiUrl}/lesson/${lessonId}`);
  }

  getResourceById(id: number): Observable<ResourceResponse> {
    return this.http.get<ResourceResponse>(`${this.apiUrl}/${id}`);
  }

  addResource(request: CreateResourceRequest): Observable<ResourceResponse> {
    const formData = new FormData();
    formData.append('lessonId', request.lessonId.toString());
    formData.append('resourceType', request.resourceType.toString());
    formData.append('resourceTitle', request.resourceTitle);
    
    if (request.resourceUrl) {
      formData.append('resourceUrl', request.resourceUrl);
    }
    if (request.description) {
      formData.append('description', request.description);
    }
    formData.append('status', request.status.toString());
    
    if (request.file) {
      formData.append('file', request.file);
    }

    return this.http.post<ResourceResponse>(this.apiUrl, formData);
  }

  updateResource(id: number, request: UpdateResourceRequest): Observable<ResourceResponse> {
    const formData = new FormData();
    if (request.resourceType !== undefined) {
      formData.append('resourceType', request.resourceType.toString());
    }
    if (request.resourceTitle) {
      formData.append('resourceTitle', request.resourceTitle);
    }
    if (request.resourceUrl) {
      formData.append('resourceUrl', request.resourceUrl);
    }
    if (request.description !== undefined) {
      formData.append('description', request.description);
    }
    if (request.status !== undefined) {
      formData.append('status', request.status.toString());
    }
    if (request.file) {
      formData.append('file', request.file);
    }

    return this.http.put<ResourceResponse>(`${this.apiUrl}/${id}`, formData);
  }

  deleteResource(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  publishResource(id: number, publish: boolean): Observable<ResourceResponse> {
    return this.http.patch<ResourceResponse>(`${this.apiUrl}/${id}/publish`, { publish });
  }

  reorderResources(lessonId: number, request: ReorderResourcesRequest): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/reorder/${lessonId}`, request);
  }
}
