import { Component, signal, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { Toaster } from '@components/toaster/toaster';
import { AuthService } from '@services/auth.service';
import { Loader } from '@components/loader/loader';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, Toaster, Loader],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  protected readonly title = signal('lms-app');
  private authService = inject(AuthService);
  // Shows the loading overlay when auth.guard or guest.guard triggers initializeAuth()
  protected isAuthenticating = this.authService.isAuthenticating;
}
