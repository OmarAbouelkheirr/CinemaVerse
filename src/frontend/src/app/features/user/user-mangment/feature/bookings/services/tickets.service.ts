import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { ApiClientService } from '../../../../../../core/http/api-client.service';
import { ITicket } from '../interfaces/ticket.interface';
import { ITicketFilters } from '../interfaces/ticket-filter.interface';
import { IPaginatedTicketResponse } from '../interfaces/ticket-response.interface';
import { buildParams } from './build-params';

@Injectable({ providedIn: 'root' })
export class TicketsService {
  private readonly apiClient = inject(ApiClientService);
  private readonly api = '/api/tickets';

  getUserTickets(userId: number, filters: ITicketFilters): Observable<IPaginatedTicketResponse> {
    return this.apiClient.get<IPaginatedTicketResponse>(
      `${this.api}/user/${userId}`,
      buildParams(filters),
    );
  }

  getTicketById(userId: number, ticketId: number): Observable<ITicket> {
    return this.apiClient.get<ITicket>(`${this.api}/user/${userId}/${ticketId}`);
  }
}
