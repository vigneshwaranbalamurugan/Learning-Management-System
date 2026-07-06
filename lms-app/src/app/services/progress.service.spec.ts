import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { ProgressService } from './progress.service';
import { environment } from '@environments/environment';

describe('ProgressService', () => {
  let service: ProgressService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    });
    service = TestBed.inject(ProgressService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should get course progress detail', () => {
    service.getCourseProgress(1).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/Progress/course/1`);
    expect(req.request.method).toBe('GET');
    req.flush({});
  });

  it('should get students progress', () => {
    service.getStudentsProgress(1, 1, 10).subscribe();
    const req = httpMock.expectOne(request => request.url.includes(`/Progress/course/1/students`));
    expect(req.request.method).toBe('GET');
    req.flush({});
  });

  it('should get course analytics', () => {
    service.getCourseAnalytics(1).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/Progress/course/1/analytics`);
    expect(req.request.method).toBe('GET');
    req.flush({});
  });
});
