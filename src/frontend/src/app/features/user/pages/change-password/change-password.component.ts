import {
  ChangeDetectionStrategy,
  Component,
  OnDestroy,
  OnInit,
  effect,
  inject,
  signal,
} from '@angular/core';
import {
  AbstractControl,
  NonNullableFormBuilder,
  ReactiveFormsModule,
  ValidationErrors,
  ValidatorFn,
  Validators,
} from '@angular/forms';
import { Router } from '@angular/router';

import { ChangePasswordRequest } from '../../models';
import { ProfileStore } from '../../store/profile.store';

@Component({
  selector: 'app-change-password',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './change-password.component.html',
  styleUrl: './change-password.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ChangePasswordComponent implements OnInit, OnDestroy {
  private readonly fb = inject(NonNullableFormBuilder);
  private readonly store = inject(ProfileStore);
  private readonly router = inject(Router);

  readonly loading = this.store.loading;
  readonly error = this.store.error;
  readonly changingPassword = this.store.changingPassword;

  readonly successMessage = signal<string | null>(null);

  readonly form = this.fb.group(
    {
      currentPassword: ['', [Validators.required]],
      newPassword: [
        '',
        [
          Validators.required,
          Validators.minLength(8),
          Validators.pattern(/^(?=.*[A-Za-z])(?=.*\d).{8,}$/),
        ],
      ],
      confirmPassword: ['', [Validators.required]],
    },
    { validators: [this.passwordsMatchValidator()] },
  );

  private readonly submitting = signal(false);
  private redirectTimeoutId: ReturnType<typeof setTimeout> | null = null;

  constructor() {
    effect(() => {
      if (!this.submitting()) {
        return;
      }

      if (this.changingPassword()) {
        return;
      }

      const hasError = !!this.error();
      this.submitting.set(false);

      if (hasError) {
        return;
      }

      this.successMessage.set('Password updated successfully. Redirecting to profile...');
      this.form.reset({
        currentPassword: '',
        newPassword: '',
        confirmPassword: '',
      });

      this.redirectTimeoutId = setTimeout(() => {
        void this.router.navigate(['/user/profile']);
      }, 1000);
    });
  }

  ngOnInit(): void {
    if (!this.store.profile()) {
      this.store.loadProfile();
    }
  }

  ngOnDestroy(): void {
    if (this.redirectTimeoutId) {
      clearTimeout(this.redirectTimeoutId);
      this.redirectTimeoutId = null;
    }
  }

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.successMessage.set(null);

    const payload: ChangePasswordRequest = {
      currentPassword: this.form.controls.currentPassword.getRawValue(),
      newPassword: this.form.controls.newPassword.getRawValue(),
    };

    this.submitting.set(true);
    this.store.changePassword(payload);
  }

  onCancel(): void {
    void this.router.navigate(['/user/profile']);
  }

  hasControlError(controlName: keyof typeof this.form.controls): boolean {
    const control = this.form.controls[controlName];
    return control.invalid && (control.touched || control.dirty);
  }

  hasPasswordMismatchError(): boolean {
    return (
      this.form.hasError('passwordMismatch') &&
      (this.form.controls.confirmPassword.touched || this.form.controls.confirmPassword.dirty)
    );
  }

  private passwordsMatchValidator(): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      const newPassword = control.get('newPassword')?.value as string | undefined;
      const confirmPassword = control.get('confirmPassword')?.value as string | undefined;

      if (!newPassword || !confirmPassword) {
        return null;
      }

      return newPassword === confirmPassword ? null : { passwordMismatch: true };
    };
  }
}
