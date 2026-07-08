import { Component, inject } from '@angular/core';
import { Router, RouterLink } from '@angular/router';

@Component({
  selector: 'app-user-header',
  imports: [RouterLink],
  templateUrl: './user-header.html',
  styleUrl: './user-header.scss',
})
export class UserHeader {
  private readonly router = inject(Router);

  goToProfile(): void {
    void this.router.navigate(['/user/profile']);
  }
}
