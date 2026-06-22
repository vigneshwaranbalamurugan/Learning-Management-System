import { Component, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-pagination',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div *ngIf="totalPages() > 1" class="flex items-center justify-between px-4 py-3 bg-white border border-gray-200 rounded-xl shadow-xs mt-4">
      <!-- Mobile: Prev / Next only -->
      <div class="flex flex-1 justify-between sm:hidden">
        <button
          [disabled]="pageNumber() === 1"
          (click)="changePage(pageNumber() - 1)"
          class="relative inline-flex items-center rounded-md border border-gray-300 bg-white px-4 py-2 text-xs font-semibold text-gray-700 hover:bg-gray-50 transition cursor-pointer disabled:opacity-40 disabled:cursor-not-allowed">
          Previous
        </button>
        <span class="text-xs text-gray-500 self-center">{{ pageNumber() }} / {{ totalPages() }}</span>
        <button
          [disabled]="pageNumber() === totalPages()"
          (click)="changePage(pageNumber() + 1)"
          class="relative ml-3 inline-flex items-center rounded-md border border-gray-300 bg-white px-4 py-2 text-xs font-semibold text-gray-700 hover:bg-gray-50 transition cursor-pointer disabled:opacity-40 disabled:cursor-not-allowed">
          Next
        </button>
      </div>

      <!-- Desktop: Full pagination -->
      <div class="hidden sm:flex sm:flex-1 sm:items-center sm:justify-between">
        <p class="text-xs text-gray-500">
          Showing page <span class="font-bold text-gray-900">{{ pageNumber() }}</span> of
          <span class="font-bold text-gray-900">{{ totalPages() }}</span>
          (<span class="font-bold text-gray-900">{{ totalCount() }}</span> total)
        </p>

        <nav class="isolate inline-flex -space-x-px rounded-md shadow-xs" aria-label="Pagination">
          <!-- Prev -->
          <button
            [disabled]="pageNumber() === 1"
            (click)="changePage(pageNumber() - 1)"
            class="relative inline-flex items-center rounded-l-md px-2 py-2 text-gray-400 ring-1 ring-inset ring-gray-300 hover:bg-gray-50 cursor-pointer disabled:opacity-40 disabled:cursor-not-allowed transition">
            <span class="sr-only">Previous</span>
            <svg class="h-4 w-4" viewBox="0 0 20 20" fill="currentColor"><path fill-rule="evenodd" d="M12.79 5.23a.75.75 0 01-.02 1.06L8.832 10l3.938 3.71a.75.75 0 11-1.04 1.08l-4.5-4.25a.75.75 0 010-1.08l4.5-4.25a.75.75 0 011.06.02z" clip-rule="evenodd" /></svg>
          </button>

          <!-- First page -->
          <button *ngIf="pageNumber() > 3"
            (click)="changePage(1)"
            class="relative inline-flex items-center px-3 py-1.5 text-xs font-semibold text-gray-900 ring-1 ring-inset ring-gray-300 hover:bg-gray-50 cursor-pointer bg-white transition">
            1
          </button>
          <span *ngIf="pageNumber() > 4"
            class="relative inline-flex items-center px-2 py-1.5 text-xs text-gray-500 ring-1 ring-inset ring-gray-300">
            ...
          </span>

          <!-- Page range -->
          <button *ngFor="let page of visiblePages()"
            (click)="changePage(page)"
            [class]="page === pageNumber()
              ? 'relative inline-flex items-center px-3 py-1.5 text-xs font-bold cursor-pointer bg-[#1C1C7B] text-white border-none'
              : 'relative inline-flex items-center px-3 py-1.5 text-xs font-semibold text-gray-900 ring-1 ring-inset ring-gray-300 hover:bg-gray-50 cursor-pointer bg-white transition'">
            {{ page }}
          </button>

          <!-- Last page -->
          <span *ngIf="pageNumber() < totalPages() - 3"
            class="relative inline-flex items-center px-2 py-1.5 text-xs text-gray-500 ring-1 ring-inset ring-gray-300">
            ...
          </span>
          <button *ngIf="pageNumber() < totalPages() - 2"
            (click)="changePage(totalPages())"
            class="relative inline-flex items-center px-3 py-1.5 text-xs font-semibold text-gray-900 ring-1 ring-inset ring-gray-300 hover:bg-gray-50 cursor-pointer bg-white transition">
            {{ totalPages() }}
          </button>

          <!-- Next -->
          <button
            [disabled]="pageNumber() === totalPages()"
            (click)="changePage(pageNumber() + 1)"
            class="relative inline-flex items-center rounded-r-md px-2 py-2 text-gray-400 ring-1 ring-inset ring-gray-300 hover:bg-gray-50 cursor-pointer disabled:opacity-40 disabled:cursor-not-allowed transition">
            <span class="sr-only">Next</span>
            <svg class="h-4 w-4" viewBox="0 0 20 20" fill="currentColor"><path fill-rule="evenodd" d="M7.21 14.77a.75.75 0 01.02-1.06L11.168 10 7.23 6.29a.75.75 0 111.04-1.08l4.5 4.25a.75.75 0 010 1.08l-4.5 4.25a.75.75 0 01-1.06-.02z" clip-rule="evenodd" /></svg>
          </button>
        </nav>
      </div>
    </div>
  `
})
export class PaginationComponent {
  pageNumber = input<number>(1);
  totalPages = input<number>(1);
  totalCount = input<number>(0);
  pageSize   = input<number>(6);

  pageChange = output<number>();

  visiblePages(): number[] {
    const current = this.pageNumber();
    const total   = this.totalPages();
    const range   = 2;
    const pages: number[] = [];
    for (let p = Math.max(1, current - range); p <= Math.min(total, current + range); p++) {
      pages.push(p);
    }
    return pages;
  }

  changePage(page: number): void {
    if (page < 1 || page > this.totalPages() || page === this.pageNumber()) return;
    this.pageChange.emit(page);
  }
}
