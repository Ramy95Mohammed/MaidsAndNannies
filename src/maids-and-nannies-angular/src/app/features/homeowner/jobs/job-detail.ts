import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { CardModule } from 'primeng/card';
import { TagModule } from 'primeng/tag';
import { ButtonModule } from 'primeng/button';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { ApiService } from '../../../core/services/api.service';

@Component({
  selector: 'app-job-detail',
  standalone: true,
  imports: [CommonModule, RouterModule, CardModule, TagModule, ButtonModule, TranslatePipe],
  template: `
    <div class="card" *ngIf="post()">
      <a routerLink="/homeowner/jobs" class="text-primary cursor-pointer"><i class="pi pi-arrow-left mr-2"></i>{{ 'COMMON.BACK' | translate }}</a>
      <div class="mt-4">
        <div class="flex align-items-center gap-3 mb-4">
          <h2 class="m-0">{{ 'JOB_POST.DETAIL' | translate }} #{{ post().id }}</h2>
          <p-tag [value]="statusLabel(post().postStatus)" [severity]="statusSeverity(post().postStatus)"></p-tag>
        </div>
        <p-card>
          <p class="text-lg whitespace-pre-wrap">{{ post().description }}</p>
        </p-card>
        <div class="grid grid-cols-12 gap-4 mt-4">
          <div class="col-span-12 md:col-span-4"><strong>{{ 'WORKER.MONTHLY_RATE' | translate }}:</strong> {{ post().monthlySalary }}</div>
          <div class="col-span-12 md:col-span-4"><strong>{{ 'WORKER.DAILY_RATE' | translate }}:</strong> {{ post().dailySalary }}</div>
          <div class="col-span-12 md:col-span-4"><strong>{{ 'WORKER.HOURLY_RATE' | translate }}:</strong> {{ post().hourlySalary }}</div>
          <div class="col-span-12 md:col-span-3"><strong>{{ 'BOOKING.TYPE' | translate }}:</strong> {{ getBookingTypeLabel(post().bookingType) }}</div>
          <div class="col-span-12 md:col-span-3"><strong>{{ 'BOOKING.COMMISSION_TYPE' | translate }}:</strong> {{ post().commissionType === 0 ? ('BOOKING.ONETIME' | translate) : ('BOOKING.SUBSCRIPTION' | translate) }}</div>
          <div class="col-span-12 md:col-span-3"><strong>{{ 'BOOKING.START_DATE' | translate }}:</strong> {{ post().startDate | date:'shortDate' }}</div>
          <div class="col-span-12 md:col-span-3"><strong>{{ 'BOOKING.QUANTITY' | translate }}:</strong> {{ post().quantity }}</div>
          <div class="col-span-12 md:col-span-4"><strong>{{ 'WORKER.CURRENCY' | translate }}:</strong> {{ post().currencyCode }}</div>
                    <div class="col-span-12 md:col-span-4"><strong>{{ 'WORKER.SPECIALIZATION' | translate }}:</strong> {{ getSpecsLabel(post()) }}</div>
        </div>
        <div class="mt-4" *ngIf="post().rejectionReason">
          <p-card header="{{ 'JOB_POST.REJECTION_REASON' | translate }}" styleClass="bg-red-50">
            <p>{{ post().rejectionReason }}</p>
          </p-card>
        </div>
        <div class="mt-4 text-center">
          <p-button [label]="'JOB_POST.VIEW_APPLICATIONS' | translate" icon="pi pi-users" routerLink="/homeowner/jobs/{{post().id}}/applications"></p-button>
        </div>
      </div>
    </div>
  `
})
export class JobDetail implements OnInit {
  private api = inject(ApiService);
  private translate = inject(TranslateService);
  private route = inject(ActivatedRoute);
  post = signal<any>(null);

  ngOnInit() {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    this.api.getJobPostById(id).subscribe({ next: (d) => this.post.set(d) });
  }

  getBookingTypeLabel(t: number) { return [this.translate.instant('WORKER_DETAIL.DAILY'), this.translate.instant('WORKER_DETAIL.MONTHLY'), this.translate.instant('WORKER_DETAIL.HOURLY')][t] || '—'; }
  statusLabel(s: number) { return [this.translate.instant('JOB_POST.STATUS_REVIEW_PENDING'), this.translate.instant('JOB_POST.STATUS_APPROVED'), this.translate.instant('WORKER_PROFILE.VERIFICATION_REJECTED')][s] || '—'; }
  statusSeverity(s: number) { return ['warn', 'success', 'danger'][s] || 'secondary'; }
    getSpecsLabel(p: any): string {
    const map: { [key: number]: string } = {
      0: this.translate.instant('SPECIALIZATIONS.CLEANING'),
      1: this.translate.instant('SPECIALIZATIONS.COOKING'),
      2: this.translate.instant('SPECIALIZATIONS.CHILDCARE'),
      3: this.translate.instant('SPECIALIZATIONS.ELDERLYCARE'),
      4: this.translate.instant('SPECIALIZATIONS.GENERALHOUSEKEEPING')
    };
    const list = [p.specialization, ...(p.specializations || [])];
    return [...new Set(list)].map((s: number) => map[s] || s).join('، ');
  }
}