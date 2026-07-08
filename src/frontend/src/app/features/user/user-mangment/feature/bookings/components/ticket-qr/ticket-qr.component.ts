// TicketQrComponent — displays a QR code along with the booking ID and ticket number
import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';

@Component({
  selector: 'app-ticket-qr',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="ticket-qr" aria-label="Ticket QR code section">
      <img
        class="ticket-qr__image"
        [src]="qrImageSrc()"
        [alt]="'QR code for ticket ' + ticketNumber()"
        loading="lazy"
      />

      <p class="ticket-qr__line">
        <span>Booking ID:</span> <strong>{{ bookingId() }}</strong>
      </p>
      <p class="ticket-qr__line">
        <span>Ticket #:</span> <strong>{{ ticketNumber() }}</strong>
      </p>
    </div>
  `,
  styles: [
    `
      .ticket-qr {
        display: flex;
        flex-direction: column;
        align-items: center;
        gap: 0.625rem;
      }

      .ticket-qr__image {
        width: min(220px, 100%);
        aspect-ratio: 1;
        background: #fff;
        border-radius: var(--radius-md);
        border: 1px solid var(--ghost-border);
        padding: 0.4rem;
        object-fit: contain;
      }

      .ticket-qr__line {
        margin: 0;
        font-size: var(--text-body-sm);
        color: var(--on-surface-variant);
        text-align: center;
      }

      .ticket-qr__line strong {
        color: var(--on-surface);
        font-weight: 600;
      }
    `,
  ],
})
export class TicketQrComponent {
  readonly qrCode = input.required<string>();
  readonly bookingId = input.required<number>();
  readonly ticketNumber = input.required<string>();

  readonly qrImageSrc = computed(() => {
    const encoded = encodeURIComponent(this.qrCode());
    return `https://api.qrserver.com/v1/create-qr-code/?size=220x220&data=${encoded}`;
  });
}
