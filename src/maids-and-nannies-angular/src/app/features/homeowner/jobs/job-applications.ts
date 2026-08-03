import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { ToastModule } from 'primeng/toast';
import { RatingModule } from 'primeng/rating';
import { ConfirmationService, MessageService } from 'primeng/api';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { ApiService } from '../../../core/services/api.service';
import { FormsModule } from '@angular/forms';
import { ConfirmDialogModule } from 'primeng/confirmdialog';

@Component({
  selector: 'app-job-applications',
  standalone: true,
    imports: [CommonModule, RouterModule, CardModule, ButtonModule, TableModule, TagModule, ToastModule, RatingModule, TranslatePipe, FormsModule, ConfirmDialogModule],
  providers: [MessageService, ConfirmationService],
  template: `
    <p-toast />
        <p-confirmdialog />

    <div class="card">
      <a routerLink="/homeowner/jobs" class="text-primary cursor-pointer"><i class="pi pi-arrow-left mr-2"></i>{{ 'COMMON.BACK' | translate }}</a>
      <h2 class="mt-4">{{ 'JOB_POST.APPLICATIONS' | translate }}</h2>
           <p-table [value]="applications()" [rows]="10" [paginator]="true">
        <ng-template #header>
          <tr>
            <th>{{ 'ADMIN.NAME' | translate }}</th>
            <th>{{ 'REVIEW.RATING' | translate }}</th>
            <th>{{ 'JOB_POST.MESSAGE' | translate }}</th>
            <th>{{ 'JOB_POST.DATE' | translate }}</th>
            <th>{{ 'ADMIN.TABLE_STATUS' | translate }}</th>
            <th>{{ 'ADMIN.TABLE_ACTIONS' | translate }}</th>
          </tr>
        </ng-template>
        <ng-template #body let-a>
          <tr>
            <td>{{ a.workerName }}</td>
            <td><p-rating [(ngModel)]="a.workerRating" [readonly]="true"></p-rating> ({{ a.workerReviews }})</td>
            <td>{{ a.message || '—' }}</td>
            <td>{{ a.createdAt | date:'short' }}</td>
            <td><p-tag [value]="getStatusLabel(a.status)" [severity]="getStatusSeverity(a.status)"></p-tag></td>
            <td>
               <p-button *ngIf="a.status === 0" [label]="'JOB_POST.ACCEPT_CREATE' | translate" icon="pi pi-check" size="small" severity="success" (onClick)="confirmAccept(a)"></p-button>
            </td>
          </tr>
        </ng-template>
      </p-table>
    </div>
  `
})
export class JobApplications implements OnInit {
  private api = inject(ApiService);
  private route = inject(ActivatedRoute);
  private msg = inject(MessageService);
  private confirm = inject(ConfirmationService);
    private translate = inject(TranslateService);

  private router = inject(Router);
  applications = signal<any[]>([]);
  postId = 0;

  ngOnInit() {
    this.postId = Number(this.route.snapshot.paramMap.get('id'));
    this.load();
  }

  load() { this.api.getJobApplications(this.postId).subscribe({ next: (d) => this.applications.set(d) }); }

  accept(appId: number) {
    this.api.acceptApplication(this.postId, appId).subscribe({
      next: (res) => {
        this.msg.add({ severity: 'success', detail: 'تم قبول الطلب' });
        this.router.navigate(['/homeowner/bookings', res.bookingId]);
      },
      error: () => this.msg.add({ severity: 'error', detail: 'فشل قبول الطلب' })
    });
  }

   

 confirmAccept(a: any) {
    const key = a.hasActiveBookingForPost
      ? 'JOB_POST.ACCEPT_CONFIRM_MSG_ACTIVE'
      : 'JOB_POST.ACCEPT_CONFIRM_MSG';
    this.confirm.confirm({
      message: this.translate.instant(key),
      header: this.translate.instant('JOB_POST.ACCEPT_CONFIRM_TITLE'),
      acceptLabel: this.translate.instant('JOB_POST.ACCEPT_CREATE'),
      rejectLabel: this.translate.instant('COMMON.CANCEL'),
      accept: () => this.accept(a.id)
    });
  }

  getStatusLabel(s: number) { return ['بانتظار', 'مقبول', 'مرفوض'][s] || '—'; }
  getStatusSeverity(s: number) { return ['warn', 'success', 'danger'][s] || 'secondary'; }
}