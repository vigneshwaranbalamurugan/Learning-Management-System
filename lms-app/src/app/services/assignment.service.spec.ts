import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { AssignmentService } from './assignment.service';
import { environment } from '@environments/environment';

describe('AssignmentService', () => {
  let service: AssignmentService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    });
    service = TestBed.inject(AssignmentService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should get my assignments', () => {
    service.getMyAssignments(1, 10, 'search').subscribe();
    const req = httpMock.expectOne(request => request.url.includes(`/Assignments/my-assignments`));
    expect(req.request.method).toBe('GET');
    req.flush({});
  });

  it('should get assignments by section', () => {
    service.getAssignmentsBySection(1).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/Assignments/section/1`);
    expect(req.request.method).toBe('GET');
    req.flush({});
  });

  it('should submit assignment', () => {
    const formData = new FormData();
    service.submitAssignment(formData).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/AssignmentSubmissions`);
    expect(req.request.method).toBe('POST');
    req.flush({});
  });
});
