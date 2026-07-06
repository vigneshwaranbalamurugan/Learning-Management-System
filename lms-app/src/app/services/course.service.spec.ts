import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { CourseService } from './course.service';
import { environment } from '@environments/environment';

describe('CourseService', () => {
  let service: CourseService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    });
    service = TestBed.inject(CourseService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should get my courses', () => {
    service.getMyCourses({ pageNumber: 1, pageSize: 10 }).subscribe();
    const req = httpMock.expectOne(request => request.url === `${environment.apiUrl}/Courses/my-courses`);
    expect(req.request.method).toBe('GET');
    expect(req.request.params.get('pageNumber')).toBe('1');
    expect(req.request.params.get('pageSize')).toBe('10');
    req.flush({});
  });

  it('should get all categories', () => {
    service.getAllCategories().subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/CourseCategories`);
    expect(req.request.method).toBe('GET');
    req.flush([]);
  });

  it('should get course by id', () => {
    service.getCourseById(1).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/Courses/1`);
    expect(req.request.method).toBe('GET');
    req.flush({});
  });

  it('should get filters metadata', () => {
    service.getFiltersMetadata().subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/Courses/filters-metadata`);
    expect(req.request.method).toBe('GET');
    req.flush({});
  });
});
