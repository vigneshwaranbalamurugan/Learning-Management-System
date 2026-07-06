import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Topbar } from './topbar';
import { RouterTestingModule } from '@angular/router/testing';
import { SignalRService } from '@services/signalr.service';
import { NotificationService } from '@services/notification.service';
import { ComponentRef } from '@angular/core';
import { of, Subject } from 'rxjs';
import { ActivatedRoute, NavigationEnd, Router } from '@angular/router';
import { vi } from 'vitest';

describe('Topbar', () => {
  let component: Topbar;
  let fixture: ComponentFixture<Topbar>;
  let componentRef: ComponentRef<Topbar>;
  let mockSignalRService: any;
  let mockNotificationService: any;
  let routerEventsSubject: Subject<any>;
  let router: Router;

  beforeEach(async () => {
    routerEventsSubject = new Subject<any>();
    
    mockSignalRService = {
      connect: vi.fn(),
      unreadCount$: new Subject<number>()
    };
    
    mockNotificationService = {
      getUnreadCount: vi.fn().mockReturnValue(of(3))
    };

    await TestBed.configureTestingModule({
      imports: [Topbar, RouterTestingModule],
      providers: [
        { provide: SignalRService, useValue: mockSignalRService },
        { provide: NotificationService, useValue: mockNotificationService },
        {
          provide: Router,
          useValue: {
            url: '/dashboard',
            events: routerEventsSubject.asObservable(),
            navigate: vi.fn()
          }
        },
        { provide: ActivatedRoute, useValue: {} }
      ]
    })
    .compileComponents();
    
    router = TestBed.inject(Router);
    
    fixture = TestBed.createComponent(Topbar);
    component = fixture.componentInstance;
    componentRef = fixture.componentRef;
    
    componentRef.setInput('user', { firstName: 'John', role: 'Learner' });
    
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should update title and close panel on NavigationEnd', () => {
    component['isPanelOpen'].set(true);
    
    routerEventsSubject.next(new NavigationEnd(1, '/profile', '/profile'));
    
    expect(component['pageTitle']()).toBe('My Profile');
    expect(component['isPanelOpen']()).toBe(false);
  });

  it('should return correct avatarInitial', () => {
    expect(component['avatarInitial']).toBe('J'); // John -> J
    
    componentRef.setInput('user', { firstName: '', role: 'Learner' });
    expect(component['avatarInitial']).toBe('U'); // Fallback -> U
  });

  it('should toggle dropdown', () => {
    const event = new MouseEvent('click');
    vi.spyOn(event, 'stopPropagation');
    
    expect(component['isDropdownOpen']()).toBe(false);
    component['toggleDropdown'](event);
    
    expect(component['isDropdownOpen']()).toBe(true);
    expect(event.stopPropagation).toHaveBeenCalled();
  });

  it('should toggle panel and close dropdown if open', () => {
    const event = new MouseEvent('click');
    vi.spyOn(event, 'stopPropagation');
    
    component['isDropdownOpen'].set(true);
    expect(component['isPanelOpen']()).toBe(false);
    
    component['togglePanel'](event);
    
    expect(component['isPanelOpen']()).toBe(true);
    expect(component['isDropdownOpen']()).toBe(false);
    expect(event.stopPropagation).toHaveBeenCalled();
  });

  it('should initialize unread count from service and SignalR', () => {
    expect(mockNotificationService.getUnreadCount).toHaveBeenCalled();
    expect(component['unreadCount']()).toBe(3);
    expect(mockSignalRService.connect).toHaveBeenCalled();
    
    // Simulate SignalR update
    mockSignalRService.unreadCount$.next(7);
    expect(component['unreadCount']()).toBe(7);
  });
});
