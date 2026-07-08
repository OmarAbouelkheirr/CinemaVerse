// TicketViewPage — displays a single ticket with QR code, ticket info, download, and print actions
import {
  ChangeDetectionStrategy,
  Component,
  computed,
  inject,
  OnInit,
  signal,
} from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';

import { API_BASE_URL } from '../../../../../../../core/config/api.config';
import { AuthStateService } from '../../../../../../../core/auth/services/auth-state.service';
import { TicketQrComponent } from '../../components/ticket-qr/ticket-qr.component';
import { TicketSkeletonComponent } from '../../components/ticket-skeleton/ticket-skeleton.component';
import { BookingsStore } from '../../state/bookings.store';

@Component({
  selector: 'app-ticket-view-page',
  standalone: true,
  imports: [TicketQrComponent, TicketSkeletonComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './ticket-view.page.html',
  styleUrl: './ticket-view.page.scss',
})
export class TicketViewPage implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly authState = inject(AuthStateService);
  private readonly store = inject(BookingsStore);

  readonly ticket = this.store.selectedTicket;
  readonly loading = this.store.loading;

  readonly bookingId = signal<number | null>(null);

  readonly showNotFound = computed(() => !this.loading() && this.ticket() === null);

  readonly ticketPosterUrl = computed(() => {
    const ticket = this.ticket();
    if (!ticket) {
      return '';
    }

    const poster = ticket.moviePoster;
    if (poster.startsWith('http://') || poster.startsWith('https://')) {
      return poster;
    }

    return `${API_BASE_URL}${poster}`;
  });

  readonly showDate = computed(() => {
    const showStartTime = this.ticket()?.showStartTime;
    if (!showStartTime) {
      return '—';
    }

    return new Date(showStartTime).toLocaleDateString('en-GB', {
      weekday: 'short',
      day: '2-digit',
      month: 'short',
      year: 'numeric',
    });
  });

  readonly showTime = computed(() => {
    const showStartTime = this.ticket()?.showStartTime;
    if (!showStartTime) {
      return '—';
    }

    return new Date(showStartTime).toLocaleTimeString('en-GB', {
      hour: '2-digit',
      minute: '2-digit',
    });
  });

  readonly durationText = computed(() => {
    const movieDuration = this.ticket()?.movieDuration;
    if (!movieDuration) {
      return '—';
    }

    return `${movieDuration.hours}h ${movieDuration.minutes}m`;
  });

  ngOnInit(): void {
    const bookingId = Number(this.route.snapshot.paramMap.get('bookingId'));
    const ticketId = Number(this.route.snapshot.paramMap.get('ticketId'));
    const userId = Number(this.authState.currentUserValue?.id);

    this.bookingId.set(!Number.isNaN(bookingId) && bookingId > 0 ? bookingId : null);

    if (!userId || Number.isNaN(ticketId) || ticketId <= 0) {
      this.store.clearSelection();
      return;
    }

    this.store.clearSelection();
    this.store.loadTicket(userId, ticketId);
  }

  onBackToBooking(): void {
    const bookingId = this.bookingId();
    if (!bookingId) {
      this.router.navigate(['/my-bookings']);
      return;
    }

    this.router.navigate(['/my-bookings', bookingId]);
  }

  onPrintTicket(): void {
    window.print();
  }

  onDownloadTicket(): void {
    const ticket = this.ticket();
    const bookingId = this.bookingId();

    if (!ticket) {
      return;
    }

    const title = `Ticket ${ticket.ticketNumber}`;
    const printableHtml = `<!doctype html>
<html>
<head>
  <meta charset="utf-8" />
  <title>${title}</title>
  <style>
    body { font-family: Arial, Helvetica, sans-serif; padding: 24px; background: #111; color: #fff; }
    .card { max-width: 760px; border: 1px solid #333; border-radius: 14px; margin: 0 auto; overflow: hidden; background: #1a1a1a; }
    .row { display: flex; gap: 20px; padding: 20px; }
    .col { flex: 1; }
    h1 { margin: 0 0 10px; font-size: 22px; }
    p { margin: 6px 0; color: #ddd; }
    strong { color: #fff; }
    img.poster { width: 100%; border-radius: 10px; max-width: 280px; }
    .qr { width: 190px; height: 190px; background: #fff; color: #111; display: grid; place-items: center; border-radius: 8px; font-size: 12px; text-align: center; padding: 10px; }
    @media print { body { background: #fff; color: #000; } .card { border-color: #ccc; background: #fff; } p { color: #333; } }
  </style>
</head>
<body>
  <article class="card">
    <div class="row">
      <div class="col">
        <h1>${ticket.movieName}</h1>
        <p><strong>Branch:</strong> ${ticket.branchName}</p>
        <p><strong>Hall:</strong> ${ticket.hallNumber} (${ticket.hallType})</p>
        <p><strong>Date:</strong> ${this.showDate()}</p>
        <p><strong>Time:</strong> ${this.showTime()}</p>
        <p><strong>Seat:</strong> ${ticket.seatLabel}</p>
        <p><strong>Duration:</strong> ${this.durationText()}</p>
      </div>
      <div class="col">
        <p><strong>Booking ID:</strong> ${bookingId ?? '—'}</p>
        <p><strong>Ticket #:</strong> ${ticket.ticketNumber}</p>
        <p><strong>Status:</strong> ${ticket.status}</p>
        <div class="qr">QR Token<br/>${ticket.qrToken}</div>
      </div>
    </div>
  </article>
</body>
</html>`;

    const blob = new Blob([printableHtml], { type: 'text/html;charset=utf-8' });
    const fileUrl = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = fileUrl;
    link.download = `ticket-${ticket.ticketNumber}.html`;
    link.click();
    URL.revokeObjectURL(fileUrl);
  }
}
