import { Component, signal, inject, OnInit } from '@angular/core';
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
export class App implements OnInit {
  protected readonly title = signal('lms-app');
  private authService = inject(AuthService);
  protected isAuthenticating = this.authService.isAuthenticating;

  ngOnInit() {
    this.authService.initializeAuth().subscribe();
  }
}
