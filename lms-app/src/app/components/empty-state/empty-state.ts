import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-empty-state',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <div class="flex flex-col items-center justify-center p-8 text-center bg-white rounded-2xl border border-gray-100 shadow-sm max-w-md w-full mx-auto">
      <div class="text-6xl mb-6">
        {{ icon }}
      </div>
      <h3 class="text-2xl font-bold text-gray-900 mb-2">{{ title }}</h3>
      <p class="text-gray-500 mb-8">{{ message }}</p>
      
      <a *ngIf="actionLabel && actionRoute"
         [routerLink]="actionRoute"
         class="px-6 py-3 bg-[#1C1C7B] hover:bg-[#14145a] text-white rounded-xl font-bold transition-all shadow-md shadow-[#1C1C7B]/20 hover:shadow-lg hover:shadow-[#1C1C7B]/40 hover:-translate-y-0.5 active:translate-y-0">
        {{ actionLabel }}
      </a>
    </div>
  `
})
export class EmptyState {
  @Input() title: string = 'No Data';
  @Input() message: string = 'There is nothing to display right now.';
  @Input() icon: string = '📁';
  @Input() actionLabel?: string;
  @Input() actionRoute?: string;
}
