import { IBooking } from './booking.interface';

/**
 * Paginated API response wrapper for booking lists.
 * Returned by GET /api/bookings/user/{userId}.
 */
export interface IBookingResponse {
  items: IBooking[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}
