import { MovieAgeRating, HallType } from './enums';
import { IMovieDuration } from './movie-duration.interface';

/** Lifecycle status of a ticket. */
export type TicketStatus = 'Active' | 'Used' | 'Cancelled';

/**
 * A single ticket belonging to a booking.
 * Returned by GET /api/tickets/user/{userId}/{ticketId}.
 */
export interface ITicket {
  ticketId: number;
  ticketNumber: string;
  movieName: string;
  showStartTime: string;
  movieDuration: IMovieDuration;
  hallNumber: string;
  hallType: HallType;
  seatLabel: string;
  moviePoster: string;
  movieAgeRating: MovieAgeRating;
  qrToken: string;
  status: TicketStatus;
  price: number;
  branchName: string;
}
