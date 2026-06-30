import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-server-timeout',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './server-timeout.html'
})
export class ServerTimeoutPage {
  retry() {
    window.location.reload();
  }
}
