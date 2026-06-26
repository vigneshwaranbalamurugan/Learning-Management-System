import { Injectable, OnDestroy, NgZone, inject } from '@angular/core';
import {
  HubConnection,
  HubConnectionBuilder,
  HubConnectionState,
  LogLevel,
} from '@microsoft/signalr';
import { BehaviorSubject, Subject } from 'rxjs';
import { environment } from '@environments/environment';
import { LessonProgressResponse } from '@models/progress';

export interface ResumePositionEvent {
  lessonId: number;
  lastWatchedSecond: number;
  maxWatchedSecond: number;
  watchPercentage: number;
  isCompleted: boolean;
}

export interface LessonCompletedEvent {
  lessonId: number;
  completedAt: string;
  watchPercentage: number;
}

@Injectable({
  providedIn: 'root',
})
export class VideoProgressSignalRService implements OnDestroy {
  private hubConnection: HubConnection | null = null;
  private zone = inject(NgZone);

  /** Emits the last watched position for a video. */
  resumePosition$ = new Subject<ResumePositionEvent>();
  
  /** Emits progress updates saved by the server. */
  progressUpdated$ = new Subject<LessonProgressResponse>();
  
  /** Emits when the lesson is fully watched (auto-completed). */
  lessonCompleted$ = new Subject<LessonCompletedEvent>();

  /** Connection state for optional UI indicators. */
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
        .withUrl(`${environment.hubUrl}/video-progress`, { withCredentials: true })
        .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
        .configureLogging(environment.production ? LogLevel.Warning : LogLevel.Debug)
        .build()
    );

    this.registerHandlers();

    this.hubConnection.onreconnecting((err) => {
      this.zone.run(() => {
        console.warn('[VideoProgressSignalR] Reconnecting...', err);
        this.connectionState$.next(HubConnectionState.Reconnecting);
      });
    });

    this.hubConnection.onreconnected((connectionId) => {
      this.zone.run(() => {
        console.log('[VideoProgressSignalR] Reconnected. ConnectionId:', connectionId);
        this.connectionState$.next(HubConnectionState.Connected);
      });
    });

    this.hubConnection.onclose((err) => {
      this.zone.run(() => {
        console.warn('[VideoProgressSignalR] Connection closed.', err);
        this.connectionState$.next(HubConnectionState.Disconnected);
      });
    });

    try {
      await this.hubConnection.start();
      this.zone.run(() => {
        this.connectionState$.next(HubConnectionState.Connected);
        console.log('[VideoProgressSignalR] Connected. State:', this.hubConnection?.state);
      });
    } catch (err) {
      this.zone.run(() => {
        console.error('[VideoProgressSignalR] Connection failed:', err);
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
        console.log('[VideoProgressSignalR] Disconnected.');
      });
    }
  }

  async getResumePosition(lessonId: number): Promise<void> {
    if (this.hubConnection?.state === HubConnectionState.Connected) {
      try {
        await this.hubConnection.invoke('GetResumePosition', lessonId);
      } catch (err) {
        console.error('[VideoProgressSignalR] GetResumePosition failed:', err);
      }
    }
  }

  async updateProgress(lessonId: number, lastWatchedSecond: number, maxWatchedSecond: number, totalSeconds: number): Promise<void> {
    if (this.hubConnection?.state === HubConnectionState.Connected) {
      try {
        await this.hubConnection.invoke('UpdateProgress', { lessonId, lastWatchedSecond, maxWatchedSecond, totalSeconds });
      } catch (err) {
        console.error('[VideoProgressSignalR] UpdateProgress failed:', err);
      }
    }
  }

  private registerHandlers(): void {
    if (!this.hubConnection) return;

    this.hubConnection.on('ResumePosition', (data: ResumePositionEvent) => {
      this.zone.run(() => {
        this.resumePosition$.next(data);
      });
    });

    this.hubConnection.on('ProgressUpdated', (data: LessonProgressResponse) => {
      this.zone.run(() => {
        this.progressUpdated$.next(data);
      });
    });

    this.hubConnection.on('LessonCompleted', (data: LessonCompletedEvent) => {
      this.zone.run(() => {
        this.lessonCompleted$.next(data);
      });
    });

    this.hubConnection.on('Error', (error: { message: string }) => {
      this.zone.run(() => {
        console.error('[VideoProgressSignalR] Server error:', error.message);
      });
    });
  }

  ngOnDestroy(): void {
    this.disconnect();
  }
}
