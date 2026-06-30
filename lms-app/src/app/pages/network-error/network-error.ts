import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-network-error',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './network-error.html'
})
export class NetworkErrorPage {
  retry() {
    window.location.reload();
  }
}
