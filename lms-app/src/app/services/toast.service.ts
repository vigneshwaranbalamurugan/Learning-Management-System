import { Injectable, signal } from '@angular/core';
import { Toast, ToastType } from '../models/toast';

@Injectable({
  providedIn: 'root',
})
export class ToastService {
  private readonly toastsSignal = signal<Toast[]>([]);
  public readonly toasts = this.toastsSignal.asReadonly();

  show(message: string, type: ToastType = 'info', duration: number = 3000) {
    const id = Math.random().toString(36).substring(2, 9);
    const toast: Toast = { id, type, message, duration };
    this.toastsSignal.update((current) => [...current, toast]);
  }

  showSuccess(message: string, duration = 3000) {
    this.show(message, 'success', duration);
  }

  showError(message: string, duration = 3000) {
    this.show(message, 'error', duration);
  }

  showWarning(message: string, duration = 3000) {
    this.show(message, 'warning', duration);
  }

  showInfo(message: string, duration = 3000) {
    this.show(message, 'info', duration);
  }

  dismiss(id: string) {
    this.toastsSignal.update((current) => current.filter((t) => t.id !== id));
  }
}
