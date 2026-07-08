import { computed, DestroyRef, inject, Injectable, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { catchError, finalize, of, tap } from 'rxjs';

import { ProfileService } from '../data-access/profile.service';
import { ChangePasswordRequest, UpdateProfileRequest, UserProfile } from '../models';

@Injectable({ providedIn: 'root' })
export class ProfileStore {
  private readonly profileService = inject(ProfileService);
  private readonly destroyRef = inject(DestroyRef);

  private readonly _profile = signal<UserProfile | null>(null);
  private readonly _loading = signal(false);
  private readonly _error = signal<string | null>(null);
  private readonly _saving = signal(false);
  private readonly _changingPassword = signal(false);

  readonly profile = this._profile.asReadonly();
  readonly loading = this._loading.asReadonly();
  readonly error = this._error.asReadonly();
  readonly saving = this._saving.asReadonly();
  readonly changingPassword = this._changingPassword.asReadonly();

  readonly fullName = computed(() => {
    const profile = this._profile();
    if (!profile) {
      return '';
    }

    return `${profile.firstName} ${profile.lastName}`.trim();
  });

  readonly isEmailVerified = computed(() => this._profile()?.isEmailConfirmed ?? false);
  readonly hasProfile = computed(() => this._profile() !== null);

  loadProfile(): void {
    this._loading.set(true);
    this._error.set(null);

    this.profileService
      .getProfile()
      .pipe(
        tap((response) => this._profile.set(response)),
        catchError((error: unknown) => {
          this._error.set(this.toErrorMessage(error, 'Failed to load profile'));
          return of(null);
        }),
        finalize(() => this._loading.set(false)),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe();
  }

  updateProfile(data: UpdateProfileRequest): void {
    this._saving.set(true);
    this._error.set(null);

    this.profileService
      .updateProfile(data)
      .pipe(
        tap(() => {
          this._profile.update((current) => (current ? { ...current, ...data } : current));
        }),
        catchError((error: unknown) => {
          this._error.set(this.toErrorMessage(error, 'Failed to update profile'));
          return of(null);
        }),
        finalize(() => this._saving.set(false)),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe();
  }

  changePassword(data: ChangePasswordRequest): void {
    this._changingPassword.set(true);
    this._error.set(null);

    this.profileService
      .changePassword(data)
      .pipe(
        catchError((error: unknown) => {
          this._error.set(this.toErrorMessage(error, 'Failed to change password'));
          return of(null);
        }),
        finalize(() => this._changingPassword.set(false)),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe();
  }

  private toErrorMessage(error: unknown, fallbackMessage: string): string {
    if (error instanceof Error && error.message.trim()) {
      return error.message;
    }

    return fallbackMessage;
  }
}
