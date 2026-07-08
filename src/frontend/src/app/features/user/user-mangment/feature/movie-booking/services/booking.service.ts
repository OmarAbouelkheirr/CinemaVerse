import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { ApiClientService } from '../../../../../../core/http/api-client.service';
import { CreateBookingRequest, CreateBookingResponse } from '../interfaces/seat.interface';

@Injectable({ providedIn: 'root' })
export class BookingService {
  private readonly apiClient = inject(ApiClientService);

  createBooking(request: CreateBookingRequest): Observable<CreateBookingResponse> {
    return this.apiClient.post<CreateBookingResponse, CreateBookingRequest>(
      '/api/bookings',
      request,
    );
  }
}
