import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { ApiClientService } from '../../../core/http/api-client.service';
import { ChangePasswordRequest, GetMyProfileResponse, UpdateProfileRequest } from '../models';

@Injectable({ providedIn: 'root' })
export class ProfileService {
  private readonly apiClient = inject(ApiClientService);
  private readonly api = '/api/me';

  getProfile(): Observable<GetMyProfileResponse> {
    return this.apiClient.get<GetMyProfileResponse>(this.api);
  }

  updateProfile(data: UpdateProfileRequest): Observable<void> {
    return this.apiClient.put<void, UpdateProfileRequest>(this.api, data);
  }

  changePassword(data: ChangePasswordRequest): Observable<void> {
    return this.apiClient.post<void, ChangePasswordRequest>(`${this.api}/change-password`, data);
  }
}
