import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { EnrollmentService } from './enrollment.service';
import { environment } from '@environments/environment';

describe('EnrollmentService', () => {
  let service: EnrollmentService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    });
    service = TestBed.inject(EnrollmentService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should get my enrollments', () => {
    service.getMyEnrollments(1, 10, 'search').subscribe();
    const req = httpMock.expectOne(request => request.url.includes('/enrollments/my'));
    expect(req.request.method).toBe('GET');
    req.flush({});
  });

  it('should get all my enrollments', () => {
    service.getAllMyEnrollments().subscribe();
    const req = httpMock.expectOne(request => request.url.includes('/enrollments/my'));
    expect(req.request.method).toBe('GET');
    req.flush({ enrollments: [] });
  });

  it('should enroll free course', () => {
    service.enrollFreeCourse(123).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/courses/123/enroll/free`);
    expect(req.request.method).toBe('POST');
    req.flush({});
  });

  it('should verify payment', () => {
    service.verifyPayment(123, {}).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/courses/123/enroll/verify`);
    expect(req.request.method).toBe('POST');
    req.flush({});
  });
});
