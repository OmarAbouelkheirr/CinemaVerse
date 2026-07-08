import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';

import { UserHeader } from '../../features/user/user-mangment/layout/user-header/user-header';
import { UserFooter } from '../../features/user/user-mangment/layout/user-footer/user-footer';

// AppShellComponent is the single root layout for all authenticated user pages.
//
// Architecture:
// - Header and Footer are rendered ONCE at this level
// - All feature pages (Home, Movies, Movie Details, Bookings, etc.)
//   render inside the router-outlet between Header and Footer
// - This prevents duplicate Header/Footer across features
// - Feature layouts (like MovieBookingLayout) should NOT include Header/Footer
//
// Hierarchy:
//   AppShellComponent
//     ├── UserHeader
//     ├── <router-outlet> ← Feature pages render here
//     └── UserFooter
@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [RouterOutlet, UserHeader, UserFooter],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <!-- Header is rendered once and remains visible across all feature pages -->
    <app-user-header />

    <!-- Feature pages render inside this outlet -->
    <main class="app-shell__content">
      <router-outlet />
    </main>

    <!-- Footer is rendered once and remains visible across all feature pages -->
    <app-user-footer />
  `,
  styles: [`
    :host {
      display: flex;
      flex-direction: column;
      min-height: 100vh;
    }
    .app-shell__content {
      flex: 1;
      display: flex;
      flex-direction: column;
    }
  `],
})
export class AppShellComponent {}
