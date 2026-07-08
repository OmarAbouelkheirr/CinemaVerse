import { Component, AfterViewInit, OnDestroy, inject, PLATFORM_ID, ElementRef, viewChild } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { Subscription } from 'rxjs';
import {
  Chart,
  LineController,
  LineElement,
  PointElement,
  CategoryScale,
  LinearScale,
  Tooltip,
  Legend,
  Filler,
} from 'chart.js';
import { AdminDashboardService } from '../../data-access/admin-dashboard.service';
import { DashboardChartCardComponent } from '../dashboard-chart-card/dashboard-chart-card.component';

Chart.register(LineController, LineElement, PointElement, CategoryScale, LinearScale, Tooltip, Legend, Filler);

/**
 * Weekly Bookings line chart component for the admin dashboard.
 * 
 * Professional SaaS Dashboard styling:
 * - Smooth curve (tension: 0.4)
 * - Point radius: 5, hover radius: 8
 * - Soft gradient fill
 * - Line width: 3
 * - Dark tooltips with rounded corners
 * - Subtle grid, muted tick labels
 * - No border around chart area
 * - Smooth entrance animation (1000ms)
 * - Hover animations enabled
 */
@Component({
  selector: 'app-weekly-bookings-chart',
  standalone: true,
  imports: [DashboardChartCardComponent],
  templateUrl: './weekly-bookings-chart.component.html',
  styleUrls: ['./weekly-bookings-chart.component.scss'],
})
export class WeeklyBookingsChartComponent implements AfterViewInit, OnDestroy {
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
   * Fetches weekly bookings data from the API and initializes the chart.
   * Falls back to default labels/data if the API returns empty or fails.
   */
  private fetchDataAndRender(): void {
    const sub = this.adminDashboardService.getWeeklyBookings().subscribe({
      next: (response) => {
        const labels = response?.labels?.length
          ? response.labels
          : ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'];
        const data = response?.data?.length ? response.data : [15, 25, 20, 45, 80, 110, 95];

        this.initChart(labels, data);
      },
      error: (err) => {
        console.error('Failed to load weekly bookings data', err);
        this.initChart(['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'], [0, 0, 0, 0, 0, 0, 0]);
      },
    });

    this.subscription.add(sub);
  }

  /**
   * Initializes the Chart.js line chart with professional SaaS Dashboard styling.
   * 
   * Configuration highlights:
   * - Smooth curve (tension: 0.4)
   * - Point radius: 5, hover radius: 8
   * - Soft gradient fill using canvas gradient
   * - Line width: 3
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

    // Create gradient fill for the area under the line
    const ctx = canvas.getContext('2d');
    let gradient: CanvasGradient | undefined;
    if (ctx) {
      gradient = ctx.createLinearGradient(0, 0, 0, canvas.clientHeight);
      gradient.addColorStop(0, 'rgba(138, 235, 255, 0.3)');
      gradient.addColorStop(0.5, 'rgba(138, 235, 255, 0.1)');
      gradient.addColorStop(1, 'rgba(138, 235, 255, 0)');
    }

    this.chartInstance = new Chart(canvas, {
      type: 'line',
      data: {
        labels,
        datasets: [
          {
            label: 'Bookings',
            data,
            borderColor: 'rgba(138, 235, 255, 1)',
            backgroundColor: gradient || 'rgba(138, 235, 255, 0.1)',
            borderWidth: 3,
            tension: 0.4,
            pointRadius: 5,
            pointHoverRadius: 8,
            pointBackgroundColor: 'rgba(138, 235, 255, 1)',
            pointBorderColor: 'rgba(14, 20, 26, 1)',
            pointBorderWidth: 2,
            pointHoverBackgroundColor: 'rgba(138, 235, 255, 1)',
            pointHoverBorderColor: 'rgba(138, 235, 255, 0.5)',
            pointHoverBorderWidth: 3,
            fill: true,
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
                return `${value.toLocaleString()} bookings`;
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
