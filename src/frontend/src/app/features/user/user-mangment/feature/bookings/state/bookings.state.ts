import { IBooking } from '../interfaces/booking.interface';
import { IBookingFilters } from '../interfaces/booking-filter.interface';
import { ITicket } from '../interfaces/ticket.interface';
import { ITicketFilters } from '../interfaces/ticket-filter.interface';

export interface BookingsState {
  bookings: IBooking[];
  selectedBooking: IBooking | null;
  tickets: ITicket[];
  selectedTicket: ITicket | null;
  loading: boolean;
  error: string | null;
  filters: IBookingFilters;
  ticketFilters: ITicketFilters;
  pagination: {
    page: number;
    pageSize: number;
    totalCount: number;
    totalPages: number;
    hasPreviousPage: boolean;
    hasNextPage: boolean;
  };
}

export const INITIAL_BOOKINGS_STATE: BookingsState = {
  bookings: [],
  selectedBooking: null,
  tickets: [],
  selectedTicket: null,
  loading: false,
  error: null,
  filters: {
    searchTerm: '',
    status: null,
    createdFrom: null,
    createdTo: null,
    minAmount: null,
    maxAmount: null,
    page: 1,
    pageSize: 10,
    sortBy: 'createdAt',
    sortOrder: 'desc',
    movieShowTimeId: null,
  },
  ticketFilters: {
    status: null,
    bookingId: null,
    showtimeId: null,
    ticketNumber: '',
    startDate: null,
    endDate: null,
    page: 1,
    pageSize: 10,
  },
  pagination: {
    page: 1,
    pageSize: 10,
    totalCount: 0,
    totalPages: 0,
    hasPreviousPage: false,
    hasNextPage: false,
  },
};
