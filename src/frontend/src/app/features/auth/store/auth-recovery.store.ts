import { HttpErrorResponse } from '@angular/common/http';
import { DestroyRef, inject, Injectable, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { catchError, finalize, of, tap } from 'rxjs';

import { AuthRecoveryService } from '../data-access/auth-recovery.service';
import { ResetPasswordRequest } from '../models';

@Injectable({ providedIn: 'root' })
export class AuthRecoveryStore {
  private readonly authRecoveryService = inject(AuthRecoveryService);
  private readonly destroyRef = inject(DestroyRef);

  private readonly _loading = signal(false);
  private readonly _error = signal<string | null>(null);
  private readonly _successMessage = signal<string | null>(null);

  readonly loading = this._loading.asReadonly();
  readonly error = this._error.asReadonly();
  readonly successMessage = this._successMessage.asReadonly();

  sendForgotPassword(email: string): void {
    this.startRequest();

    this.authRecoveryService
      .forgotPassword(email)
      .pipe(
        tap(() => this._successMessage.set('Check your email')),
        catchError((error: unknown) => {
          this._error.set(this.toErrorMessage(error, 'Failed to send reset link'));
          return of(void 0);
        }),
        finalize(() => this._loading.set(false)),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe();
  }

  resetPassword(data: ResetPasswordRequest): void {
    this.startRequest();

    this.authRecoveryService
      .resetPassword(data)
      .pipe(
        tap(() => this._successMessage.set('Password reset successfully')),
        catchError((error: unknown) => {
          this._error.set(this.toErrorMessage(error, 'Failed to reset password'));
          return of(void 0);
        }),
        finalize(() => this._loading.set(false)),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe();
  }

  verifyEmail(token: string): void {
    this.startRequest();

    this.authRecoveryService
      .verifyEmail(token)
      .pipe(
        tap(() => this._successMessage.set('Email verified successfully')),
        catchError((error: unknown) => {
          this._error.set(this.toErrorMessage(error, 'Failed to verify email'));
          return of(void 0);
        }),
        finalize(() => this._loading.set(false)),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe();
  }

  resendVerification(email: string): void {
    this.startRequest();

    this.authRecoveryService
      .resendVerification(email)
      .pipe(
        tap(() => this._successMessage.set('Verification email sent')),
        catchError((error: unknown) => {
          this._error.set(this.toErrorMessage(error, 'Failed to resend verification email'));
          return of(void 0);
        }),
        finalize(() => this._loading.set(false)),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe();
  }

  private startRequest(): void {
    this._loading.set(true);
    this._error.set(null);
    this._successMessage.set(null);
  }

  private toErrorMessage(error: unknown, fallbackMessage: string): string {
    if (error instanceof HttpErrorResponse) {
      const payload = error.error;

      if (typeof payload === 'string' && payload.trim()) {
        return payload;
      }

      if (this.isRecord(payload)) {
        const message = payload['message'] ?? payload['error'] ?? payload['detail'] ?? payload['title'];
        if (typeof message === 'string' && message.trim()) {
          return message;
        }

        const errors = payload['errors'];
        if (this.isRecord(errors)) {
          const firstError = Object.values(errors)
            .flatMap((value) => (Array.isArray(value) ? value : [value]))
            .find((value) => typeof value === 'string' && value.trim());

          if (typeof firstError === 'string') {
            return firstError;
          }
        }
      }
    }

    if (error instanceof Error && error.message.trim()) {
      return error.message;
    }

    return fallbackMessage;
  }

  private isRecord(value: unknown): value is Record<string, unknown> {
    return typeof value === 'object' && value !== null && !Array.isArray(value);
  }
}
