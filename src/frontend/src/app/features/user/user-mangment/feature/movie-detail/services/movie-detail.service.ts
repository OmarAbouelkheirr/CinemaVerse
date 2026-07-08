import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { ApiClientService } from '../../../../../../core/http/api-client.service';
import { MovieDetail } from '../interfaces/movie-detail.interface';

@Injectable({ providedIn: 'root' })
export class MovieDetailService {
  private readonly apiClient = inject(ApiClientService);

  getMovieDetail(movieId: number): Observable<MovieDetail> {
    return this.apiClient.get<MovieDetail>(`/api/movies/${movieId}`);
  }
}
