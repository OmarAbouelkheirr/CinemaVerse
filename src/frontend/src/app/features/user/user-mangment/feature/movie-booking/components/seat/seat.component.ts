import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';

import { Seat, SeatState } from '../../interfaces/seat.interface';

@Component({
  selector: 'app-seat',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <button
      type="button"
      class="seat"
      [class.seat--available]="state() === 'available'"
      [class.seat--reserved]="state() === 'reserved'"
      [class.seat--selected]="state() === 'selected'"
      [disabled]="state() === 'reserved'"
      [attr.title]="seat().seatLabel"
      (click)="onSeatClick()">
      <span class="seat__label">{{ seat().seatLabel }}</span>
    </button>
  `,
  styles: [
    `
      .seat {
        display: flex;
        align-items: center;
        justify-content: center;
        width: clamp(2rem, 4vw, 2.5rem);
        height: clamp(2rem, 4vw, 2.5rem);
        border-radius: var(--radius-sm);
        border: 1px solid var(--ghost-border);
        cursor: pointer;
        transition: all var(--transition);
        font-family: inherit;
        font-size: var(--text-label);
        font-weight: 600;
      }

      .seat--available {
        background: var(--surface-container);
        color: var(--on-surface-variant);
      }

      .seat--available:hover {
        background: var(--surface-container-high);
        border-color: var(--primary-container);
        color: var(--primary-container);
        transform: scale(1.08);
      }

      .seat--selected {
        background: var(--primary-container);
        border-color: var(--primary-container);
        color: var(--on-primary);
        box-shadow: 0 0 8px rgba(34, 211, 238, 0.3);
        animation: seatPulse 0.25s ease;
      }

      .seat--reserved {
        background: var(--surface-dim);
        border-color: var(--ghost-border);
        color: var(--on-surface-muted);
        cursor: not-allowed;
        opacity: 0.5;
      }

      .seat__label {
        pointer-events: none;
      }

      @keyframes seatPulse {
        0% { transform: scale(1); }
        50% { transform: scale(1.12); }
        100% { transform: scale(1); }
      }

      @media (max-width: 767px) {
        .seat {
          width: clamp(2.25rem, 8vw, 2.75rem);
          height: clamp(2.25rem, 8vw, 2.75rem);
        }
      }
    `,
  ],
})
export class SeatComponent {
  readonly seat = input.required<Seat>();
  readonly state = input.required<SeatState>();
  readonly seatSelected = output<Seat>();

  onSeatClick(): void {
    if (this.state() !== 'reserved') {
      this.seatSelected.emit(this.seat());
    }
  }
}
