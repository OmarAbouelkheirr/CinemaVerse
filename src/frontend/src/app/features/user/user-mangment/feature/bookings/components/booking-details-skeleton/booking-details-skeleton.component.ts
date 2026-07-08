import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'app-booking-details-skeleton',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="details-skeleton" aria-hidden="true">
      <section class="details-skeleton__header">
        <div class="line line--title skeleton-pulse"></div>
        <div class="line line--meta skeleton-pulse"></div>
        <div class="line line--badge skeleton-pulse"></div>
      </section>

      <section class="details-skeleton__content">
        <article class="details-skeleton__card">
          <div class="line line--section-title skeleton-pulse"></div>
          <div class="poster skeleton-pulse"></div>
          <div class="line line--meta skeleton-pulse"></div>
          <div class="line line--meta skeleton-pulse"></div>
          <div class="line line--meta skeleton-pulse"></div>
          <div class="line line--meta skeleton-pulse"></div>
        </article>

        <article class="details-skeleton__card">
          <div class="line line--section-title skeleton-pulse"></div>
          @for (i of [1, 2, 3]; track i) {
            <div class="ticket-skeleton">
              <div class="line line--meta skeleton-pulse"></div>
              <div class="line line--meta skeleton-pulse"></div>
              <div class="line line--button skeleton-pulse"></div>
            </div>
          }
        </article>
      </section>
    </div>
  `,
  styles: [`
    .details-skeleton {
      display: flex;
      flex-direction: column;
      gap: 1rem;
    }

    .details-skeleton__header,
    .details-skeleton__card {
      background: var(--surface-container-low);
      border: 1px solid var(--ghost-border);
      border-radius: var(--radius-lg);
      padding: clamp(1rem, 2.2vw, 1.25rem);
    }

    .details-skeleton__header {
      display: flex;
      flex-direction: column;
      gap: 0.625rem;
    }

    .details-skeleton__content {
      display: grid;
      grid-template-columns: 1.1fr 0.9fr;
      gap: clamp(1rem, 2.5vw, 1.5rem);
    }

    .details-skeleton__card {
      display: flex;
      flex-direction: column;
      gap: 0.625rem;
    }

    .poster {
      width: 100%;
      max-width: 320px;
      aspect-ratio: 16 / 9;
      border-radius: var(--radius-md);
      background: var(--surface-container);
    }

    .ticket-skeleton {
      display: flex;
      flex-direction: column;
      gap: 0.5rem;
      border: 1px solid var(--ghost-border);
      border-radius: var(--radius-md);
      padding: 0.75rem;
      background: var(--surface-container);
    }

    .line {
      border-radius: var(--radius-sm);
      background: var(--surface-container-high);
    }

    .line--title {
      width: min(18rem, 85%);
      height: 1.5rem;
    }

    .line--section-title {
      width: min(10rem, 65%);
      height: 1.1rem;
    }

    .line--meta {
      width: min(12rem, 70%);
      height: 0.875rem;
    }

    .line--badge {
      width: 5.5rem;
      height: 1.4rem;
      border-radius: 999px;
    }

    .line--button {
      width: 7rem;
      height: 2rem;
      margin-top: 0.25rem;
    }

    .skeleton-pulse {
      animation: pulse 1.5s ease-in-out infinite;
    }

    @keyframes pulse {
      0%,
      100% {
        opacity: 1;
      }
      50% {
        opacity: 0.45;
      }
    }

    @media (max-width: 1024px) {
      .details-skeleton__content {
        grid-template-columns: 1fr;
      }
    }
  `],
})
export class BookingDetailsSkeletonComponent {}
