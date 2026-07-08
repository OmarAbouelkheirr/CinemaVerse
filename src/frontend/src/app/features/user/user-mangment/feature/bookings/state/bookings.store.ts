import { HttpErrorResponse } from '@angular/common/http';
import { computed, DestroyRef, inject, Injectable, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { catchError, finalize, tap } from 'rxjs/operators';
import { of } from 'rxjs';

import { BookingsService } from '../services/bookings.service';
import { TicketsService } from '../services/tickets.service';
import { IBooking } from '../interfaces/booking.interface';
import { IBookingFilters } from '../interfaces/booking-filter.interface';
import { ITicket } from '../interfaces/ticket.interface';
import { ITicketFilters } from '../interfaces/ticket-filter.interface';
import { BookingsState, INITIAL_BOOKINGS_STATE } from './bookings.state';

@Injectable({ providedIn: 'root' })
export class BookingsStore {
  private readonly bookingsService = inject(BookingsService);
  private readonly ticketsService = inject(TicketsService);
  private readonly destroyRef = inject(DestroyRef);

  private readonly _bookings = signal<IBooking[]>(INITIAL_BOOKINGS_STATE.bookings);
  private readonly _selectedBooking = signal<IBooking | null>(
    INITIAL_BOOKINGS_STATE.selectedBooking,
  );
  private readonly _tickets = signal<ITicket[]>(INITIAL_BOOKINGS_STATE.tickets);
  private readonly _selectedTicket = signal<ITicket | null>(INITIAL_BOOKINGS_STATE.selectedTicket);
  private readonly _loading = signal<boolean>(INITIAL_BOOKINGS_STATE.loading);
  private readonly _error = signal<string | null>(INITIAL_BOOKINGS_STATE.error);
  private readonly _filters = signal<IBookingFilters>(INITIAL_BOOKINGS_STATE.filters);
  private readonly _ticketFilters = signal<ITicketFilters>(INITIAL_BOOKINGS_STATE.ticketFilters);
  private readonly _pagination = signal(INITIAL_BOOKINGS_STATE.pagination);
  private readonly _selectedBookingLoadState = signal<
    'idle' | 'loading' | 'success' | 'notFound' | 'error'
  >('idle');

  readonly bookings = this._bookings.asReadonly();
  readonly selectedBooking = this._selectedBooking.asReadonly();
  readonly tickets = this._tickets.asReadonly();
  readonly selectedTicket = this._selectedTicket.asReadonly();
  readonly loading = this._loading.asReadonly();
  readonly error = this._error.asReadonly();
  readonly filters = this._filters.asReadonly();
  readonly ticketFilters = this._ticketFilters.asReadonly();
  readonly pagination = this._pagination.asReadonly();
  readonly selectedBookingLoadState = this._selectedBookingLoadState.asReadonly();

  readonly hasBookings = computed(() => this._bookings().length > 0);
  readonly isEmpty = computed(() => this._bookings().length === 0);
  readonly totalBookings = computed(() => this._bookings().length);
  readonly confirmedBookings = computed(() =>
    this._bookings().filter((b) => b.status === 'Confirmed'),
  );
  readonly pendingBookings = computed(() => this._bookings().filter((b) => b.status === 'Pending'));
  readonly cancelledBookings = computed(() =>
    this._bookings().filter((b) => b.status === 'Cancelled'),
  );
  readonly expiredBookings = computed(() => this._bookings().filter((b) => b.status === 'Expired'));

  loadBookings(userId: number): void {
    this.clearError();
    this.setLoading(true);
    const filters = this._filters();

    this.bookingsService
      .getBookings(userId, filters)
      .pipe(
        tap((response) => {
          this._bookings.set(response.items);
          this._pagination.set({
            page: response.page,
            pageSize: response.pageSize,
            totalCount: response.totalCount,
            totalPages: response.totalPages,
            hasPreviousPage: response.hasPreviousPage,
            hasNextPage: response.hasNextPage,
          });
        }),
        catchError((error) => {
          this.setError(error.message || 'Failed to load bookings');
          return of(null);
        }),
        finalize(() => this.setLoading(false)),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe();
  }

  loadBooking(bookingId: number): void {
    this.clearError();
    this._selectedBookingLoadState.set('loading');
    this.setLoading(true);

    this.bookingsService
      .getBookingById(bookingId)
      .pipe(
        tap((booking) => {
          this._selectedBooking.set(booking);
          this._selectedBookingLoadState.set('success');
        }),
        catchError((error: unknown) => {
          this._selectedBooking.set(null);

          if (error instanceof HttpErrorResponse && error.status === 404) {
            this._selectedBookingLoadState.set('notFound');
            this.setError('Booking not found');
            return of(null);
          }

          this._selectedBookingLoadState.set('error');
          this.setError(error instanceof Error ? error.message : 'Failed to load booking details');
          return of(null);
        }),
        finalize(() => this.setLoading(false)),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe();
  }

  loadUserTickets(userId: number): void {
    this.clearError();
    this.setLoading(true);
    const filters = this._ticketFilters();

    this.ticketsService
      .getUserTickets(userId, filters)
      .pipe(
        tap((response) => this._tickets.set(response.items)),
        catchError((error) => {
          this.setError(error.message || 'Failed to load tickets');
          return of(null);
        }),
        finalize(() => this.setLoading(false)),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe();
  }

  loadTicket(userId: number, ticketId: number): void {
    this.clearError();
    this.setLoading(true);

    this.ticketsService
      .getTicketById(userId, ticketId)
      .pipe(
        tap((ticket) => this._selectedTicket.set(ticket)),
        catchError((error) => {
          this.setError(error.message || 'Failed to load ticket details');
          return of(null);
        }),
        finalize(() => this.setLoading(false)),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe();
  }

  cancelBooking(bookingId: number): void {
    this.clearError();
    this.setLoading(true);

    this.bookingsService
      .cancelBooking(bookingId)
      .pipe(
        tap(() => {
          this._bookings.update((bookings) =>
            bookings.map((b) =>
              b.bookingId === bookingId ? { ...b, status: 'Cancelled' as const } : b,
            ),
          );
          if (this._selectedBooking()?.bookingId === bookingId) {
            this._selectedBooking.update((booking) =>
              booking ? { ...booking, status: 'Cancelled' as const } : null,
            );
          }
        }),
        catchError((error) => {
          this.setError(error.message || 'Failed to cancel booking');
          return of(false);
        }),
        finalize(() => this.setLoading(false)),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe();
  }

  updateFilters(filters: Partial<IBookingFilters>): void {
    this._filters.update((current) => ({ ...current, ...filters }));
  }

  updateTicketFilters(filters: Partial<ITicketFilters>): void {
    this._ticketFilters.update((current) => ({ ...current, ...filters }));
  }

  changePage(page: number): void {
    this._filters.update((current) => ({ ...current, page }));
  }

  changePageSize(pageSize: number): void {
    this._filters.update((current) => ({ ...current, pageSize, page: 1 }));
  }

  clearSelection(): void {
    this._selectedBooking.set(null);
    this._selectedTicket.set(null);
    this._selectedBookingLoadState.set('idle');
  }

  reset(): void {
    this._bookings.set(INITIAL_BOOKINGS_STATE.bookings);
    this._selectedBooking.set(INITIAL_BOOKINGS_STATE.selectedBooking);
    this._tickets.set(INITIAL_BOOKINGS_STATE.tickets);
    this._selectedTicket.set(INITIAL_BOOKINGS_STATE.selectedTicket);
    this._loading.set(INITIAL_BOOKINGS_STATE.loading);
    this._error.set(INITIAL_BOOKINGS_STATE.error);
    this._filters.set(INITIAL_BOOKINGS_STATE.filters);
    this._ticketFilters.set(INITIAL_BOOKINGS_STATE.ticketFilters);
    this._pagination.set(INITIAL_BOOKINGS_STATE.pagination);
    this._selectedBookingLoadState.set('idle');
  }

  private setLoading(loading: boolean): void {
    this._loading.set(loading);
  }

  private setError(message: string): void {
    this._error.set(message);
  }

  private clearError(): void {
    this._error.set(null);
  }
}
