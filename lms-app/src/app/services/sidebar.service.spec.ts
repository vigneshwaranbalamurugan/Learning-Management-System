import { TestBed } from '@angular/core/testing';
import { SidebarService } from './sidebar.service';

describe('SidebarService', () => {
  let service: SidebarService;
  
  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(SidebarService);
  });
  
  it('should be created', () => {
    expect(service).toBeTruthy();
  });
  
  describe('checkScreenSize', () => {
    it('should handle mobile screen size (< 768)', () => {
      Object.defineProperty(window, 'innerWidth', { writable: true, configurable: true, value: 500 });
      service.checkScreenSize();
      
      expect(service.isMobile()).toBe(true);
      expect(service.isCollapsed()).toBe(true);
      expect(service.isMobileSidebarOpen()).toBe(false);
    });
    
    it('should handle tablet screen size (< 1024)', () => {
      Object.defineProperty(window, 'innerWidth', { writable: true, configurable: true, value: 800 });
      service.checkScreenSize();
      
      expect(service.isMobile()).toBe(false);
      expect(service.isCollapsed()).toBe(true);
      expect(service.isMobileSidebarOpen()).toBe(false);
    });
    
    it('should handle desktop screen size (>= 1024)', () => {
      Object.defineProperty(window, 'innerWidth', { writable: true, configurable: true, value: 1200 });
      service.checkScreenSize();
      
      expect(service.isMobile()).toBe(false);
      expect(service.isCollapsed()).toBe(false);
      expect(service.isMobileSidebarOpen()).toBe(false);
    });
  });
  
  describe('toggle', () => {
    it('should toggle isMobileSidebarOpen on mobile', () => {
      service.isMobile.set(true);
      service.isMobileSidebarOpen.set(false);
      
      service.toggle();
      expect(service.isMobileSidebarOpen()).toBe(true);
      
      service.toggle();
      expect(service.isMobileSidebarOpen()).toBe(false);
    });
    
    it('should toggle isCollapsed on desktop', () => {
      service.isMobile.set(false);
      service.isCollapsed.set(false);
      
      service.toggle();
      expect(service.isCollapsed()).toBe(true);
      
      service.toggle();
      expect(service.isCollapsed()).toBe(false);
    });
  });
  
  describe('closeMobile', () => {
    it('should set isMobileSidebarOpen to false', () => {
      service.isMobileSidebarOpen.set(true);
      service.closeMobile();
      expect(service.isMobileSidebarOpen()).toBe(false);
    });
  });
});
