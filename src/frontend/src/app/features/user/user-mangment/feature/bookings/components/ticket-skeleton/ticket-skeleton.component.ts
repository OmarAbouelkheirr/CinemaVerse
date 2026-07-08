import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'app-ticket-skeleton',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <article class="ticket-skeleton" aria-hidden="true">
      <section class="ticket-skeleton__left">
        <div class="line line--poster skeleton-pulse"></div>
        <div class="line line--title skeleton-pulse"></div>
        @for (i of [1, 2, 3, 4, 5, 6, 7]; track i) {
          <div class="line line--meta skeleton-pulse"></div>
        }
      </section>

      <section class="ticket-skeleton__right">
        <div class="line line--qr skeleton-pulse"></div>
        @for (i of [1, 2, 3]; track i) {
          <div class="line line--meta skeleton-pulse"></div>
        }
      </section>
    </article>
  `,
  styles: [`
    .ticket-skeleton {
      width: 100%;
      background: var(--surface-container-low);
      border: 1px solid var(--ghost-border);
      border-radius: var(--radius-lg);
      overflow: hidden;
      display: grid;
      grid-template-columns: 1.2fr 0.8fr;
    }

    .ticket-skeleton__left,
    .ticket-skeleton__right {
      padding: clamp(1rem, 2.2vw, 1.4rem);
      display: flex;
      flex-direction: column;
      gap: 0.625rem;
    }

    .ticket-skeleton__left {
      border-right: 1px dashed var(--ghost-border);
    }

    .line {
      border-radius: var(--radius-sm);
      background: var(--surface-container-high);
    }

    .line--poster {
      width: 100%;
      max-width: 320px;
      aspect-ratio: 16 / 9;
      border-radius: var(--radius-md);
    }

    .line--title {
      width: min(18rem, 90%);
      height: 1.5rem;
    }

    .line--meta {
      width: min(12rem, 80%);
      height: 0.9rem;
    }

    .line--qr {
      width: 190px;
      max-width: 100%;
      aspect-ratio: 1;
      border-radius: var(--radius-md);
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

    @media (max-width: 768px) {
      .ticket-skeleton {
        grid-template-columns: 1fr;
      }

      .ticket-skeleton__left {
        border-right: 0;
        border-bottom: 1px dashed var(--ghost-border);
      }
    }
  `],
})
export class TicketSkeletonComponent {}
