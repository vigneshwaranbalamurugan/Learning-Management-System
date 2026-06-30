import { Component, OnInit, signal, inject, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { untilDestroyed } from '../../../rxjs/until-destroyed';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../environments/environment';
import { AdminAnalytics, RecentActivity } from '../../../models/analytics';

@Component({
  selector: 'app-admin-revenue',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './revenue.html'
})
export class AdminRevenue implements OnInit {
  private http = inject(HttpClient);
  private destroyRef = inject(DestroyRef);

  protected isLoading = signal(true);
  protected stats = signal<AdminAnalytics | null>(null);
  
  protected monthlyRevenue = signal<{ month: string; value: number; height: string }[]>([]);
  protected userGrowth = signal<{ month: string; value: number; height: string }[]>([]);
  protected enrollmentTrend = signal<{ month: string; value: number; height: string }[]>([]);
  protected recentActivities = signal<RecentActivity[]>([]);
  protected showAllActivities = signal(false);

  ngOnInit() {
    this.loadAdminAnalytics();
  }

  protected get visibleActivities(): RecentActivity[] {
    return this.showAllActivities()
      ? this.recentActivities()
      : this.recentActivities().slice(0, 5);
  }

  protected toggleActivities(): void {
    this.showAllActivities.set(!this.showAllActivities());
  }

  private loadAdminAnalytics() {
    this.http.get<AdminAnalytics>(`${environment.apiUrl}/Analytics/admin`)
      .pipe(untilDestroyed(this.destroyRef))
      .subscribe({
        next: (data) => {
          this.stats.set(data);
          
          if (data) {
            // 1. Monthly Revenue
            if (data.monthlyRevenue) {
              const maxRevenue = Math.max(...data.monthlyRevenue.map(r => r.revenue), 0);
              const mapped = data.monthlyRevenue.map(r => ({
                month: r.month,
                value: r.revenue,
                height: maxRevenue > 0 ? `${(r.revenue / maxRevenue) * 85 + 15}%` : '15%'
              }));
              this.monthlyRevenue.set(mapped);
            } else {
              this.monthlyRevenue.set([]);
            }

            // 2. User Growth
            if (data.userGrowth) {
              const maxUsers = Math.max(...data.userGrowth.map(r => r.count), 0);
              const mapped = data.userGrowth.map(r => ({
                month: r.month,
                value: r.count,
                height: maxUsers > 0 ? `${(r.count / maxUsers) * 85 + 15}%` : '15%'
              }));
              this.userGrowth.set(mapped);
            } else {
              this.userGrowth.set([]);
            }

            // 3. Enrollment Trend
            if (data.enrollmentTrend) {
              const maxEnrollments = Math.max(...data.enrollmentTrend.map(r => r.count), 0);
              const mapped = data.enrollmentTrend.map(r => ({
                month: r.month,
                value: r.count,
                height: maxEnrollments > 0 ? `${(r.count / maxEnrollments) * 85 + 15}%` : '15%'
              }));
              this.enrollmentTrend.set(mapped);
            } else {
              this.enrollmentTrend.set([]);
            }

            // 4. Recent Activities
            this.recentActivities.set(data.recentActivities || []);
          }
          
          this.isLoading.set(false);
        },
        error: (err) => {
          console.error('Failed to load analytics', err);
          this.isLoading.set(false);
        }
      });
  }
}
