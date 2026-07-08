import { Injectable } from '@angular/core';
import { catchError, forkJoin, map, Observable, of } from 'rxjs';
import { ApiClientService } from '../../../../../core/http/api-client.service';

export interface KpiData {
  title: string;
  value: string;
  icon: string;
  subtitle?: string;
  trend?: string;
  trendType?: 'up' | 'down' | 'neutral';
}

export interface ChartSeriesData {
  labels: string[];
  data: number[];
}

export interface AdminDashboardSummaryResponse {
  totalRevenue?: number;
  totalBookings?: number;
  activeUsers?: number;
  occupancyRate?: number;
}

type TimeseriesMetrics = Record<string, number>;
type ChartSeriesApiResponse = ChartSeriesData | TimeseriesMetrics;

@Injectable({ providedIn: 'root' })
export class AdminDashboardService {
  constructor(private readonly apiClient: ApiClientService) {}

  getDashboardKpis(): Observable<KpiData[]> {
    return forkJoin({
      totalRevenue: this.getTotalRevenue().pipe(catchError(() => of(0))),
      totalBookings: this.getTotalBookings().pipe(catchError(() => of(0))),
      activeUsers: this.getActiveUsers().pipe(catchError(() => of(0))),
      occupancyRate: this.getOccupancyRate().pipe(catchError(() => of(0))),
    }).pipe(
      map(({ totalRevenue, totalBookings, activeUsers, occupancyRate }): KpiData[] => [
        {
          title: 'Total Revenue',
          value: this.formatCurrency(totalRevenue),
          icon: 'payments',
          subtitle: 'Admin API · /api/admin/dashboard/total-revenue',
        },
        {
          title: 'Total Bookings',
          value: this.formatCount(totalBookings),
          icon: 'confirmation_number',
          subtitle: 'Admin API · /api/admin/dashboard/total-bookings',
        },
        {
          title: 'Active Users',
          value: this.formatCount(activeUsers),
          icon: 'group',
          subtitle: 'Admin API · /api/admin/dashboard/active-users',
        },
        {
          title: 'Occupancy Rate',
          value: this.formatPercentage(occupancyRate),
          icon: 'chair',
          subtitle: 'Admin API · /api/admin/dashboard/occupancy-rate',
        },
      ]),
    );
  }

  getDashboardSummary(): Observable<AdminDashboardSummaryResponse> {
    return this.apiClient.get<AdminDashboardSummaryResponse>('/api/admin/dashboard');
  }

  getTotalRevenue(): Observable<number> {
    return this.apiClient.get<number>('/api/admin/dashboard/total-revenue');
  }

  getTotalBookings(): Observable<number> {
    return this.apiClient.get<number>('/api/admin/dashboard/total-bookings');
  }

  getActiveUsers(): Observable<number> {
    return this.apiClient.get<number>('/api/admin/dashboard/active-users');
  }

  getOccupancyRate(): Observable<number> {
    return this.apiClient.get<number>('/api/admin/dashboard/occupancy-rate');
  }

  getMonthlyRevenue(): Observable<ChartSeriesData> {
    return this.apiClient
      .get<ChartSeriesApiResponse>('/api/admin/dashboard/monthly-revenue')
      .pipe(map((response) => this.toChartSeriesData(response)));
  }

  getWeeklyBookings(): Observable<ChartSeriesData> {
    return this.apiClient
      .get<ChartSeriesApiResponse>('/api/admin/dashboard/weekly-bookings')
      .pipe(map((response) => this.toChartSeriesData(response)));
  }

  private formatCurrency(value: number): string {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD',
      maximumFractionDigits: 0,
    }).format(value);
  }

  private formatCount(value: number): string {
    return new Intl.NumberFormat('en-US', { maximumFractionDigits: 0 }).format(value);
  }

  private formatPercentage(value: number): string {
    return `${value.toFixed(1)}%`;
  }

  private toChartSeriesData(response: ChartSeriesApiResponse): ChartSeriesData {
    if (this.isChartSeriesData(response)) {
      return response;
    }

    const entries = Object.entries(response);
    return {
      labels: entries.map(([label]) => label),
      data: entries.map(([, value]) => value),
    };
  }

  private isChartSeriesData(value: ChartSeriesApiResponse): value is ChartSeriesData {
    if (typeof value !== 'object' || value === null) {
      return false;
    }

    const candidate = value as Partial<ChartSeriesData>;
    return Array.isArray(candidate.labels) && Array.isArray(candidate.data);
  }
}
