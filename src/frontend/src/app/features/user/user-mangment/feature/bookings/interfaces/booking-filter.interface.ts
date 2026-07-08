import { BookingStatus } from './booking.interface';

/** Sort direction for paginated queries. */
export type SortOrder = 'asc' | 'desc';

/**
 * Every query parameter supported by GET /api/bookings/user/{userId}.
 * All fields are optional so partial filter updates can be merged cleanly.
 */
export interface IBookingFilters {
  searchTerm: string;
  status: BookingStatus | null;
  createdFrom: string | null;
  createdTo: string | null;
  minAmount: number | null;
  maxAmount: number | null;
  page: number;
  pageSize: number;
  sortBy: string;
  sortOrder: SortOrder;
  movieShowTimeId: number | null;
}
