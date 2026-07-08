/** A single seat reserved within a booking. */
export interface IBookedSeat {
  seatId: number;
  seatLabel: string;
  seatRow: string;
  seatColumn: number;
}
