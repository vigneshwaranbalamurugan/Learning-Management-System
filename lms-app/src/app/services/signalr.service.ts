import { Injectable, OnDestroy, NgZone, inject } from '@angular/core';
import {
  HubConnection,
  HubConnectionBuilder,
  HubConnectionState,
  LogLevel,
} from '@microsoft/signalr';
import { BehaviorSubject, Subject } from 'rxjs';
import { environment } from '@environments/environment';
import { Notification } from '@models/notification';

@Injectable({
  providedIn: 'root',
})
export class SignalRService implements OnDestroy {
  private hubConnection: HubConnection | null = null;

  /**
   * NgZone is required because SignalR's on() callbacks run outside Angular's
   * zone. Without zone.run(), signal/template updates are invisible to the
   * Angular change-detection cycle — causing the popup and badge to never render.
   */
  private zone = inject(NgZone);

  /** Emits each incoming notification pushed by the server. */
  notification$ = new Subject<Notification>();

  /** Current unread badge count — initialised at 0, updated by server events. */
  unreadCount$ = new BehaviorSubject<number>(0);

  /** Connection state for optional UI indicators. */
  connectionState$ = new BehaviorSubject<HubConnectionState>(HubConnectionState.Disconnected);

  // ─── Public API ────────────────────────────────────────────────────────────

  /**
   * Builds and starts the SignalR connection to /hubs/notification.
   * Uses cookie-based auth (withCredentials) — no token in the query string.
   * Safe to call multiple times; no-ops if already connected or connecting.
   */
  async connect(): Promise<void> {
    // Already in a live state — nothing to do
    if (
      this.hubConnection?.state === HubConnectionState.Connected ||
      this.hubConnection?.state === HubConnectionState.Connecting ||
      this.hubConnection?.state === HubConnectionState.Reconnecting
    ) {
      return;
    }

    // Tear down any stale connection before creating a new one
    if (this.hubConnection) {
      try { await this.hubConnection.stop(); } catch { /* ignore */ }
      this.hubConnection = null;
    }

    // Build outside the zone — SignalR internals don't need zone tracking
    this.hubConnection = this.zone.runOutsideAngular(() =>
      new HubConnectionBuilder()
        .withUrl(`${environment.hubUrl}/notification`, { withCredentials: true })
        .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
        .configureLogging(environment.production ? LogLevel.Warning : LogLevel.Debug)
        .build()
    );

    // Register handlers BEFORE start() so no events are missed
    this.registerHandlers();

    this.hubConnection.onreconnecting((err) => {
      this.zone.run(() => {
        console.warn('[SignalR] Reconnecting...', err);
        this.connectionState$.next(HubConnectionState.Reconnecting);
      });
    });

    this.hubConnection.onreconnected((connectionId) => {
      this.zone.run(() => {
        console.log('[SignalR] Reconnected. ConnectionId:', connectionId);
        this.connectionState$.next(HubConnectionState.Connected);
      });
    });

    this.hubConnection.onclose((err) => {
      this.zone.run(() => {
        console.warn('[SignalR] Connection closed.', err);
        this.connectionState$.next(HubConnectionState.Disconnected);
      });
    });

    try {
      await this.hubConnection.start();
      this.zone.run(() => {
        this.connectionState$.next(HubConnectionState.Connected);
        console.log('[SignalR] Connected to NotificationHub. State:', this.hubConnection?.state);
      });
    } catch (err) {
      this.zone.run(() => {
        console.error('[SignalR] Connection failed:', err);
        this.connectionState$.next(HubConnectionState.Disconnected);
      });
    }
  }

  /** Cleanly stops the hub connection (e.g. on logout). */
  async disconnect(): Promise<void> {
    if (this.hubConnection) {
      try { await this.hubConnection.stop(); } catch { /* ignore */ }
      this.hubConnection = null;
      this.zone.run(() => {
        this.connectionState$.next(HubConnectionState.Disconnected);
        console.log('[SignalR] Disconnected from NotificationHub.');
      });
    }
  }

  /**
   * Invokes MarkAsRead on the hub over WebSocket.
   * Falls back silently if disconnected (REST is handled by NotificationPanel).
   */
  async markAsRead(notificationId: number): Promise<void> {
    if (this.hubConnection?.state === HubConnectionState.Connected) {
      try {
        await this.hubConnection.invoke('MarkAsRead', notificationId);
      } catch (err) {
        console.error('[SignalR] MarkAsRead failed:', err);
      }
    }
  }

  /** Invokes MarkAllAsRead on the hub. */
  async markAllAsRead(): Promise<void> {
    if (this.hubConnection?.state === HubConnectionState.Connected) {
      try {
        await this.hubConnection.invoke('MarkAllAsRead');
      } catch (err) {
        console.error('[SignalR] MarkAllAsRead failed:', err);
      }
    }
  }

  // ─── Private helpers ───────────────────────────────────────────────────────

  private registerHandlers(): void {
    if (!this.hubConnection) return;

    /**
     * ⚠️  All hubConnection.on() callbacks run OUTSIDE Angular's NgZone.
     * Wrapping each in zone.run() is MANDATORY so that:
     *   – signal() writes trigger template re-renders (badge count, popup list)
     *   – BehaviorSubject / Subject emissions schedule Angular's change detection
     */
    this.hubConnection.on('ReceiveNotification', (notification: Notification) => {
      this.zone.run(() => {
        console.log('[SignalR] ReceiveNotification →', notification);
        this.notification$.next(notification);
      });
    });

    this.hubConnection.on('UpdateUnreadCount', (count: number) => {
      this.zone.run(() => {
        console.log('[SignalR] UpdateUnreadCount →', count);
        this.unreadCount$.next(count);
      });
    });

    this.hubConnection.on('Error', (error: { message: string }) => {
      this.zone.run(() => {
        console.error('[SignalR] Server error:', error.message);
      });
    });
  }

  ngOnDestroy(): void {
    this.disconnect();
  }
}
