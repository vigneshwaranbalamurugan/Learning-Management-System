import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { NotificationService } from './notification.service';
import { environment } from '@environments/environment';

describe('NotificationService', () => {
  let service: NotificationService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    });
    service = TestBed.inject(NotificationService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should fetch unread count', () => {
    service.getUnreadCount().subscribe(count => {
      expect(count).toBe(5);
    });

    const req = httpMock.expectOne(`${environment.apiUrl}/notifications/unread-count`);
    expect(req.request.method).toBe('GET');
    req.flush(5);
  });

  it('should get notifications', () => {
    const mockData = [{ id: 1, message: 'Test notification' }];
    service.getNotifications(1, 10).subscribe(data => {
      expect(data).toEqual(mockData as any);
    });

    const req = httpMock.expectOne(`${environment.apiUrl}/notifications?page=1&pageSize=10`);
    expect(req.request.method).toBe('GET');
    req.flush(mockData);
  });

  it('should mark as read', () => {
    service.markAsRead(123).subscribe();

    const req = httpMock.expectOne(`${environment.apiUrl}/notifications/123/read`);
    expect(req.request.method).toBe('PATCH');
    req.flush({});
  });

  it('should mark all as read', () => {
    service.markAllAsRead().subscribe();

    const req = httpMock.expectOne(`${environment.apiUrl}/notifications/read-all`);
    expect(req.request.method).toBe('PATCH');
    req.flush({});
  });
});
