import { ChangeDetectionStrategy, Component, input } from '@angular/core';

/**
 * Reusable chart card wrapper for the admin dashboard.
 * 
 * Structure:
 * --------------------------------
 * Title              [Optional Action]
 * ─── Divider ─────────────────────
 * Chart (projected content)
 * --------------------------------
 * 
 * Visual rules enforced:
 * - Same padding, header spacing, title typography
 * - Same border radius, card height
 * - Dark theme, no chart borders
 * - Soft grid, rounded bars/lines
 */
@Component({
  selector: 'app-dashboard-chart-card',
  standalone: true,
  templateUrl: './dashboard-chart-card.component.html',
  styleUrl: './dashboard-chart-card.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DashboardChartCardComponent {
  /**
   * The chart title displayed in the card header (e.g., "Monthly Revenue").
   * Rendered with consistent typography across all chart cards.
   */
  readonly title = input.required<string>();

  /**
   * Optional subtitle displayed below the title (e.g., "Last 6 months").
   * Rendered in small muted text for additional context.
   */
  readonly subtitle = input<string | undefined>(undefined);

  /**
   * Whether to show a divider line between the header and chart area.
   * Defaults to true for visual separation.
   */
  readonly showDivider = input(true);
}
