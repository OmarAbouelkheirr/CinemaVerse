import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'app-screen-header',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="screen-header">
      <div class="screen-header__surface"></div>
      <span class="screen-header__label">SCREEN</span>
    </div>
  `,
  styles: [
    `
      .screen-header {
        display: flex;
        flex-direction: column;
        align-items: center;
        gap: 0.5rem;
        margin-bottom: 2rem;
      }

      .screen-header__surface {
        width: min(80%, 40rem);
        height: 0.5rem;
        background: linear-gradient(
          180deg,
          var(--primary-container) 0%,
          transparent 100%
        );
        border-radius: 9999px 9999px 0 0;
        opacity: 0.6;
        box-shadow: 0 0 20px rgba(34, 211, 238, 0.2);
      }

      .screen-header__label {
        font-size: var(--text-label);
        font-weight: 700;
        letter-spacing: 0.12em;
        text-transform: uppercase;
        color: var(--on-surface-muted);
      }
    `,
  ],
})
export class ScreenHeaderComponent {}
