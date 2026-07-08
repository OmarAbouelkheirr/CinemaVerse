import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';

import { Seat, ShowtimeInfo } from '../../interfaces/seat.interface';

@Component({
  selector: 'app-booking-summary',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="summary">
      <h3 class="summary__title">Booking Summary</h3>

      @if (showtime(); as st) {
        <div class="summary__row">
          <span class="summary__label">Cinema</span>
          <span class="summary__value">{{ st.branchName }}</span>
        </div>

        <div class="summary__row">
          <span class="summary__label">Hall</span>
          <span class="summary__value">{{ st.hallNumber }} &middot; {{ st.hallType }}</span>
        </div>

        <div class="summary__row">
          <span class="summary__label">Date</span>
          <span class="summary__value">{{ formatDate(st.showStartTime) }}</span>
        </div>

        <div class="summary__row">
          <span class="summary__label">Time</span>
          <span class="summary__value">{{ formatTime(st.showStartTime) }}</span>
        </div>
      }

      <div class="summary__divider"></div>

      <div class="summary__row">
        <span class="summary__label">Selected Seats</span>
        <span class="summary__value summary__value--seats">
          @if (selectedSeats().length > 0) {
            {{ seatLabels() }}
          } @else {
            None
          }
        </span>
      </div>

      <div class="summary__row">
        <span class="summary__label">Tickets</span>
        <span class="summary__value">{{ selectedSeats().length }}</span>
      </div>

      <div class="summary__row">
        <span class="summary__label">Price</span>
        <span class="summary__value">{{ ticketPrice() }} EGP</span>
      </div>

      <div class="summary__divider"></div>

      <div class="summary__row summary__row--total">
        <span class="summary__label">Subtotal</span>
        <span class="summary__value summary__value--highlight">{{ subtotal() }} EGP</span>
      </div>
    </div>
  `,
  styles: [
    `
      .summary {
        display: flex;
        flex-direction: column;
        gap: 0.75rem;
        padding: 1.25rem;
        background: var(--surface-container-low);
        border: 1px solid var(--ghost-border);
        border-radius: var(--radius-lg);
      }

      .summary__title {
        font-size: var(--text-title);
        font-weight: 700;
        color: var(--on-surface);
        margin: 0 0 0.25rem 0;
      }

      .summary__row {
        display: flex;
        justify-content: space-between;
        align-items: baseline;
        gap: 1rem;
      }

      .summary__label {
        font-size: var(--text-body-sm);
        font-weight: 500;
        color: var(--on-surface-muted);
        flex-shrink: 0;
      }

      .summary__value {
        font-size: var(--text-body);
        font-weight: 600;
        color: var(--on-surface);
        text-align: right;
      }

      .summary__value--seats {
        max-width: 12rem;
        word-break: break-word;
      }

      .summary__value--highlight {
        font-size: var(--text-title);
        color: var(--primary-container);
      }

      .summary__row--total {
        padding-top: 0.25rem;
      }

      .summary__divider {
        height: 1px;
        background: var(--ghost-border);
        margin: 0.25rem 0;
      }
    `,
  ],
})
export class BookingSummaryComponent {
  readonly selectedSeats = input.required<Seat[]>();
  readonly ticketPrice = input.required<number>();
  readonly showtime = input<ShowtimeInfo | null>(null);

  readonly subtotal = computed(() => this.selectedSeats().length * this.ticketPrice());

  readonly seatLabels = computed(() =>
    this.selectedSeats()
      .map((s) => s.seatLabel)
      .join(', ')
  );

  formatDate(isoString: string): string {
    const date = new Date(isoString);
    return date.toLocaleDateString('en-GB', {
      weekday: 'short',
      day: 'numeric',
      month: 'short',
      year: 'numeric',
    });
  }

  formatTime(isoString: string): string {
    const date = new Date(isoString);
    return date.toLocaleTimeString('en-GB', {
      hour: '2-digit',
      minute: '2-digit',
    });
  }
}
