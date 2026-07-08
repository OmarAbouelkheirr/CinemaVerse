export interface Seat {
  seatId: number;
  seatLabel: string;
  seatRow: string;
  seatColumn: number;
}

export interface HallSeatsResponse {
  hallId: number;
  hallNumber: string;
  hallType: string;
  branch: {
    branchId: number;
    branchName: string;
    branchLocation: string;
  };
  capacity: number;
  availableSeats: Seat[];
  reservedSeats: Seat[];
}

export interface SeatRow {
  rowLabel: string;
  seats: Seat[];
}

export type SeatState = 'available' | 'reserved' | 'selected';

export interface MovieInfo {
  movieId: number;
  movieName: string;
  moviePoster: string;
  movieDuration?: string;
}

export interface ShowtimeInfo {
  movieShowTimeId: number;
  hallNumber: string;
  hallType: string;
  branchName: string;
  branchLocation: string;
  showStartTime: string;
  ticketPrice: number;
}

export interface CreateBookingRequest {
  userId: number;
  movieShowTimeId: number;
  seatIds: number[];
}

export interface CreateBookingResponse {
  bookingId: number;
}
