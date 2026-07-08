import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'app-booking-card-skeleton',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="skeleton" aria-hidden="true">
      <div class="skeleton__poster skeleton-pulse"></div>
      <div class="skeleton__body">
        <div class="skeleton__title skeleton-pulse"></div>
        <div class="skeleton__meta">
          <div class="skeleton__meta-item skeleton-pulse"></div>
          <div class="skeleton__meta-item skeleton-pulse"></div>
        </div>
        <div class="skeleton__meta">
          <div class="skeleton__meta-item skeleton-pulse"></div>
        </div>
        <div class="skeleton__footer">
          <div class="skeleton__price skeleton-pulse"></div>
          <div class="skeleton__btn skeleton-pulse"></div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .skeleton {
      display: flex;
      flex-direction: column;
      background: var(--surface-container-low);
      border: 1px solid var(--ghost-border);
      border-radius: var(--radius-lg);
      overflow: hidden;
    }

    .skeleton__poster {
      aspect-ratio: 16 / 9;
      background: var(--surface-container);
    }

    .skeleton__body {
      display: flex;
      flex-direction: column;
      gap: 0.75rem;
      padding: 1rem 1.25rem 1.25rem;
    }

    .skeleton__title {
      height: 1.25rem;
      width: 75%;
      border-radius: var(--radius-sm);
      background: var(--surface-container-high);
    }

    .skeleton__meta {
      display: flex;
      gap: 0.75rem;
    }

    .skeleton__meta-item {
      height: 0.75rem;
      width: 5rem;
      border-radius: var(--radius-sm);
      background: var(--surface-container-high);
    }

    .skeleton__footer {
      display: flex;
      align-items: center;
      justify-content: space-between;
      margin-top: auto;
      padding-top: 0.75rem;
      border-top: 1px solid var(--ghost-border);
    }

    .skeleton__price {
      height: 1rem;
      width: 4rem;
      border-radius: var(--radius-sm);
      background: var(--surface-container-high);
    }

    .skeleton__btn {
      height: 2rem;
      width: 6rem;
      border-radius: var(--radius-md);
      background: var(--surface-container-high);
    }

    .skeleton-pulse {
      animation: pulse 1.5s ease-in-out infinite;
    }

    @keyframes pulse {
      0%, 100% { opacity: 1; }
      50% { opacity: 0.4; }
    }
  `],
})
export class BookingCardSkeletonComponent {}
