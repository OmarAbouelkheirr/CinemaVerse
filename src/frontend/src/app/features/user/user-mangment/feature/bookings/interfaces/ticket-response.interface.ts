import { ITicket } from './ticket.interface';

/**
 * Paginated API response wrapper for ticket lists.
 * Returned by GET /api/tickets/user/{userId}.
 */
export interface IPaginatedTicketResponse {
  items: ITicket[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}
