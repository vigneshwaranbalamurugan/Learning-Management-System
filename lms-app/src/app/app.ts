import { Component, signal, inject, DestroyRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { 
  RouterOutlet, 
  Router, 
  RouteConfigLoadStart, 
  RouteConfigLoadEnd, 
  NavigationStart, 
  NavigationEnd, 
  NavigationCancel, 
  NavigationError 
} from '@angular/router';
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
  private router = inject(Router);
  private destroyRef = inject(DestroyRef);

  // Shows the loading overlay when auth.guard or guest.guard triggers initializeAuth()
  protected isAuthenticating = this.authService.isAuthenticating;
  protected isRouteLoading = signal(false);

  constructor() {
    this.router.events.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(event => {
      if (event instanceof RouteConfigLoadStart || event instanceof NavigationStart) {
        this.isRouteLoading.set(true);
      } else if (
        event instanceof RouteConfigLoadEnd ||
        event instanceof NavigationEnd ||
        event instanceof NavigationCancel ||
        event instanceof NavigationError
      ) {
        this.isRouteLoading.set(false);
      }
    });
  }
}
