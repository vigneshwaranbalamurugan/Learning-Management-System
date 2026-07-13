import { Injectable, OnDestroy, NgZone, inject } from '@angular/core';
import {
  HubConnection,
  HubConnectionBuilder,
  HubConnectionState,
  LogLevel,
} from '@microsoft/signalr';
import { BehaviorSubject, Subject } from 'rxjs';
import { environment } from '@environments/environment';

export interface AnswerSavedEvent {
  questionId: number;
  selectedOptionId: number;
}

@Injectable({
  providedIn: 'root',
})
export class QuizProgressSignalRService implements OnDestroy {
  private hubConnection: HubConnection | null = null;
  private zone = inject(NgZone);
  
  answerSaved$ = new Subject<AnswerSavedEvent>();
  connectionState$ = new BehaviorSubject<HubConnectionState>(HubConnectionState.Disconnected);

  async connect(): Promise<void> {
    if (
      this.hubConnection?.state === HubConnectionState.Connected ||
      this.hubConnection?.state === HubConnectionState.Connecting ||
      this.hubConnection?.state === HubConnectionState.Reconnecting
    ) {
      return;
    }

    if (this.hubConnection) {
      try { await this.hubConnection.stop(); } catch { /* ignore */ }
      this.hubConnection = null;
    }

    this.hubConnection = this.zone.runOutsideAngular(() =>
      new HubConnectionBuilder()
        .withUrl(`${environment.hubUrl}/quiz-progress`, { withCredentials: true })
        .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
        .configureLogging(environment.production ? LogLevel.Warning : LogLevel.Debug)
        .build()
    );

    this.registerHandlers();

    this.hubConnection.onreconnecting((err) => {
      this.zone.run(() => {
        console.warn('[QuizProgressSignalR] Reconnecting...', err);
        this.connectionState$.next(HubConnectionState.Reconnecting);
      });
    });

    this.hubConnection.onreconnected((connectionId) => {
      this.zone.run(() => {
        console.log('[QuizProgressSignalR] Reconnected. ConnectionId:', connectionId);
        this.connectionState$.next(HubConnectionState.Connected);
      });
    });

    this.hubConnection.onclose((err) => {
      this.zone.run(() => {
        console.warn('[QuizProgressSignalR] Connection closed.', err);
        this.connectionState$.next(HubConnectionState.Disconnected);
      });
    });

    try {
      await this.hubConnection.start();
      this.zone.run(() => {
        this.connectionState$.next(HubConnectionState.Connected);
        console.log('[QuizProgressSignalR] Connected. State:', this.hubConnection?.state);
      });
    } catch (err) {
      this.zone.run(() => {
        console.error('[QuizProgressSignalR] Connection failed:', err);
        this.connectionState$.next(HubConnectionState.Disconnected);
      });
    }
  }

  async disconnect(): Promise<void> {
    if (this.hubConnection) {
      try { await this.hubConnection.stop(); } catch { /* ignore */ }
      this.hubConnection = null;
      this.zone.run(() => {
        this.connectionState$.next(HubConnectionState.Disconnected);
        console.log('[QuizProgressSignalR] Disconnected.');
      });
    }
  }

  async updateAnswer(attemptId: number, questionId: number, selectedOptionId: number): Promise<void> {
    if (this.hubConnection?.state === HubConnectionState.Connected) {
      try {
        await this.hubConnection.invoke('SavePartialAnswer', attemptId, questionId, selectedOptionId);
      } catch (err) {
        console.error('[QuizProgressSignalR] SavePartialAnswer failed:', err);
      }
    }
  }

  private registerHandlers(): void {
    if (!this.hubConnection) return;

    this.hubConnection.on('AnswerSaved', (data: AnswerSavedEvent) => {
      this.zone.run(() => {
        this.answerSaved$.next(data);
      });
    });

    this.hubConnection.on('Error', (error: { message: string }) => {
      this.zone.run(() => {
        console.error('[QuizProgressSignalR] Server error:', error.message);
      });
    });
  }

  ngOnDestroy(): void {
    this.disconnect();
  }
}
