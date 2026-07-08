import { ChangeDetectionStrategy, Component, OnInit, effect, inject, signal } from '@angular/core';
import {
  AbstractControl,
  NonNullableFormBuilder,
  ReactiveFormsModule,
  ValidationErrors,
  ValidatorFn,
  Validators,
} from '@angular/forms';
import { Router } from '@angular/router';

import { ProfileStore } from '../../store/profile.store';
import { UpdateProfileRequest, UserGender } from '../../models';

@Component({
  selector: 'app-edit-profile',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './edit-profile.component.html',
  styleUrl: './edit-profile.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class EditProfileComponent implements OnInit {
  private readonly fb = inject(NonNullableFormBuilder);
  private readonly store = inject(ProfileStore);
  private readonly router = inject(Router);

  readonly profile = this.store.profile;
  readonly loading = this.store.loading;
  readonly saving = this.store.saving;
  readonly error = this.store.error;

  readonly genderOptions = [UserGender.Male, UserGender.Female] as const;

  readonly form = this.fb.group({
    firstName: ['', [Validators.required, Validators.maxLength(100)]],
    lastName: ['', [Validators.required, Validators.maxLength(100)]],
    phoneNumber: ['', [Validators.required, Validators.pattern(/^[0-9+()\-\s]{7,20}$/)]],
    address: ['', [Validators.required, Validators.maxLength(250)]],
    city: ['', [Validators.required, Validators.maxLength(100)]],
    dateOfBirth: [
      '',
      [Validators.required, Validators.pattern(/^\d{4}-\d{2}-\d{2}$/), this.validDateValidator()],
    ],
    gender: [UserGender.Male as UserGender, [Validators.required]],
  });

  private readonly submitting = signal(false);
  private lastPatchedUserId: number | null = null;

  constructor() {
    effect(() => {
      const profile = this.profile();
      if (!profile || this.lastPatchedUserId === profile.userId) {
        return;
      }

      this.form.setValue({
        firstName: profile.firstName,
        lastName: profile.lastName,
        phoneNumber: profile.phoneNumber,
        address: profile.address,
        city: profile.city,
        dateOfBirth: this.toDateInputValue(profile.dateOfBirth),
        gender: profile.gender,
      });

      this.lastPatchedUserId = profile.userId;
    });

    effect(() => {
      if (!this.submitting()) {
        return;
      }

      if (this.saving()) {
        return;
      }

      const hasError = !!this.error();
      this.submitting.set(false);

      if (!hasError) {
        void this.router.navigate(['/user/profile']);
      }
    });
  }

  ngOnInit(): void {
    if (!this.profile()) {
      this.store.loadProfile();
    }
  }

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const payload: UpdateProfileRequest = this.form.getRawValue();
    this.submitting.set(true);
    this.store.updateProfile(payload);
  }

  onCancel(): void {
    void this.router.navigate(['/user/profile']);
  }

  hasControlError(controlName: keyof typeof this.form.controls): boolean {
    const control = this.form.controls[controlName];
    return control.invalid && (control.touched || control.dirty);
  }

  private toDateInputValue(value: string): string {
    return value.slice(0, 10);
  }

  private validDateValidator(): ValidatorFn {
    return (control: AbstractControl<string>): ValidationErrors | null => {
      const value = control.value;
      if (!value) {
        return null;
      }

      const date = new Date(value);
      if (Number.isNaN(date.getTime())) {
        return { invalidDate: true };
      }

      const [year, month, day] = value.split('-').map(Number);
      if (!year || !month || !day) {
        return { invalidDate: true };
      }

      const isExactDate =
        date.getUTCFullYear() === year &&
        date.getUTCMonth() + 1 === month &&
        date.getUTCDate() === day;

      return isExactDate ? null : { invalidDate: true };
    };
  }
}
