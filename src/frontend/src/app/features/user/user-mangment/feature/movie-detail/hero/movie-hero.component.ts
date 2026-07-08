import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { CommonModule } from '@angular/common';

import { MovieDetail } from '../interfaces/movie-detail.interface';

@Component({
  selector: 'app-movie-hero',
  standalone: true,
  imports: [CommonModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    @if (movie(); as m) {
      <div class="hero">
        <div class="hero__poster-wrapper">
          <img class="hero__poster" [src]="m.moviePoster" [alt]="m.movieName" loading="eager" />
        </div>

        <div class="hero__info">
          <h1 class="hero__title">{{ m.movieName }}</h1>

          <div class="hero__meta">
            @if (m.movieRating > 0) {
              <span class="hero__rating">
                <span class="material-symbols-outlined">star</span>
                {{ m.movieRating | number: '1.1-1' }}
              </span>
            }
            <span class="hero__duration">{{ m.movieDuration }}</span>
            <span class="hero__language">{{ m.language }}</span>
          </div>

          <div class="hero__genres">
            @for (genre of m.genres; track genre.genreId) {
              <span class="hero__genre">{{ genre.name }}</span>
            }
          </div>

          @if (m.movieDescription) {
            <p class="hero__description">{{ m.movieDescription }}</p>
          }

          <div class="hero__actions">
            @if (m.trailerUrl) {
              <a [href]="m.trailerUrl" target="_blank" class="btn btn-secondary">
                <span class="material-symbols-outlined">play_circle</span>
                Watch Trailer
              </a>
            }
          </div>
        </div>
      </div>
    }
  `,
  styles: [
    `
      .hero {
        display: grid;
        grid-template-columns: clamp(14rem, 25vw, 18rem) 1fr;
        gap: 2rem;
        padding: 2rem;
        background: var(--surface-container-low);
        border: 1px solid var(--ghost-border);
        border-radius: var(--radius-lg);
      }

      .hero__poster-wrapper {
        aspect-ratio: 2 / 3;
        border-radius: var(--radius-md);
        overflow: hidden;
        box-shadow: var(--shadow-card);
      }

      .hero__poster {
        width: 100%;
        height: 100%;
        object-fit: cover;
      }

      .hero__info {
        display: flex;
        flex-direction: column;
        gap: 1rem;
      }

      .hero__title {
        font-size: var(--text-display);
        font-weight: 700;
        color: var(--on-surface);
        margin: 0;
        line-height: 1.2;
      }

      .hero__meta {
        display: flex;
        gap: 1rem;
        flex-wrap: wrap;
        align-items: center;
      }

      .hero__rating {
        display: flex;
        align-items: center;
        gap: 0.25rem;
        font-size: var(--text-body);
        font-weight: 600;
        color: var(--primary-container);
      }

      .hero__rating .material-symbols-outlined {
        font-size: 18px;
      }

      .hero__duration,
      .hero__language {
        font-size: var(--text-body-sm);
        color: var(--on-surface-variant);
      }

      .hero__genres {
        display: flex;
        gap: 0.5rem;
        flex-wrap: wrap;
      }

      .hero__genre {
        padding: 0.25rem 0.75rem;
        background: var(--surface-container);
        border: 1px solid var(--ghost-border);
        border-radius: var(--radius-md);
        font-size: var(--text-body-sm);
        color: var(--on-surface-variant);
      }

      .hero__description {
        font-size: var(--text-body);
        color: var(--on-surface-variant);
        line-height: 1.6;
        margin: 0;
      }

      .hero__actions {
        display: flex;
        gap: 0.75rem;
        margin-top: auto;
      }

      @media (max-width: 767px) {
        .hero {
          grid-template-columns: 1fr;
          padding: 1rem;
        }

        .hero__poster-wrapper {
          max-width: 12rem;
          margin: 0 auto;
        }

        .hero__title {
          font-size: var(--text-page);
        }
      }
    `,
  ],
})
export class MovieHeroComponent {
  readonly movie = input.required<MovieDetail>();
}
