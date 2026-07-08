import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { KpiCardComponent } from '../../../../features/dashboard/components/kpi-card/kpi-card.component';
import type { BookingsStats } from '../../models/booking.model';

@Component({
  selector: 'app-bookings-stats',
  standalone: true,
  imports: [KpiCardComponent],
  templateUrl: './bookings-stats.component.html',
  styleUrl: './bookings-stats.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class BookingsStatsComponent {
  readonly stats = input.required<BookingsStats>();
}
