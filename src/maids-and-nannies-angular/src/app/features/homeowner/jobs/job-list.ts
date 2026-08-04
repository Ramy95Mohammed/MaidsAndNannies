import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { MessageService, ConfirmationService } from 'primeng/api';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { ApiService } from '../../../core/services/api.service';

@Component({
  selector: 'app-job-list',
  standalone: true,
  imports: [CommonModule, RouterModule, CardModule, ButtonModule, TableModule, TagModule, ToastModule, ConfirmDialogModule, TranslatePipe],
  providers: [MessageService, ConfirmationService],
  template: `
    <div class="card">
      <p-confirmdialog></p-confirmdialog>
      <div class="flex align-items-center justify-content-between mb-4">
        <h2 class="m-0">{{ 'JOB_POST.MY_POSTS' | translate }}</h2>
        <p-button [label]="'JOB_POST.CREATE' | translate" icon="pi pi-plus" routerLink="/homeowner/jobs/create"></p-button>
      </div>
      <p-table [value]="posts()" [rows]="10" [paginator]="true">
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
              <p-button icon="pi pi-pencil" [rounded]="true" [text]="true" routerLink="/homeowner/jobs/{{p.id}}/edit"></p-button>
              <p-button icon="pi pi-trash" [rounded]="true" [text]="true" severity="danger" (onClick)="confirmDelete(p)"></p-button>
            </td>
          </tr>
        </ng-template>
      </p-table>
    </div>
  `
})
export class JobList implements OnInit {
  private api = inject(ApiService);
  private translate = inject(TranslateService);
  private msg = inject(MessageService);
  private confirm = inject(ConfirmationService);
  posts = signal<any[]>([]);

  ngOnInit() { this.api.getMyJobPosts().subscribe({ next: (d) => this.posts.set(d) }); }

  getBookingTypeLabel(t: number) { return [this.translate.instant('WORKER_DETAIL.DAILY'), this.translate.instant('WORKER_DETAIL.MONTHLY'), this.translate.instant('WORKER_DETAIL.HOURLY')][t] || '—'; }
  getStatusLabel(s: number) { return [this.translate.instant('JOB_POST.STATUS_REVIEW_PENDING'), this.translate.instant('JOB_POST.STATUS_APPROVED'), this.translate.instant('WORKER_PROFILE.VERIFICATION_REJECTED')][s] || '—'; }
  getStatusSeverity(s: number) { return ['warn', 'success', 'danger'][s] || 'secondary'; }

  confirmDelete(p: any) {
    this.confirm.confirm({
      message: this.translate.instant('JOB_POST.DELETE_CONFIRM'),
      header: this.translate.instant('COMMON.CONFIRM'),
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.api.deleteJobPost(p.id).subscribe({
          next: () => {
            this.msg.add({ severity: 'success', detail: this.translate.instant('JOB_POST.DELETED') });
            this.posts.set(this.posts().filter(x => x.id !== p.id));
          },
          error: (er) => this.msg.add({ severity: 'error', detail: er.error?.message || this.translate.instant('JOB_POST.DELETE_FAILED') })
        });
      }
    });
  }
}