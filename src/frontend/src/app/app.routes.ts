import { Routes } from '@angular/router';
import { authGuard } from './core/auth/guards/auth.guard';
import { roleGuard } from './core/guards/role.guard';

export const routes: Routes = [
  // Redirect root to login
  { path: '', pathMatch: 'full', redirectTo: 'login' },

  // Auth pages (no shell - no Header/Footer)
  {
    path: 'login',
    loadComponent: () => import('./features/auth/pages/login/login.page').then((m) => m.LoginPage),
  },
  {
    path: 'register',
    loadComponent: () =>
      import('./features/auth/pages/register/register.page').then((m) => m.RegisterPage),
  },
  {
    path: 'unauthorized',
    loadComponent: () =>
      import('./features/auth/pages/unauthorized').then((m) => m.UnauthorizedPage),
  },
  {
    path: 'auth/login',
    redirectTo: 'login',
    pathMatch: 'full',
  },
  {
    path: 'auth/register',
    redirectTo: 'register',
    pathMatch: 'full',
  },
  {
    path: 'auth/forgot-password',
    loadComponent: () =>
      import('./features/auth/pages/forgot-password/forgot-password.page').then(
        (m) => m.ForgotPasswordPage,
      ),
  },
  {
    path: 'auth/reset-password',
    loadComponent: () =>
      import('./features/auth/pages/reset-password/reset-password.page').then(
        (m) => m.ResetPasswordPage,
      ),
  },
  {
    path: 'auth/verify-email',
    loadComponent: () =>
      import('./features/auth/pages/verify-email/verify-email.page').then((m) => m.VerifyEmailPage),
  },
  {
    path: 'auth/resend-verification',
    loadComponent: () =>
      import('./features/auth/pages/resend-verification/resend-verification.page').then(
        (m) => m.ResendVerificationPage,
      ),
  },

  // User feature pages - all render inside AppShell (Header + Footer)
  {
    path: '',
    canActivate: [authGuard, roleGuard(['User', 'RegularUser'])],
    loadComponent: () =>
      import('./layout/app-shell/app-shell.component').then((m) => m.AppShellComponent),
    children: [
      { path: '', redirectTo: 'user/dashboard', pathMatch: 'full' },
      // Legacy /user/* routes (for backward compatibility)
      {
        path: 'user',
        children: [
          { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
          {
            path: 'dashboard',
            loadComponent: () =>
              import('./features/user/user-mangment/feature/home/home').then((m) => m.Home),
          },
          {
            path: 'home',
            redirectTo: 'dashboard',
            pathMatch: 'full',
          },
          {
            path: 'profile',
            loadComponent: () =>
              import('./features/user/pages/profile/profile.component').then(
                (m) => m.ProfileComponent,
              ),
          },
          {
            path: 'edit-profile',
            loadComponent: () =>
              import('./features/user/pages/edit-profile/edit-profile.component').then(
                (m) => m.EditProfileComponent,
              ),
          },
          {
            path: 'change-password',
            loadComponent: () =>
              import('./features/user/pages/change-password/change-password.component').then(
                (m) => m.ChangePasswordComponent,
              ),
          },
        ],
      },
      // Movies listing page
      {
        path: 'movies',
        loadComponent: () =>
          import('./features/user/user-mangment/feature/home/nowShowing/now-showing/now-showing').then(
            (m) => m.NowShowingComponent,
          ),
      },
      // Movie detail page (with showtime selection)
      {
        path: 'movies/:id',
        loadComponent: () =>
          import('./features/user/user-mangment/feature/movie-detail/movie-detail-page').then(
            (m) => m.MovieDetailPage,
          ),
      },
      {
        path: 'my-bookings',
        loadComponent: () =>
          import('./features/user/user-mangment/feature/bookings/pages/bookings-list/bookings-list.page').then(
            (m) => m.BookingsListPage,
          ),
      },
      {
        path: 'my-bookings/:bookingId',
        loadComponent: () =>
          import('./features/user/user-mangment/feature/bookings/pages/booking-details/booking-details.page').then(
            (m) => m.BookingDetailsPage,
          ),
      },
      {
        path: 'my-bookings/:bookingId/ticket/:ticketId',
        loadComponent: () =>
          import('./features/user/user-mangment/feature/bookings/pages/ticket-view/ticket-view.page').then(
            (m) => m.TicketViewPage,
          ),
      },
      {
        path: 'movie-booking/:id',
        children: [
          { path: '', redirectTo: 'seat-selection', pathMatch: 'full' },
          {
            path: 'seat-selection',
            loadComponent: () =>
              import('./features/user/user-mangment/feature/movie-booking/pages/seat-selection/seat-selection.page').then(
                (m) => m.SeatSelectionPage,
              ),
          },
          {
            path: 'payment',
            loadComponent: () =>
              import('./features/user/user-mangment/feature/movie-booking/pages/payment/payment.page').then(
                (m) => m.PaymentPage,
              ),
          },
          {
            path: 'success',
            loadComponent: () =>
              import('./features/user/user-mangment/feature/movie-booking/pages/success/success.page').then(
                (m) => m.BookingSuccessPage,
              ),
          },
        ],
      },
    ],
  },

  // Admin routes - have their own admin shell (different UI)
  {
    path: 'admin',
    loadChildren: () => import('./features/admin/admin.routes').then((m) => m.ADMIN_ROUTES),
  },

  // Wildcard redirect
  { path: '**', redirectTo: 'login' },
];
