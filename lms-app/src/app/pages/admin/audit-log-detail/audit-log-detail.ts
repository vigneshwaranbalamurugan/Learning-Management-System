import { Component, OnInit, inject, DestroyRef, signal } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../environments/environment';
import { untilDestroyed } from '../../../rxjs/until-destroyed';
import { AuditLogResponse } from '../../../models/log';
import { ToastService } from '../../../services/toast.service';

@Component({
  selector: 'app-audit-log-detail',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './audit-log-detail.html',
  providers: [DatePipe]
})
export class AuditLogDetail implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private http = inject(HttpClient);
  private destroyRef = inject(DestroyRef);
  private toast = inject(ToastService);
  private datePipe = inject(DatePipe);

  protected isLoading = signal(true);
  protected log = signal<AuditLogResponse | null>(null);
  
  protected changedFields = signal<{ field: string; old: string; new: string }[]>([]);

  constructor() {}

  ngOnInit() {
    this.route.params.pipe(untilDestroyed(this.destroyRef)).subscribe(params => {
      const id = params['id'];
      if (id) {
        this.loadLogDetails(id);
      } else {
        this.goBack();
      }
    });
  }

  private loadLogDetails(id: string) {
    this.isLoading.set(true);
    this.http.get<AuditLogResponse>(`${environment.apiUrl}/Logs/audit/${id}`)
      .pipe(untilDestroyed(this.destroyRef))
      .subscribe({
        next: (data) => {
          this.log.set(data);
          this.parseValues(data.oldValues, data.newValues);
          this.isLoading.set(false);
        },
        error: (err) => {
          console.error('Failed to load audit log', err);
          this.toast.showError('Audit log not found.');
          this.isLoading.set(false);
          this.goBack();
        }
      });
  }

  private parseValues(oldValStr: string, newValStr: string) {
    let oldObj: any = {};
    let newObj: any = {};
    
    try {
      if (oldValStr && oldValStr !== 'null') oldObj = JSON.parse(oldValStr);
    } catch (e) {
      console.warn('Failed to parse oldValues JSON');
    }

    try {
      if (newValStr && newValStr !== 'null') newObj = JSON.parse(newValStr);
    } catch (e) {
      console.warn('Failed to parse newValues JSON');
    }

    const allKeys = new Set([...Object.keys(oldObj), ...Object.keys(newObj)]);
    const fields: { field: string; old: string; new: string }[] = [];

    allKeys.forEach(key => {
      const oldVal = oldObj[key] !== undefined ? String(oldObj[key]) : '';
      const newVal = newObj[key] !== undefined ? String(newObj[key]) : '';
      
      // Only show fields that have changed or exist in one of them
      if (oldVal !== newVal || oldVal || newVal) {
        fields.push({
          field: key,
          old: oldVal,
          new: newVal
        });
      }
    });

    this.changedFields.set(fields);
  }

  protected goBack() {
    this.router.navigate(['/admin/logs'], { queryParams: { tab: 'audit' }, queryParamsHandling: 'merge' });
  }
}
