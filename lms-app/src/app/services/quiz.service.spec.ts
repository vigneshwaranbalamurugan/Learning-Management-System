import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { QuizService } from './quiz.service';
import { environment } from '@environments/environment';

describe('QuizService', () => {
  let service: QuizService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    });
    service = TestBed.inject(QuizService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should get quiz details', () => {
    service.getQuiz(1).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/Quizzes/1`);
    expect(req.request.method).toBe('GET');
    req.flush({});
  });

  it('should get my quiz attempts', () => {
    service.getMyQuizAttempts(1, 10).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/QuizAttempts/my?page=1&pageSize=10`);
    expect(req.request.method).toBe('GET');
    req.flush({});
  });

  it('should get quiz attempt detail', () => {
    service.getQuizAttemptDetail(1).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/QuizAttempts/1`);
    expect(req.request.method).toBe('GET');
    req.flush({});
  });
});
