import { ChangeDetectionStrategy, Component, input } from '@angular/core';

/**
 * Reusable KPI card component for the admin dashboard.
 * 
 * Visual rules enforced:
 * - Same height, border radius, padding, icon container, typography, spacing
 * - Same hover animation, background, shadows
 * - Icon container: 48x48, rounded, primary color background with low opacity
 * - Icon size: 22-24px
 * - Value: Large, Bold
 * - Title: Muted
 */
@Component({
  selector: 'app-kpi-card',
  standalone: true,
  templateUrl: './kpi-card.component.html',
  styleUrl: './kpi-card.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class KpiCardComponent {
  /**
   * Material Symbols icon name to display in the icon container.
   * Examples: 'payments', 'confirmation_number', 'group', 'chair'
   */
  readonly icon = input.required<string>();

  /**
   * The metric title displayed below the value (e.g., "Total Revenue").
   * Rendered in muted color to establish visual hierarchy.
   */
  readonly title = input.required<string>();

  /**
   * The metric value displayed prominently (e.g., "$12,345", "1,234", "85.5%").
   * Rendered large and bold as the focal point of the card.
   */
  readonly value = input.required<string>();

  /**
   * Optional accent color for the icon container background.
   * Defaults to the project's primary cyan color with low opacity.
   * Accepts any valid CSS color value.
   */
  readonly accentColor = input<string | undefined>(undefined);

  /**
   * Optional trend indicator showing metric change (e.g., "+12.5%", "-3.2%").
   * Displayed with an arrow icon when provided.
   */
  readonly trend = input<string | undefined>(undefined);

  /**
   * Optional trend direction: 'up' (positive), 'down' (negative), or 'neutral'.
   * Controls the trend arrow icon and color coding.
   */
  readonly trendType = input<'up' | 'down' | 'neutral' | undefined>(undefined);

  /**
   * Optional subtitle displayed below the title (e.g., API source, description).
   * Rendered in small muted text for additional context.
   */
  readonly subtitle = input<string | undefined>(undefined);
}
