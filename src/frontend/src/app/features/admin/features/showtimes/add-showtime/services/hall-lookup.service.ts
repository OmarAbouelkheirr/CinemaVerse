import { Injectable, inject } from '@angular/core';
import { catchError, map, Observable, of } from 'rxjs';
import { ApiClientService } from '../../../../../../core/http/api-client.service';
import { AdminHallDto, AdminPagedResponse } from '../../../../admin-api.models';
import { HallOption } from './hall-option.model';

/**
 * Lookup service that loads halls filtered by a specific branch.
 * Exists so that the create-showtime modal can populate a hall <select>
 * that depends on the currently selected branch.
 *
 * Unlike MovieLookupService and BranchLookupService, this service does NOT
 * cache results because the hall list changes every time the selected branch
 * changes. The modal component uses switchMap to cancel in-flight requests
 * when the branch selection changes rapidly.
 */
@Injectable({ providedIn: 'root' })
export class HallLookupService {
  private readonly api = inject(ApiClientService);

  /**
   * Fetches halls for a given branch from the API.
   * Runs GET /api/admin/halls?BranchId={branchId} every time it is called.
   * Maps AdminHallDto items into lightweight HallOption objects.
   * Catches errors and returns an empty array so the UI never breaks.
   *
   * @param branchId - the numeric ID of the branch whose halls should be loaded
   * @returns Observable<HallOption[]> - array of { id, hallNumber } pairs
   */
  getHallsByBranch(branchId: number): Observable<HallOption[]> {
    return this.api
      .get<AdminPagedResponse<AdminHallDto>>('/api/admin/halls', {
        BranchId: branchId,
        Page: 1,
        PageSize: 100,
      })
      .pipe(
        // Extract items from whichever paged-response shape the API returns
        map((response) => response.items ?? response.data ?? response.results ?? []),
        // Map each DTO into a simple { id, hallNumber } option for the dropdown
        map((dtos) =>
          dtos
            .filter((dto) => dto.id != null)
            .map((dto) => ({
              id: dto.id!,
              hallNumber: dto.hallNumber ?? `Hall #${dto.id}`,
            })),
        ),
        // Graceful fallback: if the API call fails, emit an empty list
        catchError(() => of([] as HallOption[])),
      );
  }
}
