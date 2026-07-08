import { ChangeDetectionStrategy, Component, type Signal, inject, output, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import {
  AbstractControl,
  FormBuilder,
  FormsModule,
  ReactiveFormsModule,
  ValidationErrors,
  Validators,
} from '@angular/forms';
import { filter, switchMap } from 'rxjs';
import { MovieLookupService } from './services/movie-lookup.service';
import { BranchLookupService } from './services/branch-lookup.service';
import { HallLookupService } from './services/hall-lookup.service';
import type { MovieOption } from './services/movie-option.model';
import type { BranchOption } from './services/branch-option.model';
import type { HallOption } from './services/hall-option.model';

/**
 * Payload emitted when the admin submits the create-showtime form.
 * Contains only numeric IDs (never display names) so the backend
 * receives exactly what it expects:
 *
 *   POST /api/admin/showtimes
 *   { movieId, hallId, branchId, showStartTime, price }
 */
export interface CreateShowtimePayload {
  movieId: number;
  branchId: number;
  hallId: number;
  showStartTime: string;
  price: number;
}

/**
 * Custom validator that ensures the combined date + time represents
 * a moment strictly in the future.
 *
 * Why it exists:
 *   A showtime that is in the past or at the current moment cannot be
 *   scheduled. The business rule is "only future showtimes are accepted".
 *
 * When it runs:
 *   Every time either the date or time form control value changes,
 *   Angular re-runs this validator on the form group.
 *
 * What it returns:
 *   null if valid (future datetime), or an error object
 *   { showStartTimePast: true } if the combined datetime is now or in the past.
 */
function futureShowTimeValidator(group: AbstractControl): ValidationErrors | null {
  const dateValue = group.get('date')?.value as string;
  const timeValue = group.get('time')?.value as string;

  if (!dateValue || !timeValue) {
    return null;
  }

  const showTime = new Date(`${dateValue}T${timeValue}:00`);

  if (isNaN(showTime.getTime())) {
    return { showStartTimePast: true };
  }

  const now = new Date();

  if (showTime.getTime() <= now.getTime()) {
    return { showStartTimePast: true };
  }

  return null;
}

@Component({
  selector: 'app-create-showtime-modal',
  standalone: true,
  imports: [FormsModule, ReactiveFormsModule],
  templateUrl: './create-showtime-modal.component.html',
  styleUrls: ['./create-showtime-modal.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CreateShowtimeModalComponent {
  private readonly fb = inject(FormBuilder);
  private readonly movieLookup = inject(MovieLookupService);
  private readonly branchLookup = inject(BranchLookupService);
  private readonly hallLookup = inject(HallLookupService);

  readonly closeModal = output<void>();
  readonly createShowtime = output<CreateShowtimePayload>();

  /**
   * Tracks whether the user has attempted to submit the form.
   * When true, validation error messages are shown for ALL controls
   * regardless of their individual touched/dirty state.
   *
   * Why it exists:
   *   The UX requirement is to not show validation errors before the user
   *   interacts with a control. However, if the user clicks Submit with an
   *   incomplete form, all errors should become visible at once.
   *
   * When it runs:
   *   Set to true inside onSubmit() when the form is invalid.
   */
  private readonly submitAttempted = signal(false);

  /**
   * Reactive form that stores numeric IDs for movie, branch, and hall
   * instead of free-text names. Every control is required.
   *
   * The form group has a cross-field validator (futureShowTimeValidator)
   * that ensures the combined date + time is strictly in the future.
   *
   * Price validators enforce:
   *   - required: price must be provided
   *   - min(0.1): minimum ticket price is $0.10
   *   - max(10000): maximum ticket price is $10,000
   */
  readonly form = this.fb.group(
    {
      movieId: [null as number | null, Validators.required],
      branchId: [null as number | null, Validators.required],
      hallId: [null as number | null, Validators.required],
      date: ['', Validators.required],
      time: ['', Validators.required],
      price: [
        null as number | null,
        [Validators.required, Validators.min(0.1), Validators.max(10000)],
      ],
    },
    { validators: futureShowTimeValidator },
  );

  /**
   * Cached, shared movie list loaded once from the API.
   * toSignal converts the Observable into a reactive Signal that the
   * template iterates over with @for to render the movie <select> options.
   *
   * Runs: once on component creation (subsequent subscribers get cached data).
   * Returns: Signal<MovieOption[]> — the dropdown options for movies.
   */
  readonly movies: Signal<MovieOption[]> = toSignal(this.movieLookup.getMovies(), {
    initialValue: [] as MovieOption[],
  });

  /**
   * Cached, shared branch list loaded once from the API.
   * toSignal converts the Observable into a reactive Signal that the
   * template iterates over with @for to render the branch <select> options.
   *
   * Runs: once on component creation (subsequent subscribers get cached data).
   * Returns: Signal<BranchOption[]> — the dropdown options for branches.
   */
  readonly branches: Signal<BranchOption[]> = toSignal(this.branchLookup.getBranches(), {
    initialValue: [] as BranchOption[],
  });

  /**
   * Hall list that reactively updates whenever the selected branch changes.
   *
   * Pipeline:
   *   branchId valueChanges
   *     → filter out null/0 values (no branch selected yet)
   *     → switchMap to HallLookupService.getHallsByBranch()
   *       (switchMap cancels any in-flight request when the branch changes
   *        again before the previous response arrived, preventing duplicates)
   *     → toSignal converts the resulting Observable into a Signal
   *
   * Runs: every time the branchId form control value changes to a valid ID.
   * Returns: Signal<HallOption[]> — the dropdown options for halls of the selected branch.
   */
  readonly halls: Signal<HallOption[]> = toSignal(
    this.form.controls.branchId.valueChanges.pipe(
      // Only proceed when a real branch ID is selected (ignore null resets)
      filter((branchId): branchId is number => branchId != null && branchId > 0),
      // switchMap cancels the previous hall request if the branch changes again,
      // then calls the HallLookupService to fetch halls for the new branch
      switchMap((branchId) => this.hallLookup.getHallsByBranch(branchId)),
    ),
    { initialValue: [] as HallOption[] },
  );

  constructor() {
    /**
     * When the admin changes the branch selection, the selected hall must be
     * cleared because halls are branch-specific. The hall dropdown will be
     * repopulated automatically by the `halls` signal (switchMap pipeline above).
     *
     * Runs: every time the branchId form control emits a new value.
     */
    this.form.controls.branchId.valueChanges.subscribe(() => {
      this.form.controls.hallId.reset();
    });

    /**
     * When the date or time changes, re-validate the cross-field future-showtime
     * constraint. Angular normally runs group validators on any child value change,
     * but we also explicitly trigger updateValueAndValidity on the group so the
     * showStartTimePast error is cleared/set promptly on either control's change.
     *
     * Runs: every time the date or time form control emits a new value.
     */
    this.form.controls.date.valueChanges.subscribe(() => {
      this.form.controls.time.updateValueAndValidity({ onlySelf: true });
    });
    this.form.controls.time.valueChanges.subscribe(() => {
      this.form.controls.date.updateValueAndValidity({ onlySelf: true });
    });
  }

  /**
   * Returns whether the Create button should be disabled.
   *
   * Why this is a getter instead of a computed signal:
   *   The previous implementation used `computed(() => this.form.invalid)`,
   *   but `form.invalid` is a plain property on FormGroup, NOT a signal.
   *   The computed evaluated once during construction (getting `true` because
   *   the form starts invalid), cached that value, and never re-evaluated
   *   because no signal dependency changed. This caused the button to be
   *   permanently disabled.
   *
   *   A getter is re-evaluated on every change detection cycle. Angular's
   *   reactive forms directives call markForCheck() when control values change,
   *   which triggers change detection, which re-reads this getter.
   *
   * When it runs:
   *   On every change detection cycle (triggered by form value changes).
   *
   * Returns: boolean — true when the form has validation errors.
   */
  get isFormInvalid(): boolean {
    return this.form.invalid;
  }

  /**
   * Returns true when the given form control should display a validation error.
   *
   * Validation UX rule:
   *   Errors are NOT shown before user interaction. They appear only when:
   *   - The control has been touched (user focused and blurred it), OR
   *   - The control is dirty (user changed its value), OR
   *   - The user attempted to submit the form (submitAttempted signal is true).
   *
   * When it runs:
   *   Called from the template on every change detection cycle.
   *
   * @param controlName - the name of the form control to check
   * @returns boolean — true if the control should display an error state
   */
  isInvalid(controlName: string): boolean {
    const control = this.form.get(controlName);
    if (!control || !control.invalid) {
      return false;
    }
    return control.dirty || control.touched || this.submitAttempted();
  }

  /**
   * Returns the specific error message for a given form control, or null
   * if no message should be displayed (control is valid or not yet interacted with).
   *
   * Why it exists:
   *   Each control can have multiple validation errors. This method maps
   *   error keys to user-friendly messages and respects the UX rule of
   *   only showing errors after interaction.
   *
   * When it runs:
   *   Called from the template on every change detection cycle.
   *
   * @param controlName - the name of the form control
   * @returns string | null — the error message, or null if none to show
   */
  getErrorMessage(controlName: string): string | null {
    const control = this.form.get(controlName);
    if (!control || !control.invalid) {
      return null;
    }

    const shouldShow = control.dirty || control.touched || this.submitAttempted();
    if (!shouldShow) {
      return null;
    }

    const errors = control.errors;
    if (!errors) {
      return null;
    }

    if (errors['required']) {
      return `${this.friendlyName(controlName)} is required.`;
    }
    if (errors['min']) {
      if (controlName === 'price') {
        return 'Price must be at least 0.1.';
      }
      return `Minimum value is ${errors['min'].min}.`;
    }
    if (errors['max']) {
      if (controlName === 'price') {
        return 'Price cannot exceed 10000.';
      }
      return `Maximum value is ${errors['max'].max}.`;
    }
    if (errors['showStartTimePast']) {
      return 'Showtime must be scheduled in the future.';
    }

    return null;
  }

  /**
   * Returns the cross-group error message for the future-showtime validation,
   * or null if no error should be displayed.
   *
   * Why it exists:
   *   The futureShowTimeValidator is a form-group-level validator. Its error
   *   lives on the group, not on an individual control. This method checks
   *   whether the group has the showStartTimePast error and whether the user
   *   has interacted with the date or time fields.
   *
   * When it runs:
   *   Called from the template on every change detection cycle.
   *
   * Returns: string | null — the error message, or null if none to show.
   */
  getShowTimeError(): string | null {
    const error = this.form.errors?.['showStartTimePast'];
    if (!error) {
      return null;
    }

    const dateControl = this.form.get('date');
    const timeControl = this.form.get('time');
    const shouldShow =
      (dateControl?.dirty || dateControl?.touched || this.submitAttempted()) &&
      (timeControl?.dirty || timeControl?.touched || this.submitAttempted());

    if (!shouldShow) {
      return null;
    }

    return 'Showtime must be scheduled in the future.';
  }

  /**
   * Handles the form submission. Validates the form, then constructs the
   * CreateShowtimePayload by combining the date and time into an ISO string
   * for the showStartTime field. Emits the payload via the createShowtime output.
   *
   * The final POST body is exactly:
   *   { movieId, hallId, branchId, showStartTime (ISO), price }
   * No names, no extra fields.
   *
   * Runs: when the admin clicks the Create Showtime button (or submits the form).
   */
  onSubmit(): void {
    if (this.form.invalid) {
      // Mark all controls as touched so validation errors become visible
      this.form.markAllAsTouched();
      // Set submitAttempted so cross-field errors and all error messages appear
      this.submitAttempted.set(true);
      return;
    }

    const value = this.form.getRawValue();

    // Combine separate date + time controls into a single ISO datetime string
    const showStartTime = `${value.date}T${value.time}:00`;

    this.createShowtime.emit({
      movieId: value.movieId!,
      branchId: value.branchId!,
      hallId: value.hallId!,
      showStartTime,
      price: value.price!,
    });
  }

  /**
   * Maps a form control name to a human-readable label for error messages.
   *
   * Why it exists:
   *   Control names like "movieId" are not user-friendly. This method
   *   converts them to labels like "Movie" for display in error messages.
   *
   * When it runs:
   *   Called from getErrorMessage() when building error text.
   *
   * @param controlName - the programmatic form control name
   * @returns string — a human-readable label
   */
  private friendlyName(controlName: string): string {
    const names: Record<string, string> = {
      movieId: 'Movie',
      branchId: 'Branch',
      hallId: 'Hall',
      date: 'Date',
      time: 'Time',
      price: 'Price',
    };
    return names[controlName] ?? controlName;
  }
}
