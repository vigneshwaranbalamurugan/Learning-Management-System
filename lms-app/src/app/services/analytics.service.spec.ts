import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { AnalyticsService } from './analytics.service';
import { environment } from '@environments/environment';

describe('AnalyticsService', () => {
  let service: AnalyticsService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    });
    service = TestBed.inject(AnalyticsService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should get learner analytics', () => {
    service.getLearnerAnalytics().subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/analytics/learner`);
    expect(req.request.method).toBe('GET');
    req.flush({});
  });

  it('should get instructor analytics', () => {
    service.getInstructorAnalytics().subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/analytics/instructor`);
    expect(req.request.method).toBe('GET');
    req.flush({});
  });
});
