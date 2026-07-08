import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable, catchError, finalize, map, of, tap } from 'rxjs';
import { API_BASE_URL } from '../../config/api.config';
import { ApiClientService } from '../../http/api-client.service';
import { CurrentUser } from '../auth.models';
import { AuthResponse, AuthUser } from '../models/auth-response';
import { LoginRequest } from '../models/login-request';
import { RegisterRequest } from '../models/register-request';
import { LogoutRequest } from '../models/logout-request';
import { decodeJwtPayload, extractRoleFromPayload, readStringClaim } from '../utils/jwt.util';
import { AuthStateService } from './auth-state.service';
import { TokenService } from './token.service';
import { TokenStoreService } from '../token-store.service';

const ADMIN_ROLES = ['admin', 'administrator', 'superadmin'];
const USER_ROLES = ['user', 'regularuser'];

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly apiClient = inject(ApiClientService);
  private readonly tokenService = inject(TokenService);
  private readonly tokenStore = inject(TokenStoreService);
  private readonly authState = inject(AuthStateService);

  readonly currentUser$ = this.authState.currentUser$.pipe(map((user) => this.toCurrentUser(user)));

  constructor() {
    const token = this.tokenService.getToken();
    if (token) {
      const userFromToken = this.userFromToken(token);
      this.authState.setAuthenticated(true);
      this.authState.setCurrentUser(userFromToken);
    }
  }

  login(request: LoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${API_BASE_URL}/api/auth/login`, request).pipe(
      tap((response) => {
        const token = this.getResponseToken(response);

        if (token) {
          this.tokenService.saveToken(token);
          this.authState.setAuthenticated(true);

          const user =
            response.user ?? this.userFromToken(token) ?? this.userFromResponse(response);
          this.authState.setCurrentUser(user);
        }
      }),
    );
  }

  register(request: RegisterRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${API_BASE_URL}/api/auth/register`, request);
  }

  logout(): Observable<void> {
    const request: LogoutRequest = {
      refreshToken: this.tokenStore.getRefreshToken() ?? '',
      email: this.authState.currentUserValue?.email ?? '',
    };

    return this.apiClient.post<void, LogoutRequest>('/api/auth/logout', request).pipe(
      catchError(() => of(void 0)),
      finalize(() => this.clearSession()),
    );
  }

  isAuthenticated(): boolean {
    return this.authState.isAuthenticatedValue || this.tokenService.isLoggedIn();
  }

  isAuthenticated$(): Observable<boolean> {
    return this.authState.isAuthenticated$;
  }

  loadMe(): Observable<CurrentUser | null> {
    return of(this.toCurrentUser(this.authState.currentUserValue));
  }

  getCurrentRole(): string | null {
    const stateRole = this.authState.currentUserValue?.role;
    if (stateRole) {
      return stateRole;
    }

    const token = this.tokenService.getToken();
    const payload = decodeJwtPayload(token);
    if (!payload) {
      return null;
    }

    return extractRoleFromPayload(payload);
  }

  isAdmin(): boolean {
    return ADMIN_ROLES.includes(this.getCurrentRole()?.toLowerCase() ?? '');
  }

  isUser(): boolean {
    return USER_ROLES.includes(this.getCurrentRole()?.toLowerCase() ?? '');
  }

  private getResponseToken(response: AuthResponse): string | null {
    return response.token ?? response.accessToken ?? null;
  }

  private userFromResponse(response: AuthResponse): AuthUser | null {
    if (!response.userId && !response.email && !response.role) {
      return null;
    }

    return {
      id: response.userId,
      email: response.email,
      role: response.role,
    };
  }

  private toCurrentUser(user: AuthUser | null): CurrentUser | null {
    if (!user?.email && !user?.role && !user?.id) {
      return null;
    }

    return {
      id: user.id ?? '',
      email: user.email ?? '',
      role: user.role ?? '',
    };
  }

  private clearSession(): void {
    this.tokenStore.clear();
    this.authState.clear();
  }

  private userFromToken(token: string): AuthUser | null {
    const payload = decodeJwtPayload(token);
    if (!payload) {
      return null;
    }

    return {
      id:
        readStringClaim(payload, 'sub') ??
        readStringClaim(payload, 'nameid') ??
        readStringClaim(
          payload,
          'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier',
        ) ??
        undefined,
      email:
        readStringClaim(payload, 'email') ??
        readStringClaim(
          payload,
          'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress',
        ) ??
        undefined,
      firstName:
        readStringClaim(payload, 'firstName') ??
        readStringClaim(payload, 'given_name') ??
        readStringClaim(
          payload,
          'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname',
        ) ??
        undefined,
      lastName:
        readStringClaim(payload, 'lastName') ??
        readStringClaim(payload, 'family_name') ??
        readStringClaim(payload, 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname') ??
        undefined,
      role: extractRoleFromPayload(payload) ?? undefined,
    };
  }
}
