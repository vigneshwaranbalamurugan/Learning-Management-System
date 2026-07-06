import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { LearningService } from './learning.service';
import { environment } from '@environments/environment';

describe('LearningService', () => {
  let service: LearningService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    });
    service = TestBed.inject(LearningService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should mark lesson complete', () => {
    service.markLessonComplete(1, 100).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/Lessons/1/complete`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ watchPercentage: 100 });
    req.flush({});
  });

  it('should get quiz for student', () => {
    service.getQuizForStudent(1).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/QuizAttempts/1/take`);
    expect(req.request.method).toBe('GET');
    req.flush({});
  });
});
