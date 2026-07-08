import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { KpiCardComponent } from '../../../../../../features/dashboard/components/kpi-card/kpi-card.component';
import { ShowtimeKpiItem, ShowtimeKpiService } from '../services/showtime-kpi.service';

@Component({
  selector: 'app-showtime-kpi',
  standalone: true,
  imports: [KpiCardComponent],
  templateUrl: './showtime-kpi.component.html',
  styleUrl: './showtime-kpi.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ShowtimeKpiComponent {
  private readonly kpiService = inject(ShowtimeKpiService);

  readonly kpiItems: ShowtimeKpiItem[] = this.kpiService.getShowtimeKpis();
}
