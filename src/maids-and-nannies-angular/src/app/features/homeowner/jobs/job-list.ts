import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { TranslatePipe } from '@ngx-translate/core';
import { ApiService } from '../../../core/services/api.service';

@Component({
  selector: 'app-job-list',
  standalone: true,
  imports: [CommonModule, RouterModule, CardModule, ButtonModule, TableModule, TagModule, TranslatePipe],
  template: `
    <div class="card">
      <div class="flex align-items-center justify-content-between mb-4">
        <h2 class="m-0">{{ 'JOB_POST.MY_POSTS' | translate }}</h2>
        <p-button [label]="'JOB_POST.CREATE' | translate" icon="pi pi-plus" routerLink="/homeowner/jobs/create"></p-button>
      </div>
      <p-table [value]="posts()" [rows]="10">
        <ng-template #header>
          <tr>
            <th>{{ 'COMMON.ID' | translate }}</th>
            <th>{{ 'JOB_POST.DESCRIPTION' | translate }}</th>
            <th>{{ 'BOOKING.TYPE' | translate }}</th>
            <th>{{ 'BOOKING.START_DATE' | translate }}</th>
            <th>{{ 'BOOKING.STATUS' | translate }}</th>
            <th>{{ 'WORKER.CURRENCY' | translate }}</th>
            <th>{{ 'JOB_POST.APPLICATIONS' | translate }}</th>
            <th>{{ 'ADMIN.TABLE_ACTIONS' | translate }}</th>
          </tr>
        </ng-template>
        <ng-template #body let-p>
          <tr>
            <td>{{ p.id }}</td>
            <td>{{ p.description | slice:0:50 }}...</td>
            <td>{{ getBookingTypeLabel(p.bookingType) }}</td>
            <td>{{ p.startDate | date:'shortDate' }}</td>
            <td><p-tag [value]="getStatusLabel(p.postStatus)" [severity]="getStatusSeverity(p.postStatus)"></p-tag></td>
            <td>{{ p.currencyCode }}</td>
            <td>{{ p.applicationCount }}</td>
            <td>
              <p-button icon="pi pi-eye" [rounded]="true" [text]="true" routerLink="/homeowner/jobs/{{p.id}}"></p-button>
              <p-button icon="pi pi-users" [rounded]="true" [text]="true" routerLink="/homeowner/jobs/{{p.id}}/applications"></p-button>
            </td>
          </tr>
        </ng-template>
      </p-table>
    </div>
  `
})
export class JobList implements OnInit {
  private api = inject(ApiService);
  posts = signal<any[]>([]);

  ngOnInit() { this.api.getMyJobPosts().subscribe({ next: (d) => this.posts.set(d) }); }

  getBookingTypeLabel(t: number) { return ['يومي', 'شهري', 'ساعي'][t] || '—'; }
  getStatusLabel(s: number) { return ['بانتظار المراجعة', 'معتمد', 'مرفوض'][s] || '—'; }
  getStatusSeverity(s: number) { return ['warn', 'success', 'danger'][s] || 'secondary'; }
}