import { Component, AfterViewInit, OnDestroy, inject, PLATFORM_ID, ElementRef, viewChild } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { Subscription } from 'rxjs';
import {
  Chart,
  BarController,
  BarElement,
  CategoryScale,
  LinearScale,
  Tooltip,
  Legend,
} from 'chart.js';
import { AdminDashboardService } from '../../../data-access/admin-dashboard.service';
import { DashboardChartCardComponent } from '../../dashboard-chart-card/dashboard-chart-card.component';

Chart.register(BarController, BarElement, CategoryScale, LinearScale, Tooltip, Legend);

/**
 * Monthly Revenue bar chart component for the admin dashboard.
 * 
 * Professional SaaS Dashboard styling:
 * - Rounded bars with large hover radius
 * - Maximum bar width with better spacing
 * - No legend (only one dataset)
 * - Dark tooltips with rounded corners
 * - Subtle grid, muted tick labels
 * - No border around chart area
 * - Smooth entrance animation (1000ms)
 * - Hover animations enabled
 */
@Component({
  selector: 'app-monthly-revenue-chart',
  standalone: true,
  imports: [DashboardChartCardComponent],
  templateUrl: './monthly-revenue-chart.component.html',
  styleUrls: ['./monthly-revenue-chart.component.scss'],
})
export class MonthlyRevenueChartComponent implements AfterViewInit, OnDestroy {
  private readonly adminDashboardService = inject(AdminDashboardService);
  private readonly platformId = inject(PLATFORM_ID);

  private subscription = new Subscription();
  private chartInstance: Chart | null = null;

  /**
   * Reference to the canvas element for Chart.js rendering.
   * Using viewChild for type-safe element access.
   */
  private readonly canvasRef = viewChild<ElementRef<HTMLCanvasElement>>('chartCanvas');

  ngAfterViewInit(): void {
    if (isPlatformBrowser(this.platformId)) {
      this.fetchDataAndRender();
    }
  }

  /**
   * Fetches monthly revenue data from the API and initializes the chart.
   * Falls back to default labels/data if the API returns empty or fails.
   */
  private fetchDataAndRender(): void {
    const sub = this.adminDashboardService.getMonthlyRevenue().subscribe({
      next: (response) => {
        const labels = response?.labels?.length
          ? response.labels
          : ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun'];
        const data = response?.data?.length ? response.data : [1200, 1900, 3000, 5000, 2000, 3000];

        this.initChart(labels, data);
      },
      error: (err) => {
        console.error('Failed to load monthly revenue data', err);
        this.initChart(['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun'], [0, 0, 0, 0, 0, 0]);
      },
    });

    this.subscription.add(sub);
  }

  /**
   * Initializes the Chart.js bar chart with professional SaaS Dashboard styling.
   * 
   * Configuration highlights:
   * - Rounded bars (borderRadius: 8)
   * - Large hover radius (hoverBorderRadius: 8)
   * - Maximum bar width (maxBarThickness: 48)
   * - Better spacing (categoryPercentage: 0.8, barPercentage: 0.7)
   * - No legend (display: false)
   * - Dark tooltips with rounded corners
   * - Subtle grid (rgba(255,255,255,0.04))
   * - Muted tick labels (#8b949e)
   * - No border around chart area (drawBorder: false)
   * - Smooth entrance animation (duration: 1000ms)
   * - Hover animations enabled
   */
  private initChart(labels: string[], data: number[]): void {
    const canvasEl = this.canvasRef();
    if (!canvasEl) return;

    const canvas = canvasEl.nativeElement;
    if (!canvas) return;

    if (this.chartInstance) {
      this.chartInstance.destroy();
    }

    this.chartInstance = new Chart(canvas, {
      type: 'bar',
      data: {
        labels,
        datasets: [
          {
            label: 'Revenue',
            data,
            backgroundColor: 'rgba(138, 235, 255, 0.8)',
            hoverBackgroundColor: 'rgba(138, 235, 255, 1)',
            borderColor: 'rgba(138, 235, 255, 0.9)',
            hoverBorderColor: 'rgba(138, 235, 255, 1)',
            borderRadius: 8,
            borderWidth: 0,
            maxBarThickness: 48,
            categoryPercentage: 0.8,
            barPercentage: 0.7,
          },
        ],
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        animation: {
          duration: 1000,
          easing: 'easeOutQuart',
        },
        interaction: {
          intersect: false,
          mode: 'index',
        },
        plugins: {
          legend: {
            display: false,
          },
          tooltip: {
            enabled: true,
            backgroundColor: 'rgba(14, 20, 26, 0.95)',
            titleColor: '#dde3eb',
            bodyColor: '#bbc9cd',
            borderColor: 'rgba(138, 235, 255, 0.2)',
            borderWidth: 1,
            cornerRadius: 8,
            padding: 12,
            titleFont: {
              size: 13,
              weight: 'bold',
            },
            bodyFont: {
              size: 12,
            },
            displayColors: false,
            callbacks: {
              label: (context) => {
                const value = context.parsed.y;
                if (value == null) return '';
                return `$${value.toLocaleString()}`;
              },
            },
          },
        },
        scales: {
          x: {
            grid: {
              color: 'rgba(255, 255, 255, 0.04)',
              drawTicks: false,
            },
            border: {
              display: false,
            },
            ticks: {
              color: '#8b949e',
              font: {
                size: 11,
              },
              padding: 8,
            },
          },
          y: {
            grid: {
              color: 'rgba(255, 255, 255, 0.04)',
              drawTicks: false,
            },
            border: {
              display: false,
            },
            ticks: {
              color: '#8b949e',
              font: {
                size: 11,
              },
              padding: 8,
              callback: (value) => {
                const numValue = Number(value);
                if (numValue >= 1000) {
                  return `$${numValue / 1000}k`;
                }
                return `$${numValue}`;
              },
            },
            beginAtZero: true,
          },
        },
      },
    });
  }

  ngOnDestroy(): void {
    this.subscription.unsubscribe();
    if (this.chartInstance) {
      this.chartInstance.destroy();
    }
  }
}
