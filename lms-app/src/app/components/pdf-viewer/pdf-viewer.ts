import { Component, Input, OnChanges, SimpleChanges, ViewChild, ElementRef, signal, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';

// @ts-ignore
import * as pdfjsLib from 'pdfjs-dist';

// Configure the worker to be loaded from CDN since it's hard to configure in Angular without ejecting or custom builders
pdfjsLib.GlobalWorkerOptions.workerSrc = `https://cdnjs.cloudflare.com/ajax/libs/pdf.js/${pdfjsLib.version}/pdf.worker.min.js`;

@Component({
  selector: 'app-pdf-viewer',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './pdf-viewer.html'
})
export class PdfViewer implements OnChanges {
  @Input() src: string | null = null;
  @ViewChild('pdfCanvas') canvasRef!: ElementRef<HTMLCanvasElement>;

  pdfDoc: any = null;
  pageNum = signal(1);
  numPages = signal(0);
  scale = signal(1.2);
  isLoading = signal(false);
  isFullscreen = signal(false);

  @HostListener('document:fullscreenchange')
  onFullscreenChange() {
    this.isFullscreen.set(!!document.fullscreenElement);
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes['src'] && this.src) {
      this.loadPdf(this.src);
    } else if (changes['src'] && !this.src) {
      this.pdfDoc = null;
      this.numPages.set(0);
      this.pageNum.set(1);
    }
  }

  async loadPdf(url: string) {
    this.isLoading.set(true);
    try {
      this.pdfDoc = await pdfjsLib.getDocument(url).promise;
      this.numPages.set(this.pdfDoc.numPages);
      this.pageNum.set(1);
      
      // Delay slightly to ensure canvas is rendered if it was hidden
      setTimeout(() => {
        this.renderPage(this.pageNum());
      }, 50);
    } catch (error) {
      console.error('Error loading PDF:', error);
    } finally {
      this.isLoading.set(false);
    }
  }

  async renderPage(num: number) {
    if (!this.pdfDoc || !this.canvasRef) return;
    this.isLoading.set(true);

    try {
      const page = await this.pdfDoc.getPage(num);
      
      // We check container width vs viewport to ensure it fits well
      const container = this.canvasRef.nativeElement.parentElement;
      const unscaledViewport = page.getViewport({ scale: 1.0 });
      
      // If scale is 1.2 (default), let's ensure it doesn't vastly exceed container unless user zoomed in manually
      let actualScale = this.scale();
      
      const viewport = page.getViewport({ scale: actualScale });
      const canvas = this.canvasRef.nativeElement;
      const ctx = canvas.getContext('2d');

      if (!ctx) return;

      const outputScale = window.devicePixelRatio || 1;

      canvas.width = Math.floor(viewport.width * outputScale);
      canvas.height = Math.floor(viewport.height * outputScale);
      canvas.style.width = Math.floor(viewport.width) + "px";
      canvas.style.height =  Math.floor(viewport.height) + "px";

      const transform = outputScale !== 1 
        ? [outputScale, 0, 0, outputScale, 0, 0] 
        : null;

      const renderContext: any = {
        canvasContext: ctx,
        transform: transform,
        viewport: viewport
      };

      await page.render(renderContext).promise;
    } catch (error) {
      console.error('Error rendering page:', error);
    } finally {
      this.isLoading.set(false);
    }
  }

  async prevPage() {
    if (this.pageNum() <= 1) return;
    this.pageNum.set(this.pageNum() - 1);
    await this.renderPage(this.pageNum());
  }

  async nextPage() {
    if (this.pageNum() >= this.numPages()) return;
    this.pageNum.set(this.pageNum() + 1);
    await this.renderPage(this.pageNum());
  }

  async zoomIn() {
    this.scale.set(this.scale() + 0.2);
    await this.renderPage(this.pageNum());
  }

  async zoomOut() {
    if (this.scale() <= 0.6) return;
    this.scale.set(this.scale() - 0.2);
    await this.renderPage(this.pageNum());
  }

  toggleFullscreen() {
    const container = this.canvasRef.nativeElement.parentElement?.parentElement;
    if (!container) return;

    if (!document.fullscreenElement) {
      container.requestFullscreen().then(() => {
        this.isFullscreen.set(true);
      }).catch(err => {
        console.error(`Error attempting to enable fullscreen: ${err.message}`);
      });
    } else {
      document.exitFullscreen().then(() => {
        this.isFullscreen.set(false);
      });
    }
  }
}
