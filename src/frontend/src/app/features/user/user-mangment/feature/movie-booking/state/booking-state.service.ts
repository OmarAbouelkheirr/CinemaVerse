import { Injectable, signal } from '@angular/core';

import { Seat, MovieInfo, ShowtimeInfo } from '../interfaces/seat.interface';

@Injectable({ providedIn: 'root' })
export class BookingStateService {
  private readonly _movie = signal<MovieInfo | null>(null);
  private readonly _selectedShowtime = signal<ShowtimeInfo | null>(null);
  private readonly _selectedSeats = signal<Seat[]>([]);
  private readonly _bookingId = signal<number | null>(null);
  private readonly _userId = signal<number | null>(null);
  private readonly _ticketPrice = signal<number>(0);
  private readonly _subtotal = signal<number>(0);
  private readonly _total = signal<number>(0);
  private readonly _paymentIntent = signal<unknown | null>(null);

  readonly movie = this._movie.asReadonly();
  readonly selectedShowtime = this._selectedShowtime.asReadonly();
  readonly selectedSeats = this._selectedSeats.asReadonly();
  readonly bookingId = this._bookingId.asReadonly();
  readonly userId = this._userId.asReadonly();
  readonly ticketPrice = this._ticketPrice.asReadonly();
  readonly subtotal = this._subtotal.asReadonly();
  readonly total = this._total.asReadonly();
  readonly paymentIntent = this._paymentIntent.asReadonly();

  setMovie(movie: MovieInfo): void {
    this._movie.set(movie);
  }

  setSelectedShowtime(showtime: ShowtimeInfo): void {
    this._selectedShowtime.set(showtime);
    this._ticketPrice.set(showtime.ticketPrice);
  }

  setSelectedSeats(seats: Seat[]): void {
    this._selectedSeats.set(seats);
    this.recalculateTotals();
  }

  setBookingId(id: number): void {
    this._bookingId.set(id);
  }

  setUserId(id: number): void {
    this._userId.set(id);
  }

  setTicketPrice(price: number): void {
    this._ticketPrice.set(price);
    this.recalculateTotals();
  }

  setPaymentIntent(intent: unknown): void {
    this._paymentIntent.set(intent);
  }

  setSubtotal(value: number): void {
    this._subtotal.set(value);
  }

  setTotal(value: number): void {
    this._total.set(value);
  }

  private recalculateTotals(): void {
    const seats = this._selectedSeats();
    const price = this._ticketPrice();
    const subtotal = seats.length * price;
    this._subtotal.set(subtotal);
    this._total.set(subtotal);
  }

  reset(): void {
    this._movie.set(null);
    this._selectedShowtime.set(null);
    this._selectedSeats.set([]);
    this._bookingId.set(null);
    this._userId.set(null);
    this._ticketPrice.set(0);
    this._paymentIntent.set(null);
    this._subtotal.set(0);
    this._total.set(0);
  }
}
