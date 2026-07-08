import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'app-seat-legend',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="seat-legend">
      <div class="seat-legend__item">
        <span class="seat-legend__indicator seat-legend__indicator--available"></span>
        <span class="seat-legend__label">Available</span>
      </div>
      <div class="seat-legend__item">
        <span class="seat-legend__indicator seat-legend__indicator--selected"></span>
        <span class="seat-legend__label">Selected</span>
      </div>
      <div class="seat-legend__item">
        <span class="seat-legend__indicator seat-legend__indicator--reserved"></span>
        <span class="seat-legend__label">Reserved</span>
      </div>
    </div>
  `,
  styles: [
    `
      .seat-legend {
        display: flex;
        justify-content: center;
        gap: 1.5rem;
        flex-wrap: wrap;
        margin-bottom: 1.5rem;
      }

      .seat-legend__item {
        display: flex;
        align-items: center;
        gap: 0.5rem;
      }

      .seat-legend__indicator {
        width: 1.5rem;
        height: 1.5rem;
        border-radius: var(--radius-sm);
        border: 1px solid var(--ghost-border);
      }

      .seat-legend__indicator--available {
        background: var(--surface-container);
      }

      .seat-legend__indicator--selected {
        background: var(--primary-container);
        border-color: var(--primary-container);
      }

      .seat-legend__indicator--reserved {
        background: var(--surface-dim);
        opacity: 0.5;
      }

      .seat-legend__label {
        font-size: var(--text-body-sm);
        font-weight: 500;
        color: var(--on-surface-variant);
      }
    `,
  ],
})
export class SeatLegendComponent {}
