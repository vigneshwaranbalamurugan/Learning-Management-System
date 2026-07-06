import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { ReviewService } from './review.service';
import { environment } from '@environments/environment';

describe('ReviewService', () => {
  let service: ReviewService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    });
    service = TestBed.inject(ReviewService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should get course reviews', () => {
    service.getCourseReviews(1).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/Reviews/course/1`);
    expect(req.request.method).toBe('GET');
    req.flush({});
  });

  it('should submit review', () => {
    service.submitReview({ courseId: 1, rating: 5, reviewText: 'Great' }).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/Reviews`);
    expect(req.request.method).toBe('POST');
    req.flush({});
  });

  it('should get instructor reviews', () => {
    service.getInstructorReviews(1, 10, 5, 1, 'search').subscribe();
    const req = httpMock.expectOne(request => request.url.includes(`/Reviews/instructor`));
    expect(req.request.method).toBe('GET');
    req.flush({});
  });
});
