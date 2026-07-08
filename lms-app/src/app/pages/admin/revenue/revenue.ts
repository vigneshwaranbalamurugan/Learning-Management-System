import { Component, OnInit, signal, inject, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { untilDestroyed } from '../../../rxjs/until-destroyed';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../environments/environment';
import { AnalyticsService } from '@services/analytics.service';
import { AdminAnalytics, RecentActivity } from '../../../models/analytics';

@Component({
  selector: 'app-admin-revenue',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './revenue.html'
})
export class AdminRevenue implements OnInit {
  private analyticsService = inject(AnalyticsService);
  private destroyRef = inject(DestroyRef);

  protected isLoading = signal(true);
  protected stats = signal<AdminAnalytics | null>(null);
  
  protected monthlyRevenue = signal<{ month: string; value: number; height: string }[]>([]);
  protected userGrowth = signal<{ month: string; value: number; height: string }[]>([]);
  protected enrollmentTrend = signal<{ month: string; value: number; height: string }[]>([]);
  protected recentActivities = signal<RecentActivity[]>([]);
  protected currentPage = signal(1);
  protected hasMoreActivities = signal(true);
  protected isLoadingActivities = signal(false);

  ngOnInit() {
    this.loadAdminAnalytics();
    this.loadActivitiesPage();
  }

  protected get visibleActivities(): RecentActivity[] {
    return this.recentActivities();
  }

  protected loadMoreActivities(): void {
    if (this.isLoadingActivities() || !this.hasMoreActivities() || this.recentActivities().length >= 30) return;
    this.currentPage.update(p => p + 1);
    this.loadActivitiesPage();
  }

  private loadActivitiesPage() {
    this.isLoadingActivities.set(true);
    this.analyticsService.getAdminRecentActivities(this.currentPage(), 5)
      .pipe(untilDestroyed(this.destroyRef))
      .subscribe({
        next: (activities) => {
          const newActivities = [...this.recentActivities(), ...activities];
          if (activities.length < 5 || newActivities.length >= 30) {
            this.hasMoreActivities.set(false);
          }
          this.recentActivities.set(newActivities.slice(0, 30));
          this.isLoadingActivities.set(false);
        },
        error: (err) => {
          console.error('Failed to load activities', err);
          this.isLoadingActivities.set(false);
        }
      });
  }

  private loadAdminAnalytics() {
    this.analyticsService.getAdminAnalytics()
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

            // 4. Recent Activities handled separately
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
