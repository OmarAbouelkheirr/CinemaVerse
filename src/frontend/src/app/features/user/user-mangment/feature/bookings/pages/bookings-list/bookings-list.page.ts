import { ChangeDetectionStrategy, Component, computed, inject, OnInit, signal } from '@angular/core';
import { Router } from '@angular/router';

import { AuthStateService } from '../../../../../../../core/auth/services/auth-state.service';
import { BookingsStore } from '../../state/bookings.store';
import { BookingCardComponent } from '../../components/booking-card/booking-card.component';
import { BookingCardSkeletonComponent } from '../../components/booking-card-skeleton/booking-card-skeleton.component';
import { BookingFilterComponent } from '../../components/booking-filter/booking-filter.component';
import { BookingEmptyComponent } from '../../components/booking-empty/booking-empty.component';
import { IBookingFilters } from '../../interfaces/booking-filter.interface';

@Component({
  selector: 'app-bookings-list-page',
  standalone: true,
  imports: [
    BookingCardComponent,
    BookingCardSkeletonComponent,
    BookingFilterComponent,
    BookingEmptyComponent,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './bookings-list.page.html',
  styleUrl: './bookings-list.page.scss',
})
export class BookingsListPage implements OnInit {
  private readonly router = inject(Router);
  private readonly store = inject(BookingsStore);
  private readonly authState = inject(AuthStateService);

  readonly bookings = this.store.bookings;
  readonly loading = this.store.loading;
  readonly error = this.store.error;
  readonly filters = this.store.filters;
  readonly pagination = this.store.pagination;
  readonly isEmpty = this.store.isEmpty;
  readonly hasBookings = this.store.hasBookings;

  readonly showCancelDialog = signal(false);
  readonly bookingToCancel = signal<number | null>(null);

  readonly skeletonCount = computed(() => this.filters().pageSize);

  readonly pageNumbers = computed(() => {
    const { page, totalPages } = this.pagination();
    const pages: number[] = [];
    const start = Math.max(1, page - 2);
    const end = Math.min(totalPages, page + 2);
    for (let i = start; i <= end; i++) {
      pages.push(i);
    }
    return pages;
  });

  ngOnInit(): void {
    this.loadBookings();
  }

  onSearchChange(term: string): void {
    this.store.updateFilters({ searchTerm: term, page: 1 });
    this.loadBookings();
  }

  onFilterChange(filters: Partial<IBookingFilters>): void {
    this.store.updateFilters({ ...filters, page: 1 });
    this.loadBookings();
  }

  onClearFilters(): void {
    this.store.updateFilters({
      searchTerm: '',
      status: null,
      createdFrom: null,
      createdTo: null,
      minAmount: null,
      maxAmount: null,
      movieShowTimeId: null,
      page: 1,
    });
    this.loadBookings();
  }

  onPageChange(page: number): void {
    this.store.changePage(page);
    this.loadBookings();
  }

  onViewDetails(bookingId: number): void {
    this.router.navigate(['/my-bookings', bookingId]);
  }

  onCancelClick(bookingId: number): void {
    this.bookingToCancel.set(bookingId);
    this.showCancelDialog.set(true);
  }

  onCancelConfirm(): void {
    const bookingId = this.bookingToCancel();
    if (bookingId !== null) {
      this.store.cancelBooking(bookingId);
    }
    this.showCancelDialog.set(false);
    this.bookingToCancel.set(null);
  }

  onCancelDismiss(): void {
    this.showCancelDialog.set(false);
    this.bookingToCancel.set(null);
  }

  private loadBookings(): void {
    const userId = Number(this.authState.currentUserValue?.id);
    if (userId) {
      this.store.loadBookings(userId);
    }
  }
}
