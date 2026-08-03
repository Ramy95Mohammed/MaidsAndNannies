import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CardModule } from 'primeng/card';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { ApiService } from '../../../core/services/api.service';

@Component({
  selector: 'app-worker-applications',
  standalone: true,
  imports: [CommonModule, CardModule, TableModule, TagModule, TranslatePipe],
  template: `
    <div class="card">
      <h2>{{ 'JOB_POST.MY_APPLICATIONS' | translate }}</h2>
           <p-table [value]="apps()" [rows]="10" [paginator]="true">
        <ng-template #header>
          <tr>
            <th>{{ 'COMMON.ID' | translate }}</th>
            <th>{{ 'JOB_POST.POST_ID' | translate }}</th>
            <th>{{ 'WORKER.MONTHLY_RATE' | translate }}</th>
            <th>{{ 'BOOKING.TYPE' | translate }}</th>
            <th>{{ 'JOB_POST.DATE' | translate }}</th>
            <th>{{ 'ADMIN.TABLE_STATUS' | translate }}</th>
          </tr>
        </ng-template>
        <ng-template #body let-a>
          <tr>
            <td>{{ a.id }}</td>
            <td>#{{ a.jobPostId }}</td>
            <td>{{ a.postMonthlySalary }}</td>
            <td>{{ getTypeLabel(a.postBookingType) }}</td>
            <td>{{ a.createdAt | date:'short' }}</td>
            <td><p-tag [value]="getStatusLabel(a.status)" [severity]="getStatusSeverity(a.status)"></p-tag></td>
          </tr>
        </ng-template>
      </p-table>
    </div>
  `
})
export class WorkerApplications implements OnInit {
  private api = inject(ApiService);
  private translate = inject(TranslateService);
  apps = signal<any[]>([]);

  ngOnInit() { this.api.getMyApplications().subscribe({ next: (d) => this.apps.set(d) }); }

  getTypeLabel(t: number) { return [this.translate.instant('WORKER_DETAIL.DAILY'), this.translate.instant('WORKER_DETAIL.MONTHLY'), this.translate.instant('WORKER_DETAIL.HOURLY')][t] || '—'; }
  getStatusLabel(s: number) { return [this.translate.instant('JOB_POST.APPLICATION_STATUS_PENDING'), this.translate.instant('JOB_POST.APPLICATION_STATUS_ACCEPTED'), this.translate.instant('JOB_POST.APPLICATION_STATUS_REJECTED')][s] || '—'; }
  getStatusSeverity(s: number) { return ['warn', 'success', 'danger'][s] || 'secondary'; }
}