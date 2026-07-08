import { Injectable, inject } from '@angular/core';
import { map, Observable } from 'rxjs';
import { ApiClientService } from '../../../../../../core/http/api-client.service';
import type { ShowtimesTableRow } from '../components/showtimes-table/showtimes-table.component';
import { AdminPagedResponse, AdminShowtimeDto } from '../../../../admin-api.models';

export interface CreateShowtimePayload {
  movieId: number;
  branchId: number;
  hallId: number;
  showStartTime: string;
  price: number;
}

@Injectable({ providedIn: 'root' })
export class ShowtimesApiService {
  private readonly api = inject(ApiClientService);

  getShowtimes(query?: {
    page?: number;
    pageSize?: number;
    searchTerm?: string;
    movieId?: number;
    hallId?: number;
    branchId?: number;
  }): Observable<ShowtimesTableRow[]> {
    const page = this.normalizePage(query?.page);
    const pageSize = this.normalizePageSize(query?.pageSize);

    const params: Record<string, string | number | boolean> = {
      Page: page,
      PageSize: pageSize,
    };

    const searchTerm = query?.searchTerm?.trim();
    if (searchTerm) {
      params['SearchTerm'] = searchTerm;
    }

    if (query?.movieId && query.movieId > 0) {
      params['MovieId'] = query.movieId;
    }

    if (query?.hallId && query.hallId > 0) {
      params['HallId'] = query.hallId;
    }

    if (query?.branchId && query.branchId > 0) {
      params['BranchId'] = query.branchId;
    }

    return this.api
      .get<AdminPagedResponse<AdminShowtimeDto>>('/api/admin/showtimes', params)
      .pipe(
        map((response) =>
          (response.items ?? response.data ?? response.results ?? []).map((item) =>
            this.mapRow(item),
          ),
        ),
      );
  }

  /**
   * Sends a POST request to create a new showtime.
   * The payload already contains numeric IDs (movieId, hallId, branchId)
   * and an ISO showStartTime string, so no conversion is needed.
   *
   * Runs: when the admin submits the create-showtime form.
   * Returns: Observable<ShowtimesTableRow> — the newly created showtime row.
   */
  createShowtime(payload: CreateShowtimePayload): Observable<ShowtimesTableRow> {
    return this.api
      .post<
        AdminShowtimeDto,
        { movieId: number; hallId: number; branchId: number; showStartTime: string; price: number }
      >('/api/admin/showtimes', {
        movieId: payload.movieId,
        hallId: payload.hallId,
        branchId: payload.branchId,
        showStartTime: payload.showStartTime,
        price: payload.price,
      })
      .pipe(map((dto) => this.mapRow(dto)));
  }

  private mapRow(dto: AdminShowtimeDto): ShowtimesTableRow {
    const start = dto.showStartTime ? new Date(dto.showStartTime) : null;
    const end = dto.showEndTime ? new Date(dto.showEndTime) : null;
    const createdAt = dto.createdAt ? new Date(dto.createdAt) : null;

    return {
      id: dto.id ? `SHW-${dto.id}` : `SHW-${Math.random().toString(36).slice(2, 8).toUpperCase()}`,
      movieTitle: dto.movieName ?? dto.movieTitle ?? 'Unknown movie',
      branchName: dto.branchName ?? '—',
      hallName: dto.hallNumber ? `Hall ${dto.hallNumber}` : '—',
      date: start && !Number.isNaN(start.getTime()) ? start.toISOString().slice(0, 10) : '',
      startTime:
        start && !Number.isNaN(start.getTime())
          ? start.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit', hour12: false })
          : '',
      endTime:
        end && !Number.isNaN(end.getTime())
          ? end.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit', hour12: false })
          : '',
      price: dto.price ?? 0,
      availableSeats: dto.availableSeats ?? 0,
      totalSeats: dto.totalSeats ?? dto.totalTickets ?? 0,
      status: this.mapStatus(start),
      createdAt:
        createdAt && !Number.isNaN(createdAt.getTime()) ? createdAt.toISOString().slice(0, 10) : '',
    };
  }

  private mapStatus(start: Date | null): ShowtimesTableRow['status'] {
    if (!start || Number.isNaN(start.getTime())) {
      return 'SCHEDULED';
    }

    const now = Date.now();
    const startMs = start.getTime();
    if (startMs < now) {
      return 'NOW_SHOWING';
    }

    return 'SCHEDULED';
  }

  private normalizePage(page?: number): number {
    if (!page || !Number.isFinite(page)) {
      return 1;
    }

    return Math.max(1, Math.floor(page));
  }

  private normalizePageSize(pageSize?: number): number {
    if (!pageSize || !Number.isFinite(pageSize)) {
      return 100;
    }

    return Math.min(100, Math.max(1, Math.floor(pageSize)));
  }
}
