import { ChangeDetectionStrategy, Component, computed, effect, input, output, signal } from '@angular/core';
import { CommonModule } from '@angular/common';

import { Showtime, ShowtimeGroup, FormatGroup, BranchGroup, HallGroup } from '../interfaces/movie-detail.interface';

@Component({
  selector: 'app-book-panel',
  standalone: true,
  imports: [CommonModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="book-panel">
      <h2 class="book-panel__title">Select Showtime</h2>

      @if (showtimes().length === 0) {
        <div class="book-panel__empty">
          <span class="material-symbols-outlined">event_busy</span>
          <p>No showtimes available</p>
        </div>
      } @else {
        <div class="book-panel__dates">
          @for (dateGroup of groupedShowtimes(); track dateGroup.date) {
            <button
              type="button"
              class="book-panel__date-btn"
              [class.book-panel__date-btn--active]="selectedDate() === dateGroup.date"
              (click)="selectedDate.set(dateGroup.date)">
              <span class="book-panel__date-day">{{ formatDateDay(dateGroup.date) }}</span>
              <span class="book-panel__date-num">{{ formatDateNum(dateGroup.date) }}</span>
              <span class="book-panel__date-month">{{ formatDateMonth(dateGroup.date) }}</span>
            </button>
          }
        </div>

        @if (selectedDate(); as date) {
          @if (getDateGroup(date); as dateGroup) {
            <div class="book-panel__formats">
              @for (format of dateGroup.formats; track format.format) {
                <button
                  type="button"
                  class="book-panel__format-btn"
                  [class.book-panel__format-btn--active]="selectedFormat() === format.format"
                  (click)="selectedFormat.set(format.format)">
                  {{ format.format }}
                </button>
              }
            </div>

            @if (selectedFormat(); as format) {
              @if (getFormatGroup(date, format); as formatGroup) {
                <div class="book-panel__branches">
                  @for (branch of formatGroup.branches; track branch.branchName) {
                    <div class="book-panel__branch">
                      <h3 class="book-panel__branch-name">{{ branch.branchName }}</h3>
                      <p class="book-panel__branch-location">{{ branch.branchLocation }}</p>

                      <div class="book-panel__halls">
                        @for (hall of branch.halls; track hall.hallNumber) {
                          <div class="book-panel__hall">
                            <span class="book-panel__hall-label">Hall {{ hall.hallNumber }}</span>

                            <div class="book-panel__times">
                              @for (showtime of hall.showtimes; track showtime.movieShowTimeId) {
                                <button
                                  type="button"
                                  class="book-panel__time-btn"
                                  [class.book-panel__time-btn--selected]="selectedShowtime()?.movieShowTimeId === showtime.movieShowTimeId"
                                  (click)="onSelectShowtime(showtime)">
                                  {{ formatTime(showtime.showStartTime) }}
                                  <span class="book-panel__time-price">{{ showtime.ticketPrice }} EGP</span>
                                </button>
                              }
                            </div>
                          </div>
                        }
                      </div>
                    </div>
                  }
                </div>
              }
            }
          }
        }

        @if (selectedShowtime(); as st) {
          <div class="book-panel__summary">
            <div class="book-panel__summary-row">
              <span class="book-panel__summary-label">Selected:</span>
              <span class="book-panel__summary-value">
                {{ formatDateFull(st.showStartTime) }} at {{ formatTime(st.showStartTime) }}
              </span>
            </div>
            <div class="book-panel__summary-row">
              <span class="book-panel__summary-label">Hall:</span>
              <span class="book-panel__summary-value">{{ st.hallNumber }} ({{ st.hallType }})</span>
            </div>
            <div class="book-panel__summary-row">
              <span class="book-panel__summary-label">Cinema:</span>
              <span class="book-panel__summary-value">{{ st.branchName }}</span>
            </div>
          </div>
        }

        <button
          type="button"
          class="book-panel__continue"
          [class.book-panel__continue--disabled]="!selectedShowtime()"
          [disabled]="!selectedShowtime()"
          (click)="onContinue()">
          Continue to Seat Selection
          <span class="material-symbols-outlined">arrow_forward</span>
        </button>
      }
    </div>
  `,
  styles: [
    `
      .book-panel {
        display: flex;
        flex-direction: column;
        gap: 1.25rem;
        padding: 1.5rem;
        background: var(--surface-container-low);
        border: 1px solid var(--ghost-border);
        border-radius: var(--radius-lg);
      }

      .book-panel__title {
        font-size: var(--text-title);
        font-weight: 700;
        color: var(--on-surface);
        margin: 0;
      }

      .book-panel__empty {
        display: flex;
        flex-direction: column;
        align-items: center;
        gap: 0.75rem;
        padding: 3rem 1rem;
        text-align: center;
      }

      .book-panel__empty .material-symbols-outlined {
        font-size: 3rem;
        color: var(--on-surface-muted);
      }

      .book-panel__empty p {
        font-size: var(--text-body);
        color: var(--on-surface-muted);
        margin: 0;
      }

      .book-panel__dates {
        display: flex;
        gap: 0.5rem;
        overflow-x: auto;
        padding-bottom: 0.5rem;
      }

      .book-panel__date-btn {
        display: flex;
        flex-direction: column;
        align-items: center;
        gap: 0.25rem;
        padding: 0.75rem 1rem;
        background: var(--surface-container);
        border: 1px solid var(--ghost-border);
        border-radius: var(--radius-md);
        cursor: pointer;
        transition: all var(--transition);
        font-family: inherit;
        min-width: 4.5rem;
      }

      .book-panel__date-btn:hover {
        background: var(--surface-container-high);
        border-color: var(--primary-dim);
      }

      .book-panel__date-btn--active {
        background: var(--primary-dim);
        border-color: var(--primary-container);
      }

      .book-panel__date-day {
        font-size: var(--text-label);
        font-weight: 600;
        color: var(--on-surface-muted);
        text-transform: uppercase;
      }

      .book-panel__date-num {
        font-size: var(--text-title);
        font-weight: 700;
        color: var(--on-surface);
      }

      .book-panel__date-month {
        font-size: var(--text-label);
        color: var(--on-surface-variant);
      }

      .book-panel__date-btn--active .book-panel__date-day,
      .book-panel__date-btn--active .book-panel__date-num,
      .book-panel__date-btn--active .book-panel__date-month {
        color: var(--primary-container);
      }

      .book-panel__formats {
        display: flex;
        gap: 0.5rem;
        flex-wrap: wrap;
      }

      .book-panel__format-btn {
        padding: 0.5rem 1rem;
        background: var(--surface-container);
        border: 1px solid var(--ghost-border);
        border-radius: var(--radius-md);
        font-size: var(--text-body-sm);
        font-weight: 600;
        color: var(--on-surface-variant);
        cursor: pointer;
        transition: all var(--transition);
        font-family: inherit;
      }

      .book-panel__format-btn:hover {
        background: var(--surface-container-high);
        border-color: var(--primary-dim);
      }

      .book-panel__format-btn--active {
        background: var(--primary-dim);
        border-color: var(--primary-container);
        color: var(--primary-container);
      }

      .book-panel__branches {
        display: flex;
        flex-direction: column;
        gap: 1.25rem;
      }

      .book-panel__branch {
        display: flex;
        flex-direction: column;
        gap: 0.75rem;
        padding: 1rem;
        background: var(--surface-container);
        border: 1px solid var(--ghost-border);
        border-radius: var(--radius-md);
      }

      .book-panel__branch-name {
        font-size: var(--text-title-sm);
        font-weight: 700;
        color: var(--on-surface);
        margin: 0;
      }

      .book-panel__branch-location {
        font-size: var(--text-body-sm);
        color: var(--on-surface-muted);
        margin: 0;
      }

      .book-panel__halls {
        display: flex;
        flex-direction: column;
        gap: 0.75rem;
        margin-top: 0.5rem;
      }

      .book-panel__hall {
        display: flex;
        flex-direction: column;
        gap: 0.5rem;
      }

      .book-panel__hall-label {
        font-size: var(--text-body-sm);
        font-weight: 600;
        color: var(--on-surface-variant);
      }

      .book-panel__times {
        display: flex;
        gap: 0.5rem;
        flex-wrap: wrap;
      }

      .book-panel__time-btn {
        display: flex;
        flex-direction: column;
        align-items: center;
        gap: 0.25rem;
        padding: 0.5rem 0.75rem;
        background: var(--surface-container-low);
        border: 1px solid var(--ghost-border);
        border-radius: var(--radius-sm);
        font-size: var(--text-body-sm);
        font-weight: 600;
        color: var(--on-surface);
        cursor: pointer;
        transition: all var(--transition);
        font-family: inherit;
      }

      .book-panel__time-btn:hover {
        background: var(--surface-container-high);
        border-color: var(--primary-container);
      }

      .book-panel__time-btn--selected {
        background: var(--primary-container);
        border-color: var(--primary-container);
        color: var(--on-primary);
      }

      .book-panel__time-price {
        font-size: var(--text-label);
        color: var(--on-surface-muted);
      }

      .book-panel__time-btn--selected .book-panel__time-price {
        color: var(--on-primary);
      }

      .book-panel__summary {
        display: flex;
        flex-direction: column;
        gap: 0.5rem;
        padding: 1rem;
        background: var(--surface-container);
        border: 1px solid var(--ghost-border);
        border-radius: var(--radius-md);
      }

      .book-panel__summary-row {
        display: flex;
        justify-content: space-between;
        gap: 1rem;
      }

      .book-panel__summary-label {
        font-size: var(--text-body-sm);
        color: var(--on-surface-muted);
      }

      .book-panel__summary-value {
        font-size: var(--text-body-sm);
        font-weight: 600;
        color: var(--on-surface);
        text-align: right;
      }

      .book-panel__continue {
        display: flex;
        align-items: center;
        justify-content: center;
        gap: 0.5rem;
        padding: 0.875rem 1.5rem;
        background: linear-gradient(135deg, var(--primary-container) 0%, var(--primary) 100%);
        border: none;
        border-radius: var(--radius-md);
        font-size: var(--text-body);
        font-weight: 700;
        color: var(--on-primary);
        cursor: pointer;
        transition: all var(--transition);
        font-family: inherit;
        text-transform: uppercase;
        letter-spacing: 0.04em;
      }

      .book-panel__continue:hover:not(.book-panel__continue--disabled) {
        filter: brightness(1.08);
        transform: translateY(-1px);
        box-shadow: 0 4px 16px rgba(34, 211, 238, 0.25);
      }

      .book-panel__continue--disabled {
        opacity: 0.4;
        cursor: not-allowed;
      }

      .book-panel__continue .material-symbols-outlined {
        font-size: 18px;
      }

      @media (max-width: 767px) {
        .book-panel {
          padding: 1rem;
        }
      }
    `,
  ],
})
export class BookPanelComponent {
  readonly showtimes = input.required<Showtime[]>();
  readonly continueClick = output<Showtime>();

  readonly selectedDate = signal<string | null>(null);
  readonly selectedFormat = signal<string | null>(null);
  readonly selectedShowtime = signal<Showtime | null>(null);

  constructor() {
    effect(() => {
      const groups = this.groupedShowtimes();
      if (groups.length > 0 && !this.selectedDate()) {
        this.selectedDate.set(groups[0].date);
      }
      if (groups.length > 0 && groups[0].formats.length > 0 && !this.selectedFormat()) {
        this.selectedFormat.set(groups[0].formats[0].format);
      }
    });
  }

  readonly groupedShowtimes = computed((): ShowtimeGroup[] => {
    const showtimes = this.showtimes();
    
    if (!showtimes || showtimes.length === 0) {
      return [];
    }

    const dateMap = new Map<string, Showtime[]>();

    for (const st of showtimes) {
      const date = this.extractDate(st.showStartTime);
      const existing = dateMap.get(date);
      if (existing) {
        existing.push(st);
      } else {
        dateMap.set(date, [st]);
      }
    }

    const groups: ShowtimeGroup[] = [];
    for (const [date, sts] of dateMap.entries()) {
      const formats = this.groupByFormat(sts);
      groups.push({
        date,
        formats,
      });
    }

    groups.sort((a, b) => a.date.localeCompare(b.date));

    return groups;
  });

  private groupByFormat(showtimes: Showtime[]): FormatGroup[] {
    const formatMap = new Map<string, Showtime[]>();

    for (const st of showtimes) {
      const format = st.hallType;
      const existing = formatMap.get(format);
      if (existing) {
        existing.push(st);
      } else {
        formatMap.set(format, [st]);
      }
    }

    const groups: FormatGroup[] = [];
    for (const [format, sts] of formatMap.entries()) {
      const branches = this.groupByBranch(sts);
      groups.push({
        format,
        branches,
      });
    }

    return groups;
  }

  private groupByBranch(showtimes: Showtime[]): BranchGroup[] {
    const branchMap = new Map<string, Showtime[]>();

    for (const st of showtimes) {
      const key = `${st.branchName}|${st.branchLocation}`;
      const existing = branchMap.get(key);
      if (existing) {
        existing.push(st);
      } else {
        branchMap.set(key, [st]);
      }
    }

    const groups: BranchGroup[] = [];
    for (const [key, sts] of branchMap.entries()) {
      const [branchName, branchLocation] = key.split('|');
      const halls = this.groupByHall(sts);
      groups.push({
        branchName,
        branchLocation,
        halls,
      });
    }

    return groups;
  }

  private groupByHall(showtimes: Showtime[]): HallGroup[] {
    const hallMap = new Map<string, Showtime[]>();

    for (const st of showtimes) {
      const key = `${st.hallNumber}|${st.hallType}`;
      const existing = hallMap.get(key);
      if (existing) {
        existing.push(st);
      } else {
        hallMap.set(key, [st]);
      }
    }

    const groups: HallGroup[] = [];
    for (const [key, sts] of hallMap.entries()) {
      const [hallNumber, hallType] = key.split('|');
      groups.push({
        hallNumber,
        hallType,
        showtimes: sts,
      });
    }

    return groups;
  }

  getDateGroup(date: string): ShowtimeGroup | undefined {
    return this.groupedShowtimes().find((g) => g.date === date);
  }

  getFormatGroup(date: string, format: string): FormatGroup | undefined {
    const dateGroup = this.getDateGroup(date);
    return dateGroup?.formats.find((f: FormatGroup) => f.format === format);
  }

  onSelectShowtime(showtime: Showtime): void {
    this.selectedShowtime.set(showtime);
  }

  onContinue(): void {
    const st = this.selectedShowtime();
    if (st) {
      this.continueClick.emit(st);
    }
  }

  private extractDate(isoString: string): string {
    return isoString.split('T')[0];
  }

  formatDateDay(date: string): string {
    const d = new Date(date);
    return d.toLocaleDateString('en-US', { weekday: 'short' });
  }

  formatDateNum(date: string): string {
    const d = new Date(date);
    return d.getDate().toString();
  }

  formatDateMonth(date: string): string {
    const d = new Date(date);
    return d.toLocaleDateString('en-US', { month: 'short' });
  }

  formatDateFull(isoString: string): string {
    const d = new Date(isoString);
    return d.toLocaleDateString('en-GB', {
      weekday: 'short',
      day: 'numeric',
      month: 'short',
    });
  }

  formatTime(isoString: string): string {
    const d = new Date(isoString);
    return d.toLocaleTimeString('en-GB', {
      hour: '2-digit',
      minute: '2-digit',
    });
  }
}
