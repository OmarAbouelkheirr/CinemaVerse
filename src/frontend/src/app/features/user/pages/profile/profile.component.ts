import { ChangeDetectionStrategy, Component, computed, inject, OnInit } from '@angular/core';
import { DatePipe } from '@angular/common';
import { Router } from '@angular/router';

import { AuthService } from '../../../../core/auth/services/auth.service';
import { ProfileStore } from '../../store/profile.store';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [DatePipe],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProfileComponent implements OnInit {
  private readonly store = inject(ProfileStore);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  readonly profile = this.store.profile;
  readonly loading = this.store.loading;
  readonly error = this.store.error;
  readonly fullName = this.store.fullName;
  readonly isEmailVerified = this.store.isEmailVerified;
  readonly hasProfile = this.store.hasProfile;

  readonly avatarInitials = computed(() => {
    const profile = this.profile();
    if (!profile) {
      return 'U';
    }

    const first = profile.firstName.trim().charAt(0);
    const last = profile.lastName.trim().charAt(0);
    return `${first}${last}`.toUpperCase() || 'U';
  });

  ngOnInit(): void {
    this.store.loadProfile();
  }

  onRetry(): void {
    this.store.loadProfile();
  }

  onEditProfile(): void {
    void this.router.navigate(['/user/edit-profile']);
  }

  onChangePassword(): void {
    void this.router.navigate(['/user/change-password']);
  }

  logout(): void {
    this.authService.logout().subscribe({
      next: () => {
        void this.router.navigate(['/login']);
      },
    });
  }
}
