import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { ApiClientService } from '../../../../../../core/http/api-client.service';
import { IBooking } from '../interfaces/booking.interface';
import { IBookingFilters } from '../interfaces/booking-filter.interface';
import { IBookingResponse } from '../interfaces/booking-response.interface';
import { buildParams } from './build-params';

@Injectable({ providedIn: 'root' })
export class BookingsService {
  private readonly apiClient = inject(ApiClientService);
  private readonly api = '/api/bookings';

  getBookings(userId: number, filters: IBookingFilters): Observable<IBookingResponse> {
    return this.apiClient.get<IBookingResponse>(
      `${this.api}/user/${userId}`,
      buildParams(filters),
    );
  }

  getBookingById(bookingId: number): Observable<IBooking> {
    return this.apiClient.get<IBooking>(`${this.api}/${bookingId}`);
  }

  cancelBooking(bookingId: number): Observable<boolean> {
    return this.apiClient.delete<boolean>(`${this.api}/${bookingId}`);
  }
}
