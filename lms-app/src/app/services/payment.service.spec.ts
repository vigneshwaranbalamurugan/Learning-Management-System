import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { PaymentService } from './payment.service';
import { environment } from '@environments/environment';

describe('PaymentService', () => {
  let service: PaymentService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    });
    service = TestBed.inject(PaymentService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should get my payments', () => {
    service.getMyPayments('', '', 1, 10).subscribe();
    const req = httpMock.expectOne(request => request.url.includes('/payments/my'));
    expect(req.request.method).toBe('GET');
    req.flush({});
  });

  it('should download invoice', () => {
    service.downloadInvoice(1).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/payments/my/1/invoice`);
    expect(req.request.method).toBe('GET');
    req.flush(new Blob());
  });
});
