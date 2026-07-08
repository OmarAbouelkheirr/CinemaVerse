import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { ApiClientService } from '../../../../../../core/http/api-client.service';
import { HallSeatsResponse } from '../interfaces/seat.interface';

@Injectable({ providedIn: 'root' })
export class SeatSelectionService {
  private readonly apiClient = inject(ApiClientService);

  getHallSeats(movieShowTimeId: number): Observable<HallSeatsResponse> {
    return this.apiClient.get<HallSeatsResponse>(`/api/showtimes/${movieShowTimeId}/hall-seats`);
  }
}
