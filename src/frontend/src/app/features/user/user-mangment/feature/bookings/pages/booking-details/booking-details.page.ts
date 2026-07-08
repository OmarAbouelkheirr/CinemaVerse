// BookingDetailsPage — displays full booking information with summary, tickets list, and cancel booking action
import {
  ChangeDetectionStrategy,
  Component,
  computed,
  inject,
  OnInit,
  signal,
} from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';

import { BookingsStore } from '../../state/bookings.store';
import { BookingSummaryComponent } from '../../components/booking-summary/booking-summary.component';
import { BookingTicketCardComponent } from '../../components/booking-ticket-card/booking-ticket-card.component';
import { BookingStatusBadgeComponent } from '../../components/booking-status-badge/booking-status-badge.component';
import { BookingDetailsSkeletonComponent } from '../../components/booking-details-skeleton/booking-details-skeleton.component';

@Component({
  selector: 'app-booking-details-page',
  standalone: true,
  imports: [
    BookingSummaryComponent,
    BookingTicketCardComponent,
    BookingStatusBadgeComponent,
    BookingDetailsSkeletonComponent,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './booking-details.page.html',
  styleUrl: './booking-details.page.scss',
})
export class BookingDetailsPage implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly store = inject(BookingsStore);

  readonly booking = this.store.selectedBooking;
  readonly loading = this.store.loading;
  readonly error = this.store.error;
  readonly bookingLoadState = this.store.selectedBookingLoadState;

  readonly showCancelDialog = signal(false);

  readonly bookingId = computed(() => this.booking()?.bookingId ?? null);
  readonly createdDateText = computed(() => {
    const createdAt = this.booking()?.createdAt;
    if (!createdAt) {
      return '—';
    }

    const createdDate = new Date(createdAt);
    return createdDate.toLocaleString('en-GB', {
      day: '2-digit',
      month: 'short',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  });

  readonly canCancelBooking = computed(() => {
    const status = this.booking()?.status;
    return status === 'Pending' || status === 'Confirmed';
  });

  readonly tickets = computed(() => this.booking()?.tickets ?? []);
  readonly showNotFound = computed(() => this.bookingLoadState() === 'notFound');
  readonly showGenericError = computed(() => this.bookingLoadState() === 'error');

  ngOnInit(): void {
    const bookingId = Number(this.route.snapshot.paramMap.get('bookingId'));

    if (!bookingId || Number.isNaN(bookingId)) {
      this.store.clearSelection();
      return;
    }

    this.store.clearSelection();
    this.store.loadBooking(bookingId);
  }

  onBackToBookings(): void {
    this.router.navigate(['/my-bookings']);
  }

  onViewTicket(ticketId: number): void {
    const bookingId = this.bookingId();
    if (!bookingId) {
      return;
    }

    this.router.navigate(['/my-bookings', bookingId, 'ticket', ticketId]);
  }

  onCancelClick(): void {
    this.showCancelDialog.set(true);
  }

  onCancelDismiss(): void {
    this.showCancelDialog.set(false);
  }

  onCancelConfirm(): void {
    const bookingId = this.bookingId();
    if (!bookingId) {
      this.showCancelDialog.set(false);
      return;
    }

    this.store.cancelBooking(bookingId);
    this.showCancelDialog.set(false);
  }
}
