// BookingTicketCardComponent — displays a single ticket within a booking with ticket number, seat, status, and a view ticket action
import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';

import { ITicket } from '../../interfaces/ticket.interface';

@Component({
  selector: 'app-booking-ticket-card',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <article class="ticket-card" [attr.aria-label]="'Ticket ' + ticket().ticketNumber">
      <div class="ticket-card__head">
        <h3>Ticket #{{ ticket().ticketNumber }}</h3>
        @switch (ticket().status) {
          @case ('Active') {
            <span class="ticket-status ticket-status--active">{{ ticket().status }}</span>
          }
          @case ('Used') {
            <span class="ticket-status ticket-status--used">{{ ticket().status }}</span>
          }
          @case ('Cancelled') {
            <span class="ticket-status ticket-status--cancelled">{{ ticket().status }}</span>
          }
        }
      </div>

      <p class="ticket-card__seat">
        <span class="ticket-card__label">Seat</span>
        <strong>{{ ticket().seatLabel }}</strong>
      </p>

      <button
        type="button"
        class="btn btn-secondary btn-sm"
        (click)="viewTicket.emit(ticket().ticketId)"
        aria-label="View ticket details"
      >
        View Ticket
      </button>
    </article>
  `,
  styles: [
    `
      .ticket-card {
        border: 1px solid var(--ghost-border);
        border-radius: var(--radius-md);
        background: var(--surface-container);
        padding: 0.875rem;
        display: flex;
        flex-direction: column;
        gap: 0.75rem;
      }

      .ticket-card__head {
        display: flex;
        align-items: center;
        justify-content: space-between;
        gap: 0.75rem;
        flex-wrap: wrap;
      }

      .ticket-card__head h3 {
        margin: 0;
        font-size: var(--text-body);
        color: var(--on-surface);
      }

      .ticket-card__seat {
        margin: 0;
        display: flex;
        align-items: baseline;
        justify-content: space-between;
        gap: 0.75rem;
        color: var(--on-surface);
      }

      .ticket-card__label {
        color: var(--on-surface-variant);
        font-size: var(--text-body-sm);
      }

      .ticket-status {
        display: inline-flex;
        align-items: center;
        justify-content: center;
        border-radius: 999px;
        padding: 0.2rem 0.6rem;
        border: 1px solid transparent;
        font-size: var(--text-label);
        font-weight: 600;
        letter-spacing: 0.02em;
        text-transform: uppercase;
      }

      .ticket-status--active {
        color: var(--status-active);
        background: var(--status-active-bg);
        border-color: var(--status-active-br);
      }

      .ticket-status--used {
        color: var(--status-suspended);
        background: var(--status-suspended-bg);
        border-color: var(--status-suspended-br);
      }

      .ticket-status--cancelled {
        color: var(--status-danger);
        background: var(--status-danger-bg);
        border-color: var(--status-danger-br);
      }

      .ticket-card .btn {
        align-self: flex-start;
      }
    `,
  ],
})
export class BookingTicketCardComponent {
  readonly ticket = input.required<ITicket>();
  readonly viewTicket = output<number>();
}
