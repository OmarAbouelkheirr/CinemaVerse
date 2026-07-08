import { ChangeDetectionStrategy, Component, input, signal } from '@angular/core';
import { CommonModule } from '@angular/common';

import { MovieDetail } from '../interfaces/movie-detail.interface';

type TabId = 'overview' | 'cast' | 'gallery' | 'reviews';

interface Tab {
  id: TabId;
  label: string;
}

@Component({
  selector: 'app-movie-tabs',
  standalone: true,
  imports: [CommonModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="tabs">
      <nav class="tabs__nav">
        @for (tab of tabs; track tab.id) {
          <button
            type="button"
            class="tabs__btn"
            [class.tabs__btn--active]="activeTab() === tab.id"
            (click)="activeTab.set(tab.id)">
            {{ tab.label }}
          </button>
        }
      </nav>

      <div class="tabs__content">
        @if (movie(); as m) {
          @switch (activeTab()) {
            @case ('overview') {
              <div class="tabs__section">
                <h3 class="tabs__section-title">Synopsis</h3>
                <p class="tabs__text">{{ m.movieDescription }}</p>
              </div>
            }
            @case ('cast') {
              @if (m.castMembers && m.castMembers.length > 0) {
                <div class="tabs__cast-grid">
                  @for (member of m.castMembers; track member.id) {
                    <div class="tabs__cast-card">
                      <img
                        class="tabs__cast-photo"
                        [src]="member.imageUrl"
                        [alt]="member.personName"
                        loading="lazy" />
                      <div class="tabs__cast-info">
                        <span class="tabs__cast-name">{{ member.personName }}</span>
                        <span class="tabs__cast-character">as {{ member.characterName }}</span>
                      </div>
                    </div>
                  }
                </div>
              } @else {
                <p class="tabs__empty">No cast information available</p>
              }
            }
            @case ('gallery') {
              @if (m.images && m.images.length > 0) {
                <div class="tabs__gallery">
                  @for (image of m.images; track image.id) {
                    <img
                      class="tabs__gallery-image"
                      [src]="image.imageUrl"
                      alt="Movie still"
                      loading="lazy" />
                  }
                </div>
              } @else {
                <p class="tabs__empty">No images available</p>
              }
            }
            @case ('reviews') {
              <p class="tabs__empty">No reviews yet</p>
            }
          }
        }
      </div>
    </div>
  `,
  styles: [
    `
      .tabs {
        display: flex;
        flex-direction: column;
        gap: 1.5rem;
      }

      .tabs__nav {
        display: flex;
        gap: 0.5rem;
        border-bottom: 1px solid var(--ghost-border);
        padding-bottom: 0.5rem;
      }

      .tabs__btn {
        padding: 0.5rem 1rem;
        background: transparent;
        border: none;
        border-radius: var(--radius-md);
        font-size: var(--text-body);
        font-weight: 600;
        color: var(--on-surface-muted);
        cursor: pointer;
        transition: all var(--transition);
        font-family: inherit;
      }

      .tabs__btn:hover {
        background: var(--surface-container);
        color: var(--on-surface-variant);
      }

      .tabs__btn--active {
        background: var(--primary-dim);
        color: var(--primary-container);
      }

      .tabs__content {
        padding: 1.5rem;
        background: var(--surface-container-low);
        border: 1px solid var(--ghost-border);
        border-radius: var(--radius-lg);
        min-height: 12rem;
      }

      .tabs__section {
        display: flex;
        flex-direction: column;
        gap: 0.75rem;
      }

      .tabs__section-title {
        font-size: var(--text-title-sm);
        font-weight: 700;
        color: var(--on-surface);
        margin: 0;
      }

      .tabs__text {
        font-size: var(--text-body);
        color: var(--on-surface-variant);
        line-height: 1.6;
        margin: 0;
      }

      .tabs__cast-grid {
        display: grid;
        grid-template-columns: repeat(auto-fill, minmax(12rem, 1fr));
        gap: 1rem;
      }

      .tabs__cast-card {
        display: flex;
        gap: 0.75rem;
        padding: 0.75rem;
        background: var(--surface-container);
        border: 1px solid var(--ghost-border);
        border-radius: var(--radius-md);
      }

      .tabs__cast-photo {
        width: 3.5rem;
        height: 3.5rem;
        border-radius: var(--radius-sm);
        object-fit: cover;
        flex-shrink: 0;
      }

      .tabs__cast-info {
        display: flex;
        flex-direction: column;
        gap: 0.25rem;
        min-width: 0;
      }

      .tabs__cast-name {
        font-size: var(--text-body-sm);
        font-weight: 600;
        color: var(--on-surface);
      }

      .tabs__cast-character {
        font-size: var(--text-body-sm);
        color: var(--on-surface-muted);
      }

      .tabs__gallery {
        display: grid;
        grid-template-columns: repeat(auto-fill, minmax(16rem, 1fr));
        gap: 1rem;
      }

      .tabs__gallery-image {
        width: 100%;
        aspect-ratio: 16 / 9;
        object-fit: cover;
        border-radius: var(--radius-md);
      }

      .tabs__empty {
        font-size: var(--text-body);
        color: var(--on-surface-muted);
        margin: 0;
        text-align: center;
        padding: 2rem;
      }
    `,
  ],
})
export class MovieTabsComponent {
  readonly movie = input.required<MovieDetail>();

  readonly tabs: Tab[] = [
    { id: 'overview', label: 'Overview' },
    { id: 'cast', label: 'Cast' },
    { id: 'gallery', label: 'Gallery' },
    { id: 'reviews', label: 'Reviews' },
  ];

  readonly activeTab = signal<TabId>('overview');
}
