import { IBookedSeat } from './booked-seat.interface';
import { IShowtime } from './showtime.interface';
import { ITicket } from './ticket.interface';

/** Lifecycle status of a booking. */
export type BookingStatus = 'Pending' | 'Confirmed' | 'Cancelled' | 'Expired';

/**
 * A single booking belonging to a user.
 * Returned by GET /api/bookings/user/{userId} and GET /api/bookings/{bookingId}.
 */
export interface IBooking {
  bookingId: number;
  status: BookingStatus;
  totalAmount: number;
  createdAt: string;
  expiresAt: string;
  customerName: string;
  showtime: IShowtime;
  bookedSeats: IBookedSeat[];
  tickets: ITicket[];
}
