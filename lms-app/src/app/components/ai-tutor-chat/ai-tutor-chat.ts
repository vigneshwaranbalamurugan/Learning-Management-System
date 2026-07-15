import {
  Component, OnInit, OnChanges, SimpleChanges,
  Input, signal, inject, ElementRef, ViewChild, AfterViewChecked
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AiService, AiChatMessage } from '@services/ai.service';

interface ChatEntry {
  role: 'user' | 'assistant';
  content: string;
  isLoading?: boolean;
}

const SUGGESTED_QUESTIONS = [
  'Explain this lesson in simple terms.',
  'What are the key concepts?',
  'Give me a real-world example.',
  'What should I remember from this lesson?',
  'How does this relate to what I already know?'
];

@Component({
  selector: 'app-ai-tutor-chat',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './ai-tutor-chat.html',
  styleUrl: './ai-tutor-chat.css'
})
export class AiTutorChat implements OnInit, OnChanges, AfterViewChecked {
  @Input() lessonId!: number;
  @Input() lessonType!: number; // LessonType enum value
  @ViewChild('messagesEnd') messagesEnd!: ElementRef;

  private aiService = inject(AiService);

  // Panel state
  isOpen = signal(false);
  isSupported = signal(true);

  // Chat state
  messages = signal<ChatEntry[]>([]);
  userInput = signal('');
  isLoading = signal(false);
  errorMsg = signal('');

  readonly suggestedQuestions = SUGGESTED_QUESTIONS;

  ngOnInit(): void {
    this.checkSupport();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['lessonId'] && !changes['lessonId'].firstChange) {
      // Lesson changed — reset the chat
      this.messages.set([]);
      this.userInput.set('');
      this.isLoading.set(false);
      this.errorMsg.set('');
      this.checkSupport();
    }
  }

  ngAfterViewChecked(): void {
    this.scrollToBottom();
  }

  private checkSupport(): void {
    // ExternalLink = 3
    this.isSupported.set(this.lessonType !== 3);
  }

  togglePanel(): void {
    this.isOpen.update(v => !v);
  }

  askSuggested(question: string): void {
    this.userInput.set(question);
    this.sendMessage();
  }

  sendMessage(): void {
    const question = this.userInput().trim();
    if (!question || this.isLoading()) return;

    // Add user message
    const userMsg: ChatEntry = { role: 'user', content: question };
    const loadingMsg: ChatEntry = { role: 'assistant', content: '', isLoading: true };

    this.messages.update(msgs => [...msgs, userMsg, loadingMsg]);
    this.userInput.set('');
    this.isLoading.set(true);
    this.errorMsg.set('');

    // Build history from all non-loading messages
    const history: AiChatMessage[] = this.messages()
      .filter(m => !m.isLoading)
      .slice(0, -1) // exclude the message we just added
      .map(m => ({ role: m.role, content: m.content }));

    this.aiService.chatWithTutor(this.lessonId, question, history).subscribe({
      next: (res) => {
        this.messages.update(msgs => {
          const updated = [...msgs];
          const loadingIdx = updated.findIndex(m => m.isLoading);
          if (loadingIdx !== -1) {
            updated[loadingIdx] = { role: 'assistant', content: res.answer };
          }
          return updated;
        });
        this.isLoading.set(false);
      },
      error: (err) => {
        this.messages.update(msgs => msgs.filter(m => !m.isLoading));
        this.isLoading.set(false);
        this.errorMsg.set(
          err?.error?.message || 'Something went wrong. Please try again.'
        );
      }
    });
  }

  private scrollToBottom(): void {
    try {
      this.messagesEnd?.nativeElement?.scrollIntoView({ behavior: 'smooth' });
    } catch (_) {}
  }

  onKeyDown(event: KeyboardEvent): void {
    if (event.key === 'Enter' && !event.shiftKey) {
      event.preventDefault();
      this.sendMessage();
    }
  }
}
