import { Component, input, output, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CategoryResponse } from '@models/course';

@Component({
  selector: 'app-category-filter',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './category-filter.html'
})
export class CategoryFilter {
  categories = input<CategoryResponse[]>([]);
  selectedId = input<number | null>(null);
  categoryChange = output<number | null>();

  isOpen = signal<boolean>(false);

  protected toggleCollapse() {
    this.isOpen.update(v => !v);
  }

  protected onSelect(id: number | null) {
    this.categoryChange.emit(id);
    this.isOpen.set(false);
  }
}
