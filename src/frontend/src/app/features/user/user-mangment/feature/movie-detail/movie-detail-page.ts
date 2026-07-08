import { ChangeDetectionStrategy, Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';

import { MovieDetailService } from './services/movie-detail.service';
import { BookingStateService } from '../movie-booking/state/booking-state.service';
import { MovieDetail, Showtime } from './interfaces/movie-detail.interface';
import { MovieHeroComponent } from './hero/movie-hero.component';
import { MovieTabsComponent } from './tabs/movie-tabs.component';
import { BookPanelComponent } from './book-panel/book-panel.component';

@Component({
  selector: 'app-movie-detail-page',
  standalone: true,
  imports: [CommonModule, MovieHeroComponent, MovieTabsComponent, BookPanelComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="page">
      @if (loading()) {
        <div class="page__loading">
          <span class="material-symbols-outlined page__spinner">progress_activity</span>
          <p>Loading movie details...</p>
        </div>
      } @else if (error(); as errorMsg) {
        <div class="page__error">
          <span class="material-symbols-outlined">error_outline</span>
          <p>{{ errorMsg }}</p>
        </div>
      } @else if (movie(); as m) {
        <div class="page__layout">
          <div class="page__main">
            <app-movie-hero [movie]="m" />
            <app-movie-tabs [movie]="m" />
          </div>

          <div class="page__sidebar">
            <div class="page__sidebar-sticky">
              <app-book-panel [showtimes]="m.showTimes" (continueClick)="onContinue($event)" />
            </div>
          </div>
        </div>
      }
    </div>
  `,
  styles: [
    `
      .page {
        padding: 2rem 0;
        width: min(92%, 90rem);
        margin-inline: auto;
      }

      .page__layout {
        display: grid;
        grid-template-columns: 1fr clamp(18rem, 28vw, 22rem);
        gap: 2rem;
        align-items: start;
      }

      .page__main {
        display: flex;
        flex-direction: column;
        gap: 2rem;
      }

      .page__sidebar-sticky {
        position: sticky;
        top: calc(var(--nav-height, 3.5rem) + 1rem);
      }

      .page__loading,
      .page__error {
        display: flex;
        flex-direction: column;
        align-items: center;
        justify-content: center;
        gap: 1rem;
        padding: 6rem 2rem;
        text-align: center;
      }

      .page__spinner {
        font-size: 2.5rem;
        color: var(--primary-container);
        animation: spin 1s linear infinite;
      }

      @keyframes spin {
        from {
          transform: rotate(0deg);
        }
        to {
          transform: rotate(360deg);
        }
      }

      .page__loading p,
      .page__error p {
        font-size: var(--text-body);
        color: var(--on-surface-muted);
        margin: 0;
      }

      .page__error .material-symbols-outlined {
        font-size: 2.5rem;
        color: var(--status-danger);
      }

      @media (max-width: 1199px) {
        .page__layout {
          grid-template-columns: 1fr;
        }

        .page__sidebar-sticky {
          position: static;
        }
      }

      @media (max-width: 767px) {
        .page {
          padding: 1rem 0;
        }
      }
    `,
  ],
})
export class MovieDetailPage implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly movieDetailService = inject(MovieDetailService);
  private readonly bookingState = inject(BookingStateService);

  readonly movie = signal<MovieDetail | null>(null);
  readonly loading = signal(false);
  readonly error = signal<string | null>(null);

  ngOnInit(): void {
    const movieId = Number(this.route.snapshot.paramMap.get('id'));
    if (!movieId || isNaN(movieId)) {
      this.error.set('Invalid movie ID');
      return;
    }

    this.loadMovie(movieId);
  }

  private loadMovie(movieId: number): void {
    this.loading.set(true);
    this.error.set(null);

    this.movieDetailService.getMovieDetail(movieId).subscribe({
      next: (movie: MovieDetail) => {
        this.movie.set(movie);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Failed to load movie details. Please try again.');
        this.loading.set(false);
      },
    });
  }

  onContinue(showtime: Showtime): void {
    const m = this.movie();
    if (!m) return;

    this.bookingState.setMovie({
      movieId: m.movieId,
      movieName: m.movieName,
      moviePoster: m.moviePoster,
      movieDuration: m.movieDuration,
    });

    this.bookingState.setSelectedShowtime({
      movieShowTimeId: showtime.movieShowTimeId,
      hallNumber: showtime.hallNumber,
      hallType: showtime.hallType,
      branchName: showtime.branchName,
      branchLocation: showtime.branchLocation,
      showStartTime: showtime.showStartTime,
      ticketPrice: showtime.ticketPrice,
    });

    this.router.navigate(['/movie-booking', m.movieId, 'seat-selection']);
  }
}
