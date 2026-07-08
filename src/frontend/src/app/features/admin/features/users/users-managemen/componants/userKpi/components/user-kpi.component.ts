import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { KpiCardComponent } from '../../../../../../features/dashboard/components/kpi-card/kpi-card.component';
import { UserKpiService } from '../services/user-kpi.service';

@Component({
  selector: 'app-user-kpi',
  standalone: true,
  imports: [KpiCardComponent],
  templateUrl: './user-kpi.component.html',
  styleUrl: './user-kpi.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UserKpiComponent {
  private readonly userKpiService = inject(UserKpiService);

  readonly kpiItems = this.userKpiService.getUserKpis();
}
