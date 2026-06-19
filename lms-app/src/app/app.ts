import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { Toaster } from './components/toaster/toaster';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, Toaster],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  protected readonly title = signal('lms-app');
}
