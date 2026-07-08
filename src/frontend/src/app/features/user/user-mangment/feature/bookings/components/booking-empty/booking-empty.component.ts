import { ChangeDetectionStrategy, Component, output } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-booking-empty',
  standalone: true,
  imports: [RouterLink],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="empty">
      <span class="material-symbols-outlined empty__icon">confirmation_number</span>
      <h3 class="empty__title">No Bookings Yet</h3>
      <p class="empty__description">You haven't made any cinema bookings. Browse our movies and book your first show!</p>
      <a routerLink="/movies" class="btn btn-primary btn-md empty__cta" aria-label="Browse movies to book tickets">
        <span class="material-symbols-outlined">movie</span>
        Browse Movies
      </a>
    </div>
  `,
  styles: [`
    .empty {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      text-align: center;
      padding: clamp(3rem, 8vw, 6rem) 2rem;
      gap: 1rem;
    }

    .empty__icon {
      font-size: 4rem;
      color: var(--on-surface-muted);
      opacity: 0.5;
    }

    .empty__title {
      font-size: var(--text-subtitle);
      font-weight: 700;
      color: var(--on-surface);
      margin: 0;
    }

    .empty__description {
      font-size: var(--text-body);
      color: var(--on-surface-variant);
      margin: 0;
      max-width: 28rem;
      line-height: 1.6;
    }

    .empty__cta {
      margin-top: 0.5rem;
      text-decoration: none;
    }
  `],
})
export class BookingEmptyComponent {}
