import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';

@Component({
  selector: 'app-continue-button',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <button
      type="button"
      class="continue-btn"
      [class.continue-btn--disabled]="disabled() || loading()"
      [disabled]="disabled() || loading()"
      (click)="continueClick.emit()"
    >
      <span class="continue-btn__text">{{ loading() ? 'Processing...' : 'Continue' }}</span>
      <span class="material-symbols-outlined continue-btn__icon">arrow_forward</span>
    </button>
  `,
  styles: [
    `
      .continue-btn {
        display: flex;
        align-items: center;
        justify-content: center;
        gap: 0.5rem;
        width: 100%;
        padding: 0.875rem 1.5rem;
        font-size: var(--text-body);
        font-weight: 700;
        font-family: inherit;
        letter-spacing: 0.04em;
        text-transform: uppercase;
        color: var(--on-primary);
        background: linear-gradient(135deg, var(--primary-container) 0%, var(--primary) 100%);
        border: none;
        border-radius: var(--radius-md);
        cursor: pointer;
        transition: all var(--transition);
      }

      .continue-btn:hover:not(.continue-btn--disabled) {
        filter: brightness(1.08);
        transform: translateY(-1px);
        box-shadow: 0 4px 16px rgba(34, 211, 238, 0.25);
      }

      .continue-btn:active:not(.continue-btn--disabled) {
        transform: translateY(0);
      }

      .continue-btn--disabled {
        opacity: 0.4;
        cursor: not-allowed;
      }

      .continue-btn__icon {
        font-size: 18px;
      }
    `,
  ],
})
export class ContinueButtonComponent {
  readonly disabled = input(false);
  readonly loading = input(false);
  readonly continueClick = output<void>();
}
