import { ChangeDetectionStrategy, Component, input, output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { IBookingFilters } from '../../interfaces/booking-filter.interface';
import { BookingStatus } from '../../interfaces/booking.interface';

@Component({
  selector: 'app-booking-filter',
  standalone: true,
  imports: [FormsModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './booking-filter.component.html',
  styleUrls: ['./booking-filter.component.scss'],
})
export class BookingFilterComponent {
  readonly currentFilters = input.required<IBookingFilters>();
  readonly searchChanged = output<string>();
  readonly filterChanged = output<Partial<IBookingFilters>>();
  readonly clearFilters = output<void>();

  readonly showFilters = signal<boolean>(false);

  toggleFilters(): void {
    this.showFilters.update((current) => !current);
  }

  onSearchChange(term: string): void {
    this.searchChanged.emit(term);
  }

  onStatusChange(status: BookingStatus | null): void {
    this.filterChanged.emit({ status });
  }

  onDateFromChange(date: string): void {
    this.filterChanged.emit({ createdFrom: date || null });
  }

  onDateToChange(date: string): void {
    this.filterChanged.emit({ createdTo: date || null });
  }
}
