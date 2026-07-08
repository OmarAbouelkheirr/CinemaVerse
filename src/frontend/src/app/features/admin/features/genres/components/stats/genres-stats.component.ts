import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { KpiCardComponent } from '../../../../features/dashboard/components/kpi-card/kpi-card.component';
import type { Genre } from '../../models/genre.model';

@Component({
  selector: 'app-genres-stats',
  standalone: true,
  imports: [KpiCardComponent],
  templateUrl: './genres-stats.component.html',
  styleUrl: './genres-stats.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class GenresStatsComponent {
  readonly totalGenres = input(0);
  readonly mostPopular = input<Genre | null>(null);
  readonly recentlyAdded = input<Genre | null>(null);

  readonly mostPopularValue = computed(() => this.mostPopular()?.name ?? '\u2014');
  readonly mostPopularSubtitle = computed(() => {
    const popular = this.mostPopular();
    return popular ? `${popular.moviesCount} movies` : undefined;
  });

  readonly recentlyAddedValue = computed(() => this.recentlyAdded()?.name ?? '\u2014');
  readonly recentlyAddedSubtitle = computed(() => this.recentlyAdded()?.id);
}
