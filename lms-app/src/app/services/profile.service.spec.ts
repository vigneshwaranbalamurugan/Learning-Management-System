import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { ProfileService, UserProfile } from './profile.service';
import { environment } from '@environments/environment';

describe('ProfileService', () => {
  let service: ProfileService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    });
    service = TestBed.inject(ProfileService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should get profile', () => {
    service.getProfile().subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/profile/get-profile`);
    expect(req.request.method).toBe('GET');
    req.flush({});
  });

  it('should update profile', () => {
    const profile: Partial<UserProfile> = { firstName: 'Test', lastName: 'User', role: 'Learner', email: 'test@example.com' };
    service.updateProfile(profile).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/profile/update-profile`);
    expect(req.request.method).toBe('PUT');
    expect(req.request.body).toEqual(profile);
    req.flush({});
  });
});
