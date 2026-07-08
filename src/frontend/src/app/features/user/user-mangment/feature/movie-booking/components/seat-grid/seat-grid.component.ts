import { ChangeDetectionStrategy, Component, computed, input, output } from '@angular/core';

import { Seat, SeatRow, SeatState } from '../../interfaces/seat.interface';
import { SeatComponent } from '../seat/seat.component';

@Component({
  selector: 'app-seat-grid',
  standalone: true,
  imports: [SeatComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="seat-grid">
      @for (row of seatRows(); track row.rowLabel) {
        <div class="seat-grid__row">
          <span class="seat-grid__row-label">{{ row.rowLabel }}</span>
          <div class="seat-grid__seats">
            @for (seat of row.seats; track seat.seatId) {
              <app-seat
                [seat]="seat"
                [state]="getSeatState(seat)"
                (seatSelected)="seatSelected.emit($event)" />
            }
          </div>
        </div>
      }
    </div>
  `,
  styles: [
    `
      .seat-grid {
        display: flex;
        flex-direction: column;
        gap: 0.5rem;
        align-items: center;
      }

      .seat-grid__row {
        display: flex;
        align-items: center;
        gap: 0.5rem;
      }

      .seat-grid__row-label {
        width: 1.5rem;
        text-align: center;
        font-size: var(--text-label);
        font-weight: 600;
        color: var(--on-surface-muted);
      }

      .seat-grid__seats {
        display: flex;
        gap: 0.375rem;
      }

      @media (max-width: 767px) {
        .seat-grid {
          gap: 0.375rem;
          overflow-x: auto;
          -webkit-overflow-scrolling: touch;
          padding-bottom: 0.5rem;
        }

        .seat-grid__row {
          gap: 0.375rem;
        }

        .seat-grid__seats {
          gap: 0.25rem;
        }
      }
    `,
  ],
})
export class SeatGridComponent {
  readonly availableSeats = input.required<Seat[]>();
  readonly reservedSeats = input.required<Seat[]>();
  readonly selectedSeats = input.required<Seat[]>();
  readonly maxSelection = input<number>(0);
  readonly seatSelected = output<Seat>();

  readonly seatRows = computed((): SeatRow[] => {
    const allSeats = [...this.availableSeats(), ...this.reservedSeats()];
    const rowMap = new Map<string, Seat[]>();

    for (const seat of allSeats) {
      const row = rowMap.get(seat.seatRow);
      if (row) {
        row.push(seat);
      } else {
        rowMap.set(seat.seatRow, [seat]);
      }
    }

    const rows: SeatRow[] = [];
    for (const [rowLabel, seats] of rowMap.entries()) {
      seats.sort((a, b) => a.seatColumn - b.seatColumn);
      rows.push({ rowLabel, seats });
    }

    rows.sort((a, b) => a.rowLabel.localeCompare(b.rowLabel));
    return rows;
  });

  getSeatState(seat: Seat): SeatState {
    const isSelected = this.selectedSeats().some((s) => s.seatId === seat.seatId);
    if (isSelected) return 'selected';

    const isReserved = this.reservedSeats().some((s) => s.seatId === seat.seatId);
    if (isReserved) return 'reserved';

    return 'available';
  }
}
