import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ToastService } from '@services/toast.service';
import { ToastItemComponent } from './toast-item/toast-item';

@Component({
  selector: 'app-toaster',
  standalone: true,
  imports: [CommonModule, ToastItemComponent],
  templateUrl: './toaster.html',
  styleUrl: './toaster.css',
})
export class Toaster {
  constructor(protected toastService: ToastService) { }
}
