import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { ApiClientService } from '../../../core/http/api-client.service';
import { ForgotPasswordRequest, ResendVerificationRequest, ResetPasswordRequest } from '../models';

@Injectable({ providedIn: 'root' })
export class AuthRecoveryService {
  private readonly apiClient = inject(ApiClientService);

  forgotPassword(email: string): Observable<void> {
    const body: ForgotPasswordRequest = { email };
    return this.apiClient.post<void, ForgotPasswordRequest>('/api/auth/forgot-password', body);
  }

  resetPassword(data: ResetPasswordRequest): Observable<void> {
    return this.apiClient.post<void, ResetPasswordRequest>('/api/auth/reset-password', data);
  }

  verifyEmail(token: string): Observable<void> {
    return this.apiClient.get<void>('/api/auth/verify-email', { token });
  }

  resendVerification(email: string): Observable<void> {
    const body: ResendVerificationRequest = { email };
    return this.apiClient.post<void, ResendVerificationRequest>('/api/auth/resend-verification', body);
  }
}
