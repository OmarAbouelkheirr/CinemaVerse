import { Component, Input, inject } from '@angular/core';
import { Router } from '@angular/router';
import { IMovie } from '../../../../../user-core/user-services/user-movie/movie-interface';
import { API_BASE_URL } from '../../../../../../../core/config/api.config';

@Component({
  selector: 'app-movie-card',
  standalone: true,
  imports: [],
  templateUrl: './movie-card.html',
  styleUrl: './movie-card.component.scss'
})
export class MovieCardComponent {
  @Input({ required: true }) movie!: IMovie;

  private readonly router = inject(Router);

  resolvePosterUrl(posterUrl: string): string {
    if (posterUrl.startsWith('http://') || posterUrl.startsWith('https://')) {
      return posterUrl;
    }
    return `${API_BASE_URL}${posterUrl}`;
  }

  openBooking(): void {
    this.router.navigate(['/movies', this.movie.movieId]);
  }
}
