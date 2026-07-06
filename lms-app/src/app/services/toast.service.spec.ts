import { TestBed } from '@angular/core/testing';
import { ToastService } from './toast.service';
import { ToastType } from '@models/toast';

describe('ToastService', () => {
  let service: ToastService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ToastService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should add a toast with correct type and message', () => {
    service.show('Test message', 'success', 2000);
    const toasts = service.toasts();
    expect(toasts.length).toBe(1);
    expect(toasts[0].message).toBe('Test message');
    expect(toasts[0].type).toBe('success');
    expect(toasts[0].duration).toBe(2000);
    expect(toasts[0].id).toBeTruthy();
  });

  it('should delegate showSuccess correctly', () => {
    service.showSuccess('Success', 1000);
    expect(service.toasts()[0].type).toBe('success');
  });

  it('should delegate showError correctly', () => {
    service.showError('Error', 1000);
    expect(service.toasts()[0].type).toBe('error');
  });

  it('should delegate showWarning correctly', () => {
    service.showWarning('Warning', 1000);
    expect(service.toasts()[0].type).toBe('warning');
  });

  it('should delegate showInfo correctly', () => {
    service.showInfo('Info', 1000);
    expect(service.toasts()[0].type).toBe('info');
  });

  it('should dismiss a toast by id', () => {
    service.show('Message 1');
    service.show('Message 2');
    const toasts = service.toasts();
    expect(toasts.length).toBe(2);
    
    const idToDismiss = toasts[0].id;
    service.dismiss(idToDismiss);
    
    const updatedToasts = service.toasts();
    expect(updatedToasts.length).toBe(1);
    expect(updatedToasts[0].message).toBe('Message 2');
  });

  describe('showApiError', () => {
    it('should handle string error', () => {
      service.showApiError('String error', 'Fallback');
      expect(service.toasts()[0].message).toBe('String error');
    });

    it('should handle err.error as string', () => {
      service.showApiError({ error: 'Server error string' }, 'Fallback');
      expect(service.toasts()[0].message).toBe('Server error string');
    });

    it('should handle err.error.message', () => {
      service.showApiError({ error: { message: 'Server error message' } }, 'Fallback');
      expect(service.toasts()[0].message).toBe('Server error message');
    });

    it('should handle err.error.title', () => {
      service.showApiError({ error: { title: 'Server error title' } }, 'Fallback');
      expect(service.toasts()[0].message).toBe('Server error title');
    });

    it('should handle err.message', () => {
      service.showApiError({ message: 'General error' }, 'Fallback');
      expect(service.toasts()[0].message).toBe('General error');
    });

    it('should use fallback message if error is null/undefined', () => {
      service.showApiError(null, 'Fallback error');
      expect(service.toasts()[0].message).toBe('Fallback error');
    });
  });
});
