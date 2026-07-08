// BookingSummaryComponent — displays a detailed summary of a booking including movie, cinema, hall, seats, amount, date, and time
import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';

import { API_BASE_URL } from '../../../../../../../core/config/api.config';
import { IBooking } from '../../interfaces/booking.interface';

@Component({
  selector: 'app-booking-summary',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <article class="summary-card" aria-label="Booking summary">
      <header class="summary-card__header">
        <h2>Booking Summary</h2>
      </header>

      <div class="summary-card__poster-wrap">
        <img
          class="summary-card__poster"
          [src]="posterUrl()"
          [alt]="booking().showtime.movieTitle"
          loading="lazy"
        />
      </div>

      <div class="summary-card__details">
        <div class="summary-item">
          <span class="summary-item__label">Movie Name</span>
          <strong class="summary-item__value">{{ booking().showtime.movieTitle }}</strong>
        </div>

        <div class="summary-item">
          <span class="summary-item__label">Cinema</span>
          <span class="summary-item__value">{{ cinemaName() }}</span>
        </div>

        <div class="summary-item">
          <span class="summary-item__label">Branch</span>
          <span class="summary-item__value">{{ branchName() }}</span>
        </div>

        <div class="summary-item">
          <span class="summary-item__label">Hall</span>
          <span class="summary-item__value">{{ hallName() }}</span>
        </div>

        <div class="summary-item">
          <span class="summary-item__label">Show Date</span>
          <span class="summary-item__value">{{ showDate() }}</span>
        </div>

        <div class="summary-item">
          <span class="summary-item__label">Show Time</span>
          <span class="summary-item__value">{{ showTime() }}</span>
        </div>

        <div class="summary-item">
          <span class="summary-item__label">Seats</span>
          <span class="summary-item__value">{{ seatsLabel() }}</span>
        </div>

        <div class="summary-item summary-item--total">
          <span class="summary-item__label">Total Paid</span>
          <strong class="summary-item__value">{{ booking().totalAmount }} EGP</strong>
        </div>
      </div>
    </article>
  `,
  styles: [
    `
      .summary-card {
        background: var(--surface-container-low);
        border: 1px solid var(--ghost-border);
        border-radius: var(--radius-lg);
        padding: clamp(1rem, 2.2vw, 1.25rem);
        display: flex;
        flex-direction: column;
        gap: 1rem;
      }

      .summary-card__header h2 {
        margin: 0;
        font-size: var(--text-title-sm);
        color: var(--on-surface);
      }

      .summary-card__poster-wrap {
        border-radius: var(--radius-md);
        overflow: hidden;
        border: 1px solid var(--ghost-border);
        background: var(--surface-container);
      }

      .summary-card__poster {
        width: 100%;
        display: block;
        aspect-ratio: 16 / 9;
        object-fit: cover;
      }

      .summary-card__details {
        display: grid;
        gap: 0.5rem;
      }

      .summary-item {
        display: flex;
        align-items: baseline;
        justify-content: space-between;
        gap: 1rem;
        border-bottom: 1px solid var(--ghost-border);
        padding-bottom: 0.5rem;
      }

      .summary-item__label {
        font-size: var(--text-body-sm);
        color: var(--on-surface-variant);
      }

      .summary-item__value {
        font-size: var(--text-body);
        color: var(--on-surface);
        text-align: right;
        overflow-wrap: anywhere;
      }

      .summary-item--total {
        border-bottom: 0;
        padding-bottom: 0;
        margin-top: 0.25rem;
      }

      .summary-item--total .summary-item__value {
        color: var(--primary-container);
        font-size: var(--text-title-sm);
      }

      @media (max-width: 480px) {
        .summary-item {
          flex-direction: column;
          align-items: flex-start;
          gap: 0.25rem;
        }

        .summary-item__value {
          text-align: left;
        }
      }
    `,
  ],
})
export class BookingSummaryComponent {
  readonly booking = input.required<IBooking>();

  readonly primaryTicket = computed(() => this.booking().tickets[0] ?? null);

  readonly posterUrl = computed(() => {
    const url = this.booking().showtime.posterUrl;
    if (url.startsWith('http://') || url.startsWith('https://')) {
      return url;
    }
    return `${API_BASE_URL}${url}`;
  });

  readonly branchName = computed(() => this.primaryTicket()?.branchName ?? '—');
  readonly cinemaName = computed(() => this.primaryTicket()?.branchName ?? '—');
  readonly hallName = computed(() => this.primaryTicket()?.hallNumber ?? '—');

  readonly showDate = computed(() => {
    const date = new Date(this.booking().showtime.startTime);
    return date.toLocaleDateString('en-GB', {
      weekday: 'short',
      day: '2-digit',
      month: 'short',
      year: 'numeric',
    });
  });

  readonly showTime = computed(() => {
    const date = new Date(this.booking().showtime.startTime);
    return date.toLocaleTimeString('en-GB', {
      hour: '2-digit',
      minute: '2-digit',
    });
  });

  readonly seatsLabel = computed(() =>
    this.booking()
      .bookedSeats.map((seat) => seat.seatLabel)
      .join(', '),
  );
}
