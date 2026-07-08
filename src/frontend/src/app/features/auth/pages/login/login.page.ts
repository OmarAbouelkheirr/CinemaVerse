import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { AuthService } from '../../../../core/auth/services/auth.service';

@Component({
  selector: 'app-login-page',
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './login.page.html',
  styleUrl: './login.page.scss',
})
export class LoginPage {
  private readonly fb = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  readonly isLoading = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly errorCode = signal<string | null>(null);
  readonly showPassword = signal(false);

  readonly canResendVerification = computed(() => this.isEmailNotConfirmedCode(this.errorCode()));

  readonly form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required]],
  });

  submit(): void {
    this.errorMessage.set(null);
    this.errorCode.set(null);
    this.form.markAllAsTouched();

    if (this.form.invalid || this.isLoading()) {
      return;
    }

    this.isLoading.set(true);
    this.authService
      .login(this.form.getRawValue())
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: () => {
          const destination = this.authService.isAdmin()
            ? '/admin/dashboard'
            : this.authService.isUser()
              ? '/user/dashboard'
              : '/unauthorized';

          void this.router.navigateByUrl(destination);
        },
        error: (error: unknown) => {
          const details = this.getErrorDetails(error, 'Invalid email or password');
          this.errorMessage.set(details.message);
          this.errorCode.set(details.code);
        },
      });
  }

  togglePassword(): void {
    this.showPassword.update((value) => !value);
  }

  hasError(controlName: 'email' | 'password', errorName: string): boolean {
    const control = this.form.controls[controlName];
    return control.hasError(errorName) && (control.dirty || control.touched);
  }

  private getErrorDetails(
    error: unknown,
    fallback: string,
  ): { message: string; code: string | null } {
    if (!(error instanceof HttpErrorResponse)) {
      return { message: fallback, code: null };
    }

    const apiError: unknown = error.error;
    if (typeof apiError === 'string') {
      return { message: apiError, code: null };
    }

    if (this.isRecord(apiError)) {
      const code = this.readString(apiError, 'code') ?? this.readString(apiError, 'errorCode');
      const message =
        this.readString(apiError, 'message') ??
        this.readString(apiError, 'error') ??
        this.readString(apiError, 'detail') ??
        this.readString(apiError, 'title');

      if (message) {
        return { message, code };
      }

      const errors = apiError['errors'];
      if (this.isRecord(errors)) {
        const firstError = Object.values(errors)
          .flatMap((value) => (Array.isArray(value) ? value : [value]))
          .find((value) => typeof value === 'string' && value.trim());

        if (typeof firstError === 'string') {
          return { message: firstError, code };
        }
      }

      return { message: fallback, code };
    }

    return { message: fallback, code: null };
  }

  private isEmailNotConfirmedCode(code: string | null): boolean {
    if (!code) {
      return false;
    }

    const normalized = code.toLowerCase();
    return normalized.includes('email') && normalized.includes('confirm');
  }

  private readString(record: Record<string, unknown>, key: string): string | null {
    const value = record[key];
    if (typeof value !== 'string') {
      return null;
    }

    const trimmed = value.trim();
    return trimmed ? trimmed : null;
  }

  private isRecord(value: unknown): value is Record<string, unknown> {
    return typeof value === 'object' && value !== null && !Array.isArray(value);
  }
}
