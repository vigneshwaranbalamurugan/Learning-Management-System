import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { CertificateService } from './certificate.service';
import { environment } from '@environments/environment';

describe('CertificateService', () => {
  let service: CertificateService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    });
    service = TestBed.inject(CertificateService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should get my certificates', () => {
    service.getMyCertificates(1, 10).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/certificates/my?page=1&pageSize=10`);
    expect(req.request.method).toBe('GET');
    req.flush({});
  });

  it('should verify certificate', () => {
    service.verifyCertificate('abc').subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/certificates/verify/abc`);
    expect(req.request.method).toBe('GET');
    req.flush({});
  });
});
