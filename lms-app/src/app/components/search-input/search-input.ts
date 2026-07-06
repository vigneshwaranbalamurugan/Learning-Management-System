import { Component, Input, Output, EventEmitter, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Subject, Subscription } from 'rxjs';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';

@Component({
  selector: 'app-search-input',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './search-input.html',
  styleUrl: './search-input.css',
})
export class SearchInput implements OnInit, OnDestroy {
  @Input() placeholder: string = 'Search...';
  @Input() id: string = 'search-input';
  @Input() value: string = '';
  @Input() debounceTimeMs: number = 300;
  @Output() searchChange = new EventEmitter<string>();

  private searchSubject = new Subject<string>();
  private subscription: Subscription | undefined;

  ngOnInit() {
    this.subscription = this.searchSubject
      .pipe(
        debounceTime(this.debounceTimeMs),
        distinctUntilChanged()
      )
      .subscribe(searchValue => {
        this.searchChange.emit(searchValue);
      });
  }

  ngOnDestroy() {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }

  onInput(event: Event) {
    const target = event.target as HTMLInputElement;
    this.value = target.value;
    this.searchSubject.next(this.value);
  }

  clearSearch() {
    this.value = '';
    this.searchSubject.next(this.value);
  }
}
