import { ChangeDetectionStrategy, Component, input } from '@angular/core';

import { MovieInfo } from '../../interfaces/seat.interface';

@Component({
  selector: 'app-movie-info-card',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="movie-card">
      <img
        class="movie-card__poster"
        [src]="movie().moviePoster"
        [alt]="movie().movieName"
        loading="lazy" />
      <div class="movie-card__info">
        <h3 class="movie-card__title">{{ movie().movieName }}</h3>
        @if (movie().movieDuration) {
          <span class="movie-card__duration">{{ movie().movieDuration }}</span>
        }
      </div>
    </div>
  `,
  styles: [
    `
      .movie-card {
        display: flex;
        gap: 1rem;
        padding: 1rem;
        background: var(--surface-container-low);
        border: 1px solid var(--ghost-border);
        border-radius: var(--radius-lg);
      }

      .movie-card__poster {
        width: 4.5rem;
        height: 6.5rem;
        object-fit: cover;
        border-radius: var(--radius-md);
        flex-shrink: 0;
      }

      .movie-card__info {
        display: flex;
        flex-direction: column;
        justify-content: center;
        gap: 0.375rem;
        min-width: 0;
      }

      .movie-card__title {
        font-size: var(--text-title-sm);
        font-weight: 700;
        color: var(--on-surface);
        margin: 0;
        line-height: 1.3;
        overflow: hidden;
        text-overflow: ellipsis;
        display: -webkit-box;
        -webkit-line-clamp: 2;
        -webkit-box-orient: vertical;
      }

      .movie-card__duration {
        font-size: var(--text-body-sm);
        color: var(--on-surface-muted);
      }
    `,
  ],
})
export class MovieInfoCardComponent {
  readonly movie = input.required<MovieInfo>();
}
