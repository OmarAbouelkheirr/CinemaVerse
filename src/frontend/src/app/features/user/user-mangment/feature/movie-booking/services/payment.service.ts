import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { ApiClientService } from '../../../../../../core/http/api-client.service';
import {
  CreatePaymentIntentRequest,
  PaymentIntentResponse,
  ConfirmPaymentRequest,
  ConfirmPaymentResponse,
  RefundRequest,
  RefundResponse,
} from '../interfaces/payment.interface';

@Injectable({ providedIn: 'root' })
export class PaymentService {
  private readonly apiClient = inject(ApiClientService);

  createIntent(userId: number, request: CreatePaymentIntentRequest): Observable<PaymentIntentResponse> {
    return this.apiClient.post<PaymentIntentResponse, CreatePaymentIntentRequest>(
      `/api/payments/user/${userId}/intent`,
      request
    );
  }

  confirm(userId: number, request: ConfirmPaymentRequest): Observable<ConfirmPaymentResponse> {
    return this.apiClient.post<ConfirmPaymentResponse, ConfirmPaymentRequest>(
      `/api/payments/user/${userId}/confirm`,
      request
    );
  }

  refund(userId: number, request: RefundRequest): Observable<RefundResponse> {
    return this.apiClient.post<RefundResponse, RefundRequest>(
      `/api/payments/user/${userId}/refund`,
      request
    );
  }
}
