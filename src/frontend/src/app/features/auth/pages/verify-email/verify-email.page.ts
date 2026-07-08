import { ChangeDetectionStrategy, Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';

import { AuthRecoveryStore } from '../../store/auth-recovery.store';

@Component({
  selector: 'app-verify-email-page',
  imports: [CommonModule, RouterLink],
  templateUrl: './verify-email.page.html',
  styleUrl: './verify-email.page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class VerifyEmailPage implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly store = inject(AuthRecoveryStore);

  readonly loading = this.store.loading;
  readonly error = this.store.error;
  readonly successMessage = this.store.successMessage;

  readonly tokenError = signal<string | null>(null);

  ngOnInit(): void {
    const token = this.route.snapshot.queryParamMap.get('token') ?? '';
    if (!token) {
      this.tokenError.set('Verification token is missing or invalid');
      return;
    }

    this.store.verifyEmail(token);
  }

  goToLogin(): void {
    void this.router.navigate(['/login']);
  }
}
