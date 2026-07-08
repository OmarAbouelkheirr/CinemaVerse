import {
  ChangeDetectionStrategy,
  Component,
  OnInit,
  computed,
  inject,
  signal,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';

import { BookingStateService } from '../../state/booking-state.service';
import { PaymentService } from '../../services/payment.service';

type PaymentStatus = 'idle' | 'creating' | 'ready' | 'confirming' | 'success' | 'error';

@Component({
  selector: 'app-payment-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './payment.page.html',
  styleUrl: './payment.page.scss',
})
export class PaymentPage implements OnInit {
  private readonly router = inject(Router);
  private readonly bookingState = inject(BookingStateService);
  private readonly paymentService = inject(PaymentService);
  private readonly fb = inject(FormBuilder);

  private readonly _status = signal<PaymentStatus>('idle');
  private readonly _errorMessage = signal<string | null>(null);
  private readonly _paymentIntentId = signal<string | null>(null);
  private readonly _isForbidden = signal(false);
  private readonly _isValidating = signal(true);

  readonly paymentForm = this.fb.nonNullable.group({
    cardHolderName: ['', [Validators.required]],
    cardNumber: ['', [Validators.required, Validators.pattern(/^\d{16}$/)]],
    expiryDate: ['', [Validators.required, Validators.pattern(/^(0[1-9]|1[0-2])\/\d{2}$/)]],
    cvv: ['', [Validators.required, Validators.pattern(/^\d{3}$/)]],
  });

  readonly status = this._status.asReadonly();
  readonly errorMessage = this._errorMessage.asReadonly();
  readonly isForbidden = this._isForbidden.asReadonly();
  readonly isValidating = this._isValidating.asReadonly();

  readonly movie = this.bookingState.movie;
  readonly showtime = this.bookingState.selectedShowtime;
  readonly seats = this.bookingState.selectedSeats;
  readonly total = this.bookingState.total;
  readonly bookingId = this.bookingState.bookingId;
  readonly userId = this.bookingState.userId;

  readonly isBookingValid = computed(() => this.bookingId() !== null && this.userId() !== null);

  readonly isProcessing = computed(() => {
    const current = this._status();
    return current === 'creating' || current === 'confirming';
  });

  readonly canConfirm = computed(() => {
    const current = this._status();
    return current === 'ready' || current === 'error';
  });

  readonly seatLabels = computed(() =>
    this.seats()
      .map((seat) => seat.seatLabel)
      .join(', '),
  );

  ngOnInit(): void {
    if (!this.isBookingValid()) {
      this._errorMessage.set('Missing booking information. Please complete seat selection first.');
      this._status.set('error');
      this._isValidating.set(false);
      return;
    }

    this._isValidating.set(false);
    this.createPaymentIntent();
  }

  private createPaymentIntent(): void {
    const bookingId = this.bookingId();
    const userId = this.userId();
    const total = this.total();

    if (bookingId === null || userId === null) {
      this._errorMessage.set('Missing booking information. Please complete seat selection first.');
      this._status.set('error');
      return;
    }

    this._status.set('creating');
    this._errorMessage.set(null);
    this._isForbidden.set(false);

    const requestBody = {
      bookingId,
      amount: total,
      currency: 'EGP',
      paymentMethodType: 'Card',
    };

    this.paymentService.createIntent(userId, requestBody).subscribe({
      next: (response) => {
        this._paymentIntentId.set(response.paymentIntentId);
        this._status.set('ready');
      },
      error: (error: HttpErrorResponse) => {
        if (error.status === 403) {
          this._isForbidden.set(true);
          this._errorMessage.set('Payment authorization failed. Please contact support.');
        } else {
          this._errorMessage.set('Unable to initialize payment. Please try again.');
        }
        this._status.set('error');
      },
    });
  }

  confirmPayment(): void {
    if (this.isProcessing() || this.isForbidden()) {
      return;
    }

    if (this.paymentForm.invalid) {
      this.paymentForm.markAllAsTouched();
      return;
    }

    const bookingId = this.bookingId();
    const userId = this.userId();
    const paymentIntentId = this._paymentIntentId();

    if (bookingId === null || userId === null || paymentIntentId === null) {
      this._errorMessage.set('Payment session is incomplete. Please retry payment initialization.');
      this._status.set('error');
      return;
    }

    this._status.set('confirming');
    this._errorMessage.set(null);

    // Card form is a frontend validation layer only.
    // Backend confirm contract currently accepts only bookingId + paymentIntentId
    // in an intent/gateway-simulated payment flow.
    const requestBody = {
      bookingId,
      paymentIntentId,
    };

    this.paymentService.confirm(userId, requestBody).subscribe({
      next: (response) => {
        if (response.success === true) {
          this._status.set('success');

          const movieId = this.movie()?.movieId;
          if (movieId) {
            setTimeout(() => {
              this.router.navigate(['/movie-booking', movieId, 'success']);
            }, 700);
          }
          return;
        }

        this._status.set('error');
        this._errorMessage.set(
          response.message?.trim() || 'Payment confirmation failed. Please try again.',
        );
      },
      error: () => {
        this._status.set('error');
        this._errorMessage.set('Payment confirmation failed. Please try again or contact support.');
      },
    });
  }

  retryCreateIntent(): void {
    if (this.isProcessing()) {
      return;
    }

    this.createPaymentIntent();
  }

  redirectToCheckout(): void {
    const movieId = this.movie()?.movieId;
    if (movieId) {
      this.router.navigate(['/movie-booking', movieId, 'seat-selection']);
    } else {
      this.router.navigate(['/']);
    }
  }

  onCardNumberInput(event: Event): void {
    const input = event.target as HTMLInputElement;
    const digits = input.value.replace(/\D/g, '').slice(0, 16);
    this.paymentForm.controls.cardNumber.setValue(digits);
  }

  onExpiryInput(event: Event): void {
    const input = event.target as HTMLInputElement;
    const digits = input.value.replace(/\D/g, '').slice(0, 4);

    let formatted = digits;
    if (digits.length >= 3) {
      formatted = `${digits.slice(0, 2)}/${digits.slice(2)}`;
    }

    this.paymentForm.controls.expiryDate.setValue(formatted);
  }

  onCvvInput(event: Event): void {
    const input = event.target as HTMLInputElement;
    const digits = input.value.replace(/\D/g, '').slice(0, 3);
    this.paymentForm.controls.cvv.setValue(digits);
  }

  formatShowtimeDate(showtime: string | undefined): string {
    if (!showtime) {
      return '—';
    }

    const date = new Date(showtime);
    return date.toLocaleDateString('en-GB', {
      weekday: 'short',
      day: '2-digit',
      month: 'short',
      year: 'numeric',
    });
  }

  formatShowtimeTime(showtime: string | undefined): string {
    if (!showtime) {
      return '—';
    }

    const date = new Date(showtime);
    return date.toLocaleTimeString('en-GB', {
      hour: '2-digit',
      minute: '2-digit',
    });
  }
}
