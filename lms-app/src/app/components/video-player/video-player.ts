import { Component, Input, Output, EventEmitter, OnInit, HostListener, ViewChild, ElementRef, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SafeResourceUrl } from '@angular/platform-browser';

@Component({
  selector: 'app-video-player',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './video-player.html'
})
export class VideoPlayer implements OnInit {
  @Input() src: SafeResourceUrl | string | null = null;
  @Input() preventForward: boolean = false;
  @Input() initialTime: number = 0;
  @Input() maxAllowedTime: number = 0;
  @Input() autoplay: boolean = true;
  @Output() timeWatchedUpdate = new EventEmitter<{ currentTime: number, maxTimeWatched: number, duration: number }>();

  @ViewChild('videoPlayer') videoElement!: ElementRef<HTMLVideoElement>;

  // ── Custom Video Player State ─────────────────────────────────────────────
  protected isPlaying      = signal(false);
  protected currentTime    = signal(0);
  protected duration       = signal(0);
  protected volume         = signal(1);
  protected isMuted        = signal(false);
  protected isFullscreen   = signal(false);
  protected showControls   = signal(true);
  private maxTimeWatched   = 0;
  private controlsTimeout: any;
  private isScrubbing = false;
  private currentProgressBar: HTMLElement | null = null;
  private currentVideo: HTMLVideoElement | null = null;

  ngOnInit(): void {
    this.maxTimeWatched = this.maxAllowedTime;
  }

  @HostListener('document:keydown', ['$event'])
  protected handleKeyDown(event: KeyboardEvent): void {
    if (!this.videoElement || !this.src) return;
    const video = this.videoElement.nativeElement;

    // Ignore if typing in input/textarea fields
    const activeEl = document.activeElement;
    if (activeEl && (activeEl.tagName === 'INPUT' || activeEl.tagName === 'TEXTAREA')) {
      return;
    }

    if (event.key === ' ' || event.key.toLowerCase() === 'k') {
      event.preventDefault();
      this.togglePlay(video);
    } else if (event.key === 'ArrowRight' || event.key.toLowerCase() === 'l') {
      event.preventDefault();
      this.seekRelative(video, 5);
    } else if (event.key === 'ArrowLeft' || event.key.toLowerCase() === 'j') {
      event.preventDefault();
      this.seekRelative(video, -5);
    }
  }

  private seekRelative(video: HTMLVideoElement, amount: number): void {
    let targetTime = video.currentTime + amount;
    if (targetTime < 0) targetTime = 0;
    if (targetTime > video.duration) targetTime = video.duration;

    // Prevent scrubbing forward past max watched time if preventForward is true
    if (this.preventForward && amount > 0 && targetTime > this.maxTimeWatched + 0.5) {
      targetTime = this.maxTimeWatched;
    }

    video.currentTime = targetTime;
    this.currentTime.set(targetTime);
    this.onMouseMove(); // Keep controls visible when seeking
  }

  @HostListener('document:fullscreenchange', [])
  protected onFullscreenChange(): void {
    this.isFullscreen.set(!!document.fullscreenElement);
  }

  protected onMouseMove(): void {
    this.showControls.set(true);
    if (this.controlsTimeout) {
      clearTimeout(this.controlsTimeout);
    }
    if (this.isPlaying()) {
      this.controlsTimeout = setTimeout(() => {
        this.showControls.set(false);
      }, 2500);
    }
  }

  protected onMouseLeave(): void {
    if (this.isPlaying()) {
      this.showControls.set(false);
    }
  }

  protected togglePlay(video: HTMLVideoElement): void {
    if (video.paused) {
      video.play().then(() => {
        this.isPlaying.set(true);
        this.onMouseMove();
      }).catch(() => {});
    } else {
      video.pause();
      this.isPlaying.set(false);
      this.showControls.set(true);
      if (this.controlsTimeout) {
        clearTimeout(this.controlsTimeout);
      }
    }
  }

  protected onLoadedMetadata(video: HTMLVideoElement): void {
    this.duration.set(video.duration || 0);
    // Start from the specific initial time
    if (this.initialTime > 0) {
      video.currentTime = this.initialTime;
      this.currentTime.set(this.initialTime);
    }
  }

  protected onVideoEnded(video: HTMLVideoElement): void {
    this.isPlaying.set(false);
  }

  protected onVolumeChange(event: Event, video: HTMLVideoElement): void {
    const target = event.target as HTMLInputElement;
    const val = parseFloat(target.value);
    video.volume = val;
    this.volume.set(val);
    if (val > 0) {
      video.muted = false;
      this.isMuted.set(false);
    }
  }

  protected toggleMute(video: HTMLVideoElement): void {
    const muted = !video.muted;
    video.muted = muted;
    this.isMuted.set(muted);
  }

  protected toggleFullscreen(videoContainer: HTMLElement): void {
    if (!document.fullscreenElement) {
      videoContainer.requestFullscreen().catch(() => {});
    } else {
      document.exitFullscreen().catch(() => {});
    }
  }

  @HostListener('document:mousemove', ['$event'])
  protected onDocumentMouseMove(event: MouseEvent): void {
    if (this.isScrubbing && this.currentProgressBar && this.currentVideo) {
      this.handleScrub(event, this.currentVideo, this.currentProgressBar);
    }
  }

  @HostListener('document:mouseup')
  protected onDocumentMouseUp(): void {
    this.isScrubbing = false;
  }

  protected onScrubStart(event: MouseEvent, video: HTMLVideoElement, progressBar: HTMLElement): void {
    this.isScrubbing = true;
    this.currentVideo = video;
    this.currentProgressBar = progressBar;
    this.handleScrub(event, video, progressBar);
  }

  private handleScrub(event: MouseEvent, video: HTMLVideoElement, progressBar: HTMLElement): void {
    const rect = progressBar.getBoundingClientRect();
    let clickX = event.clientX - rect.left;
    if (clickX < 0) clickX = 0;
    if (clickX > rect.width) clickX = rect.width;

    const width = rect.width;
    let targetTime = (clickX / width) * video.duration;

    if (targetTime < 0) targetTime = 0;
    if (targetTime > video.duration) targetTime = video.duration;

    // Prevent scrubbing forward past max watched time if preventForward is true
    if (this.preventForward && targetTime > this.maxTimeWatched + 0.5) {
      targetTime = this.maxTimeWatched;
    }

    video.currentTime = targetTime;
    this.currentTime.set(targetTime);
  }

  protected formatTime(secs: number): string {
    if (isNaN(secs)) return '0:00';
    const minutes = Math.floor(secs / 60);
    const seconds = Math.floor(secs % 60);
    return `${minutes}:${seconds < 10 ? '0' : ''}${seconds}`;
  }

  protected onTimeUpdate(video: HTMLVideoElement): void {
    this.currentTime.set(video.currentTime);
    if (video.currentTime > this.maxTimeWatched) {
      this.maxTimeWatched = video.currentTime;
    }
    this.timeWatchedUpdate.emit({ currentTime: video.currentTime, maxTimeWatched: this.maxTimeWatched, duration: video.duration });
  }
}
