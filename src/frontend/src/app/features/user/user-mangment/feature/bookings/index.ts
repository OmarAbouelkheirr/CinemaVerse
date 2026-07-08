// Bookings feature barrel export — public API surface for the bookings feature

// Enums / Union types
export type { HallType, MovieAgeRating } from './interfaces/enums';
export type { SortOrder } from './interfaces/booking-filter.interface';
export type { BookingStatus } from './interfaces/booking.interface';
export type { TicketStatus } from './interfaces/ticket.interface';

// Domain interfaces
export type { IBooking } from './interfaces/booking.interface';
export type { ITicket } from './interfaces/ticket.interface';
export type { IShowtime } from './interfaces/showtime.interface';
export type { IBookedSeat } from './interfaces/booked-seat.interface';
export type { IMovieDuration } from './interfaces/movie-duration.interface';

// API interfaces
export type { IBookingFilters } from './interfaces/booking-filter.interface';
export type { IBookingResponse } from './interfaces/booking-response.interface';
export type { ITicketFilters } from './interfaces/ticket-filter.interface';
export type { IPaginatedTicketResponse } from './interfaces/ticket-response.interface';

// Services
export { BookingsService } from './services/bookings.service';
export { TicketsService } from './services/tickets.service';

// State
export { BookingsStore } from './state/bookings.store';
export type { BookingsState } from './state/bookings.state';
