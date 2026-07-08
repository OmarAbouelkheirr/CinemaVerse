import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';

import { BookingStatus } from '../../interfaces/booking.interface';

const STATUS_CONFIG: Record<BookingStatus, { label: string; class: string }> = {
  Pending: { label: 'Pending', class: 'badge--pending' },
  Confirmed: { label: 'Confirmed', class: 'badge--confirmed' },
  Cancelled: { label: 'Cancelled', class: 'badge--cancelled' },
  Expired: { label: 'Expired', class: 'badge--expired' },
};

@Component({
  selector: 'app-booking-status-badge',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <span class="badge" [class]="badgeClass()">{{ badgeLabel() }}</span>
  `,
  styles: [`
    .badge {
      display: inline-flex;
      align-items: center;
      gap: 5px;
      padding: 3px 10px;
      border-radius: 20px;
      font-size: var(--text-label);
      font-weight: 600;
      letter-spacing: 0.05em;
      text-transform: uppercase;
      border: 1px solid transparent;
    }

    .badge::before {
      content: '';
      width: 5px;
      height: 5px;
      border-radius: 50%;
      background: currentColor;
    }

    .badge--pending {
      color: var(--status-pending);
      background: var(--status-pending-bg);
      border-color: var(--status-pending-br);
    }

    .badge--confirmed {
      color: var(--status-active);
      background: var(--status-active-bg);
      border-color: var(--status-active-br);
    }

    .badge--cancelled {
      color: var(--status-danger);
      background: var(--status-danger-bg);
      border-color: var(--status-danger-br);
    }

    .badge--expired {
      color: var(--status-suspended);
      background: var(--status-suspended-bg);
      border-color: var(--status-suspended-br);
    }
  `],
})
export class BookingStatusBadgeComponent {
  readonly status = input.required<BookingStatus>();

  readonly badgeClass = computed(() => STATUS_CONFIG[this.status()].class);
  readonly badgeLabel = computed(() => STATUS_CONFIG[this.status()].label);
}
