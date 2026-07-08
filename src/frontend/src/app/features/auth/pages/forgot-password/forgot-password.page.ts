import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';

import { AuthRecoveryStore } from '../../store/auth-recovery.store';

@Component({
  selector: 'app-forgot-password-page',
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './forgot-password.page.html',
  styleUrl: './forgot-password.page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ForgotPasswordPage {
  private readonly fb = inject(NonNullableFormBuilder);
  private readonly store = inject(AuthRecoveryStore);

  readonly loading = this.store.loading;
  readonly error = this.store.error;
  readonly successMessage = this.store.successMessage;

  readonly form = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
  });

  submit(): void {
    this.form.markAllAsTouched();
    if (this.form.invalid || this.loading()) {
      return;
    }

    this.store.sendForgotPassword(this.form.controls.email.getRawValue());
  }

  hasError(errorName: string): boolean {
    const control = this.form.controls.email;
    return control.hasError(errorName) && (control.dirty || control.touched);
  }
}
