import { Injectable, inject } from '@angular/core';
import { catchError, map, Observable, of, shareReplay } from 'rxjs';
import { ApiClientService } from '../../../../../../core/http/api-client.service';
import { AdminBranchDto, AdminPagedResponse } from '../../../../admin-api.models';
import { BranchOption } from './branch-option.model';

/**
 * Lightweight lookup service that provides a cached list of branches for dropdowns.
 * Exists so that the create-showtime modal can populate a branch <select>
 * without pulling in the full CRUD BranchesService.
 *
 * The branch list is fetched once from the API and then replayed to every
 * subsequent subscriber via shareReplay(1), preventing duplicate HTTP calls
 * when the modal is opened multiple times.
 */
@Injectable({ providedIn: 'root' })
export class BranchLookupService {
  private readonly api = inject(ApiClientService);

  /**
   * Cached observable that replays the last emitted branch list to new subscribers.
   * Created once so every call to getBranches() returns the same shared stream.
   */
  private readonly branches$ = this.fetchBranches().pipe(
    shareReplay({ bufferSize: 1, refCount: true }),
  );

  /**
   * Returns an Observable emitting the list of branch options (id + name).
   * Runs a single GET /api/admin/branches call; subsequent subscribers receive
   * the cached result without triggering another HTTP request.
   *
   * @returns Observable<BranchOption[]> - array of { id, name } pairs
   */
  getBranches(): Observable<BranchOption[]> {
    return this.branches$;
  }

  /**
   * Performs the actual HTTP request to load branches from the API.
   * Maps AdminBranchDto items into lightweight BranchOption objects.
   * Catches errors and returns an empty array so the UI never breaks.
   *
   * @returns Observable<BranchOption[]> - mapped branch options from the API
   */
  private fetchBranches(): Observable<BranchOption[]> {
    return this.api
      .get<AdminPagedResponse<AdminBranchDto>>('/api/admin/branches', {
        Page: 1,
        PageSize: 100,
      })
      .pipe(
        // Extract items from whichever paged-response shape the API returns
        map((response) => response.items ?? response.data ?? response.results ?? []),
        // Map each DTO into a simple { id, name } option for the dropdown
        map((dtos) =>
          dtos
            .filter((dto) => dto.id != null)
            .map((dto) => ({
              id: dto.id!,
              name: dto.branchName ?? `Branch #${dto.id}`,
            })),
        ),
        // Graceful fallback: if the API call fails, emit an empty list
        catchError(() => of([] as BranchOption[])),
      );
  }
}
