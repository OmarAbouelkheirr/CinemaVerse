import { TicketStatus } from './ticket.interface';

/**
 * Every query parameter supported by GET /api/tickets/user/{userId}.
 * All fields are optional so partial filter updates can be merged cleanly.
 */
export interface ITicketFilters {
  status: TicketStatus | null;
  bookingId: number | null;
  showtimeId: number | null;
  ticketNumber: string;
  startDate: string | null;
  endDate: string | null;
  page: number;
  pageSize: number;
}
