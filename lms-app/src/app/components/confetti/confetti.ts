import { Component, Input, Output, EventEmitter, OnInit, OnChanges, SimpleChanges, ViewEncapsulation } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-confetti',
  standalone: true,
  imports: [CommonModule],
  encapsulation: ViewEncapsulation.None,
  template: `
    @if (isActive) {
      <div class="fixed inset-0 z-[9999] pointer-events-none flex items-center justify-center overflow-hidden">
        
        <!-- Overlay background -->
        <div class="absolute inset-0 bg-black/40 backdrop-blur-sm animate-[fadeIn_0.3s_ease-out]"></div>

        <!-- Confetti Particles -->
        @for (piece of pieces; track piece.id) {
          <div class="absolute top-[-10%] w-3 h-6 rounded-sm opacity-90"
               [style.left.%]="piece.left"
               [style.background-color]="piece.color"
               [style.animation]="piece.animation">
          </div>
        }

        <!-- Success Message -->
        <div class="relative z-10 bg-white px-8 py-10 rounded-3xl shadow-2xl flex flex-col items-center gap-4 text-center transform animate-[popIn_0.5s_cubic-bezier(0.175,0.885,0.32,1.275)] max-w-md w-full mx-4 pointer-events-auto">
          <div class="w-20 h-20 bg-green-100 rounded-full flex items-center justify-center mb-2">
            <svg class="w-10 h-10 text-green-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M5 13l4 4L19 7"></path>
            </svg>
          </div>
          <h2 class="text-3xl font-black text-[#1C1C7B]">{{ title }}</h2>
          <p class="text-gray-600 text-sm leading-relaxed">{{ message }}</p>
          <button (click)="close.emit()"
            class="mt-4 px-8 py-3 bg-[#FF8C00] text-white font-extrabold rounded-xl shadow-lg hover:bg-[#e67e00] hover:-translate-y-1 transition-all">
            Continue
          </button>
        </div>

      </div>
    }
  `,
  styles: [`
    @keyframes confettiFall {
      0% { transform: translateY(0vh) rotate(0deg) scale(1); opacity: 1; }
      100% { transform: translateY(120vh) rotate(720deg) scale(0.5); opacity: 0; }
    }
    @keyframes fadeIn {
      from { opacity: 0; }
      to { opacity: 1; }
    }
    @keyframes popIn {
      from { transform: scale(0.8) translateY(20px); opacity: 0; }
      to { transform: scale(1) translateY(0); opacity: 1; }
    }
  `]
})
export class ConfettiComponent implements OnInit, OnChanges {
  @Input() isActive = false;
  @Input() title = 'Success!';
  @Input() message = 'Happy Learning!';
  @Output() close = new EventEmitter<void>();

  pieces: any[] = [];
  private colors = ['#1C1C7B', '#FF8C00', '#f59e0b', '#3b82f6', '#10b981', '#ef4444']; // primary, secondary + lively colors

  ngOnInit() {
    this.generateConfetti();
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes['isActive'] && changes['isActive'].currentValue) {
      this.generateConfetti();
    }
  }

  private generateConfetti() {
    this.pieces = [];
    for (let i = 0; i < 150; i++) {
      const left = Math.random() * 100; // 0 to 100%
      const color = this.colors[Math.floor(Math.random() * this.colors.length)];
      const duration = 1.5 + Math.random() * 2; // 1.5s to 3.5s
      const delay = Math.random() * 0.2; // 0s to 0.2s
      
      this.pieces.push({
        id: i,
        left,
        color,
        animation: `confettiFall ${duration}s ease-in ${delay}s forwards`
      });
    }
  }
}
