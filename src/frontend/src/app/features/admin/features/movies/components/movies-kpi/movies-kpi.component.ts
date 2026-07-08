import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { KpiCardComponent } from '../../../../features/dashboard/components/kpi-card/kpi-card.component';

@Component({
  selector: 'app-movies-kpi',
  standalone: true,
  imports: [KpiCardComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './movies-kpi.component.html',
  styleUrl: './movies-kpi.component.scss',
})
export class MoviesKpiComponent {
  readonly totalMovies = input.required<number>();
  readonly nowShowingMovies = input.required<number>();
  readonly comingSoonMovies = input.required<number>();
  readonly averageRating = input.required<number>();

  readonly totalMoviesDisplay = computed(() => this.totalMovies().toLocaleString());
  readonly nowShowingDisplay = computed(() => this.nowShowingMovies().toLocaleString());
  readonly comingSoonDisplay = computed(() => this.comingSoonMovies().toLocaleString());
  readonly averageRatingDisplay = computed(() => this.averageRating().toFixed(1));
}
