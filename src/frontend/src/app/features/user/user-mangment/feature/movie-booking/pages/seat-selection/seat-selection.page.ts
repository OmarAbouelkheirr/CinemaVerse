import {
  ChangeDetectionStrategy,
  Component,
  OnInit,
  computed,
  inject,
  signal,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { finalize } from 'rxjs/operators';

import { BookingStateService } from '../../state/booking-state.service';
import { SeatSelectionService } from '../../services/seat-selection.service';
import { BookingService } from '../../services/booking.service';
import { AuthService } from '../../../../../../../core/auth/services/auth.service';
import { AuthStateService } from '../../../../../../../core/auth/services/auth-state.service';
import {
  Seat,
  HallSeatsResponse,
  MovieInfo,
  ShowtimeInfo,
  CreateBookingResponse,
} from '../../interfaces/seat.interface';
import { ScreenHeaderComponent } from '../../components/screen-header/screen-header.component';
import { SeatLegendComponent } from '../../components/seat-legend/seat-legend.component';
import { SeatGridComponent } from '../../components/seat-grid/seat-grid.component';
import { BookingSummaryComponent } from '../../components/booking-summary/booking-summary.component';
import { MovieInfoCardComponent } from '../../components/movie-info-card/movie-info-card.component';
import { ContinueButtonComponent } from '../../components/continue-button/continue-button.component';

@Component({
  selector: 'app-seat-selection-page',
  standalone: true,
  imports: [
    CommonModule,
    ScreenHeaderComponent,
    SeatLegendComponent,
    SeatGridComponent,
    BookingSummaryComponent,
    MovieInfoCardComponent,
    ContinueButtonComponent,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="page">
      <header class="page__header">
        <h1 class="page__title">Seat Selection</h1>
        @if (showtimeInfo(); as st) {
          <p class="page__subtitle">
            {{ st.branchName }} &middot; Hall {{ st.hallNumber }} &middot; {{ st.hallType }}
          </p>
        }
      </header>

      @if (loading()) {
        <div class="page__loading">
          <span class="material-symbols-outlined page__spinner">progress_activity</span>
          <p>Loading seat layout...</p>
        </div>
      } @else if (error(); as errorMsg) {
        <div class="page__error">
          <span class="material-symbols-outlined">error_outline</span>
          <p>{{ errorMsg }}</p>
        </div>
      } @else if (hallSeats(); as hall) {
        <div class="page__layout">
          <div class="page__main">
            @if (movieInfo(); as mi) {
              <div class="page__movie-mobile">
                <app-movie-info-card [movie]="mi" />
              </div>
            }

            <div class="page__grid-container">
              <app-screen-header />
              <app-seat-legend />
              <app-seat-grid
                [availableSeats]="hall.availableSeats"
                [reservedSeats]="hall.reservedSeats"
                [selectedSeats]="selectedSeats()"
                (seatSelected)="onSeatToggle($event)"
              />
            </div>
          </div>

          <div class="page__sidebar">
            <div class="page__sidebar-sticky">
              @if (movieInfo(); as mi) {
                <app-movie-info-card [movie]="mi" />
              }

              <app-booking-summary
                [selectedSeats]="selectedSeats()"
                [ticketPrice]="ticketPrice()"
                [showtime]="showtimeInfo()"
              />

              <app-continue-button
                [disabled]="selectedSeats().length === 0 || isSubmitting()"
                [loading]="isSubmitting()"
                (continueClick)="onContinue()"
              />
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

      .page__header {
        margin-bottom: 2rem;
      }

      .page__title {
        font-size: var(--text-page);
        font-weight: 700;
        color: var(--on-surface);
        margin: 0 0 0.25rem 0;
      }

      .page__subtitle {
        font-size: var(--text-body);
        color: var(--on-surface-muted);
        margin: 0;
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
        gap: 1.5rem;
      }

      .page__movie-mobile {
        display: none;
      }

      .page__grid-container {
        display: flex;
        flex-direction: column;
        align-items: center;
        padding: 2rem;
        background: var(--surface-container-low);
        border: 1px solid var(--ghost-border);
        border-radius: var(--radius-lg);
      }

      .page__sidebar-sticky {
        position: sticky;
        top: calc(var(--nav-height, 3.5rem) + 1rem);
        display: flex;
        flex-direction: column;
        gap: 1rem;
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

        .page__grid-container {
          padding: 1rem;
        }

        .page__movie-mobile {
          display: block;
        }

        .page__sidebar > .page__sidebar-sticky > app-movie-info-card {
          display: none;
        }
      }
    `,
  ],
})
export class SeatSelectionPage implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly bookingState = inject(BookingStateService);
  private readonly seatSelectionService = inject(SeatSelectionService);
  private readonly bookingService = inject(BookingService);
  private readonly authService = inject(AuthService);
  private readonly authState = inject(AuthStateService);

  readonly hallSeats = signal<HallSeatsResponse | null>(null);
  readonly loading = signal(false);
  readonly error = signal<string | null>(null);
  readonly selectedSeats = signal<Seat[]>([]);
  readonly isSubmitting = signal(false);

  readonly movieInfo = this.bookingState.movie;
  readonly showtimeInfo = this.bookingState.selectedShowtime;

  readonly ticketPrice = computed(() => {
    return this.showtimeInfo()?.ticketPrice ?? 0;
  });

  ngOnInit(): void {
    const showtime = this.bookingState.selectedShowtime();
    if (showtime) {
      this.fetchHallSeats(showtime.movieShowTimeId);
    } else {
      const idParam = this.route.snapshot.paramMap.get('id');
      if (idParam) {
        this.fetchHallSeats(Number(idParam));
      } else {
        this.error.set('No showtime selected. Please go back and select a showtime.');
      }
    }
  }

  fetchHallSeats(movieShowTimeId: number): void {
    this.loading.set(true);
    this.error.set(null);

    this.seatSelectionService.getHallSeats(movieShowTimeId).subscribe({
      next: (response) => {
        this.hallSeats.set(response);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Failed to load seat layout. Please try again.');
        this.loading.set(false);
      },
    });
  }

  onSeatToggle(seat: Seat): void {
    const current = this.selectedSeats();
    const isAlreadySelected = current.some((s) => s.seatId === seat.seatId);

    if (isAlreadySelected) {
      this.selectedSeats.set(current.filter((s) => s.seatId !== seat.seatId));
    } else {
      this.selectedSeats.set([...current, seat]);
    }
  }

  onContinue(): void {
    if (this.isSubmitting()) {
      return;
    }

    console.log('[SeatSelectionPage] onContinue() called');
    console.log('[SeatSelectionPage] selectedSeats:', this.selectedSeats());
    console.log('[SeatSelectionPage] selectedSeats.length:', this.selectedSeats().length);

    const seats = this.selectedSeats();
    if (seats.length === 0) {
      console.log('[SeatSelectionPage] No seats selected, returning early');
      return;
    }

    const showtime = this.showtimeInfo();
    if (!showtime) {
      console.log('[SeatSelectionPage] No showtime selected, returning early');
      this.error.set('No showtime selected. Please go back and select a showtime.');
      return;
    }

    console.log('[SeatSelectionPage] Checking auth state...');
    console.log('[SeatSelectionPage] authState.currentUserValue:', this.authState.currentUserValue);
    console.log(
      '[SeatSelectionPage] authState.isAuthenticatedValue:',
      this.authState.isAuthenticatedValue,
    );

    const currentUser = this.authState.currentUserValue;
    if (!currentUser?.id) {
      console.log('[SeatSelectionPage] FAILED: currentUser is null or has no id');
      console.log('[SeatSelectionPage] currentUser:', currentUser);
      console.log('[SeatSelectionPage] currentUser?.id:', currentUser?.id);
      this.error.set('User not authenticated. Please log in again.');
      return;
    }

    console.log('[SeatSelectionPage] Auth check passed, currentUser.id:', currentUser.id);

    const userId = Number(currentUser.id);
    const seatIds = seats.map((s) => s.seatId);

    this.bookingState.setSelectedSeats(seats);
    this.bookingState.setTicketPrice(this.ticketPrice());

    this.isSubmitting.set(true);

    this.bookingService
      .createBooking({ userId, movieShowTimeId: showtime.movieShowTimeId, seatIds })
      .pipe(finalize(() => this.isSubmitting.set(false)))
      .subscribe({
        next: (response: CreateBookingResponse) => {
          this.bookingState.setBookingId(response.bookingId);
          this.bookingState.setUserId(userId);

          const movieId = this.movieInfo()?.movieId;
          if (movieId) {
            this.router.navigate(['/movie-booking', movieId, 'payment']);
          }
        },
        error: () => {
          this.error.set('Failed to create booking. Please try again.');
        },
      });
  }
}
