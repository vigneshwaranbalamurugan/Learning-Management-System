import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { RevenueService } from './revenue.service';
import { environment } from '@environments/environment';

describe('RevenueService', () => {
  let service: RevenueService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    });
    service = TestBed.inject(RevenueService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should get instructor revenue', () => {
    service.getInstructorRevenue(1, 10, 'search', 'completed').subscribe();
    const req = httpMock.expectOne(request => request.url.includes(`/revenue/instructor`));
    expect(req.request.method).toBe('GET');
    req.flush({});
  });
});
