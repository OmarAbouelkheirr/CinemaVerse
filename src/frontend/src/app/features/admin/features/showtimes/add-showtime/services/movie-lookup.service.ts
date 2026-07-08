import { Injectable, inject } from '@angular/core';
import { catchError, map, Observable, of, shareReplay } from 'rxjs';
import { ApiClientService } from '../../../../../../core/http/api-client.service';
import { AdminMovieDto, AdminPagedResponse } from '../../../../admin-api.models';
import { MovieOption } from './movie-option.model';

/**
 * Lightweight lookup service that provides a cached list of movies for dropdowns.
 * Exists so that the create-showtime modal can populate a movie <select>
 * without pulling in the full CRUD MoviesService.
 *
 * The movie list is fetched once from the API and then replayed to every
 * subsequent subscriber via shareReplay(1), preventing duplicate HTTP calls
 * when the modal is opened multiple times.
 */
@Injectable({ providedIn: 'root' })
export class MovieLookupService {
  private readonly api = inject(ApiClientService);

  /**
   * Cached observable that replays the last emitted movie list to new subscribers.
   * Created once at construction time so every call to getMovies() returns
   * the same shared stream.
   */
  private readonly movies$ = this.fetchMovies().pipe(shareReplay({ bufferSize: 1, refCount: true }));

  /**
   * Returns an Observable emitting the list of movie options (id + name).
   * Runs a single GET /api/admin/movies call; subsequent subscribers receive
   * the cached result without triggering another HTTP request.
   *
   * @returns Observable<MovieOption[]> - array of { id, name } pairs
   */
  getMovies(): Observable<MovieOption[]> {
    return this.movies$;
  }

  /**
   * Performs the actual HTTP request to load movies from the API.
   * Maps AdminMovieDto items into lightweight MovieOption objects.
   * Catches errors and returns an empty array so the UI never breaks.
   *
   * @returns Observable<MovieOption[]> - mapped movie options from the API
   */
  private fetchMovies(): Observable<MovieOption[]> {
    return this.api
      .get<AdminPagedResponse<AdminMovieDto>>('/api/admin/movies', {
        Page: 1,
        PageSize: 100,
      })
      .pipe(
        // Extract items from whichever paged-response shape the API returns
        map((response) => response.items ?? response.data ?? response.results ?? []),
        // Map each DTO into a simple { id, name } option for the dropdown
        map((dtos) =>
          dtos
            .filter((dto) => dto.movieId != null)
            .map((dto) => ({
              id: dto.movieId!,
              name: dto.movieName ?? `Movie #${dto.movieId}`,
            })),
        ),
        // Graceful fallback: if the API call fails, emit an empty list
        catchError(() => of([] as MovieOption[])),
      );
  }
}
