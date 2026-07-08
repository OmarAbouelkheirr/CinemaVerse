import { ChangeDetectionStrategy, Component, computed, inject, input, output } from '@angular/core';

import { API_BASE_URL } from '../../../../../../../core/config/api.config';
import { IBooking } from '../../interfaces/booking.interface';
import { BookingStatusBadgeComponent } from '../booking-status-badge/booking-status-badge.component';

@Component({
  selector: 'app-booking-card',
  standalone: true,
  imports: [BookingStatusBadgeComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <article class="card">
      <div class="card__poster">
        <img
          [src]="posterUrl()"
          [alt]="booking().showtime.movieTitle"
          class="card__image"
          loading="lazy"
        />
        <div class="card__status">
          <app-booking-status-badge [status]="booking().status" />
        </div>
      </div>

      <div class="card__body">
        <h3 class="card__title">{{ booking().showtime.movieTitle }}</h3>

        <div class="card__meta">
          <span class="card__meta-item">
            <span class="material-symbols-outlined card__meta-icon">calendar_today</span>
            {{ formattedDate() }}
          </span>
          <span class="card__meta-item">
            <span class="material-symbols-outlined card__meta-icon">schedule</span>
            {{ formattedTime() }}
          </span>
        </div>

        <div class="card__meta">
          <span class="card__meta-item">
            <span class="material-symbols-outlined card__meta-icon">event_seat</span>
            {{ seatLabels() }}
          </span>
        </div>

        <div class="card__footer">
          <span class="card__price">{{ booking().totalAmount }} EGP</span>
          <div class="card__actions">
            <button
              type="button"
              class="btn btn-secondary btn-sm"
              (click)="viewDetails.emit(booking().bookingId)"
              aria-label="View booking details"
            >
              View Details
            </button>
            @if (isCancellable()) {
              <button
                type="button"
                class="btn btn-danger btn-sm"
                (click)="cancelBooking.emit(booking().bookingId)"
                aria-label="Cancel this booking"
              >
                Cancel
              </button>
            }
          </div>
        </div>
      </div>
    </article>
  `,
  styles: [`
    .card {
      display: flex;
      flex-direction: column;
      background: var(--surface-container-low);
      border: 1px solid var(--ghost-border);
      border-radius: var(--radius-lg);
      overflow: hidden;
      box-shadow: var(--shadow-card);
      transition: transform 0.3s ease, box-shadow 0.3s ease;
    }

    .card:hover {
      transform: translateY(-0.25rem);
      box-shadow: var(--shadow-float);
    }

    .card__poster {
      position: relative;
      aspect-ratio: 16 / 9;
      overflow: hidden;
      background: var(--surface-container);
    }

    .card__image {
      width: 100%;
      height: 100%;
      object-fit: cover;
      transition: transform 0.4s ease;
    }

    .card:hover .card__image {
      transform: scale(1.05);
    }

    .card__status {
      position: absolute;
      top: 0.75rem;
      right: 0.75rem;
      z-index: 2;
    }

    .card__body {
      display: flex;
      flex-direction: column;
      gap: 0.75rem;
      padding: 1rem 1.25rem 1.25rem;
      flex: 1;
    }

    .card__title {
      font-size: var(--text-title);
      font-weight: 700;
      color: var(--on-surface);
      margin: 0;
      line-height: 1.3;
      display: -webkit-box;
      -webkit-line-clamp: 2;
      -webkit-box-orient: vertical;
      overflow: hidden;
    }

    .card__meta {
      display: flex;
      flex-wrap: wrap;
      gap: 0.75rem;
    }

    .card__meta-item {
      display: inline-flex;
      align-items: center;
      gap: 0.25rem;
      font-size: var(--text-body-sm);
      color: var(--on-surface-variant);
    }

    .card__meta-icon {
      font-size: 0.875rem;
      color: var(--on-surface-muted);
    }

    .card__footer {
      display: flex;
      align-items: center;
      justify-content: space-between;
      gap: 0.75rem;
      margin-top: auto;
      padding-top: 0.75rem;
      border-top: 1px solid var(--ghost-border);
    }

    .card__price {
      font-size: var(--text-title-sm);
      font-weight: 700;
      color: var(--primary-container);
    }

    .card__actions {
      display: flex;
      gap: 0.5rem;
    }

    @media (max-width: 480px) {
      .card__body {
        padding: 0.75rem 1rem 1rem;
      }

      .card__footer {
        flex-direction: column;
        align-items: stretch;
        gap: 0.5rem;
      }

      .card__actions {
        justify-content: stretch;
      }

      .card__actions .btn {
        flex: 1;
        justify-content: center;
      }
    }
  `],
})
export class BookingCardComponent {
  readonly booking = input.required<IBooking>();
  readonly viewDetails = output<number>();
  readonly cancelBooking = output<number>();

  readonly isCancellable = computed(() => {
    const status = this.booking().status;
    return status === 'Pending' || status === 'Confirmed';
  });

  readonly seatLabels = computed(() =>
    this.booking().bookedSeats.map(s => s.seatLabel).join(', ')
  );

  readonly posterUrl = computed(() => {
    const url = this.booking().showtime.posterUrl;
    if (url.startsWith('http://') || url.startsWith('https://')) {
      return url;
    }
    return `${API_BASE_URL}${url}`;
  });

  readonly formattedDate = computed(() => {
    const date = new Date(this.booking().showtime.startTime);
    return date.toLocaleDateString('en-GB', {
      weekday: 'short',
      day: 'numeric',
      month: 'short',
    });
  });

  readonly formattedTime = computed(() => {
    const date = new Date(this.booking().showtime.startTime);
    return date.toLocaleTimeString('en-GB', {
      hour: '2-digit',
      minute: '2-digit',
    });
  });
}
