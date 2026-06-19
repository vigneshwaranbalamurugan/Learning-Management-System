import { Component, Input, Output, EventEmitter, ElementRef, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-dropdown',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dropdown.html',
  styleUrl: './dropdown.css',
})
export class Dropdown {
  @Input() label: string = '';
  @Input() placeholder: string = 'Select an option';
  @Input() id: string = '';
  @Input() value: string = '';
  @Input() options: { value: string; label: string }[] = [];
  @Input() icon: 'email' | 'lock' | 'user' | 'role' | 'none' = 'none';
  @Input() error: string = '';
  @Output() valueChange = new EventEmitter<string>();

  protected isOpen = false;

  constructor(private elementRef: ElementRef) {}

  protected toggleDropdown(): void {
    this.isOpen = !this.isOpen;
  }

  protected selectOption(optionValue: string): void {
    this.value = optionValue;
    this.valueChange.emit(this.value);
    this.isOpen = false;
  }

  protected get selectedLabel(): string {
    const selected = this.options.find(opt => opt.value === this.value);
    return selected ? selected.label : '';
  }

  @HostListener('document:click', ['$event'])
  protected onClickOutside(event: MouseEvent): void {
    if (!this.elementRef.nativeElement.contains(event.target)) {
      this.isOpen = false;
    }
  }
}
