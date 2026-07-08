import { CommonModule } from '@angular/common';
import {
  AbstractControl,
  NonNullableFormBuilder,
  ReactiveFormsModule,
  ValidationErrors,
  ValidatorFn,
  Validators,
} from '@angular/forms';
import { ChangeDetectionStrategy, Component, effect, inject, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';

import { AuthRecoveryStore } from '../../store/auth-recovery.store';

const PASSWORD_PATTERN = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$/;

@Component({
  selector: 'app-reset-password-page',
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './reset-password.page.html',
  styleUrl: './reset-password.page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ResetPasswordPage {
  private readonly fb = inject(NonNullableFormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly store = inject(AuthRecoveryStore);

  readonly loading = this.store.loading;
  readonly error = this.store.error;
  readonly successMessage = this.store.successMessage;

  readonly token = signal(this.route.snapshot.queryParamMap.get('token') ?? '');
  readonly tokenError = signal<string | null>(null);

  readonly form = this.fb.group(
    {
      newPassword: ['', [Validators.required, Validators.pattern(PASSWORD_PATTERN)]],
      confirmPassword: ['', [Validators.required]],
    },
    { validators: this.passwordsMatchValidator() },
  );

  private readonly submitted = signal(false);
  private readonly redirected = signal(false);

  constructor() {
    if (!this.token()) {
      this.tokenError.set('Reset token is missing or invalid');
    }

    effect(() => {
      if (!this.submitted() || this.redirected() || this.loading()) {
        return;
      }

      if (!this.successMessage()) {
        this.submitted.set(false);
        return;
      }

      this.redirected.set(true);
      void this.router.navigate(['/login']);
    });
  }

  submit(): void {
    this.form.markAllAsTouched();

    if (this.form.invalid || this.loading()) {
      return;
    }

    const token = this.token();
    if (!token) {
      this.tokenError.set('Reset token is missing or invalid');
      return;
    }

    this.tokenError.set(null);
    this.submitted.set(true);
    this.store.resetPassword({
      token,
      newPassword: this.form.controls.newPassword.getRawValue(),
    });
  }

  hasControlError(controlName: 'newPassword' | 'confirmPassword', errorName: string): boolean {
    const control = this.form.controls[controlName];
    return control.hasError(errorName) && (control.dirty || control.touched);
  }

  hasPasswordMismatchError(): boolean {
    const control = this.form.controls.confirmPassword;
    return this.form.hasError('passwordMismatch') && (control.dirty || control.touched);
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
