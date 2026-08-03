import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { TagModule } from 'primeng/tag';
import { ChipModule } from 'primeng/chip';
import { ToastModule } from 'primeng/toast';
import { DialogModule } from 'primeng/dialog';
import { FormsModule } from '@angular/forms';
import { SelectModule } from 'primeng/select';
import { InputNumberModule } from 'primeng/inputnumber';
import { InputTextModule } from 'primeng/inputtext';
import { Paginator } from 'primeng/paginator';
import { MessageService } from 'primeng/api';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { ApiService } from '../../../core/services/api.service';

@Component({
  selector: 'app-job-browse',
  standalone: true,
  imports: [CommonModule, RouterModule, CardModule, ButtonModule, TagModule, ChipModule, ToastModule, DialogModule, FormsModule, SelectModule, InputNumberModule, InputTextModule, Paginator, TranslatePipe],
  providers: [MessageService],
  template: `
    <p-toast />
    <div class="card">
      <h2>{{ 'JOB_POST.BROWSE' | translate }}</h2>

      <div class="flex flex-wrap gap-3 mb-3 align-items-center">
        <p-select [options]="specOptions" [(ngModel)]="filters.specialization" optionLabel="label" optionValue="value" [placeholder]="'ADMIN.SPECIALIZATION' | translate" [showClear]="true" styleClass="w-14rem"></p-select>
        <p-select [options]="typeOptions" [(ngModel)]="filters.bookingType" optionLabel="label" optionValue="value" [placeholder]="'COMMON.BOOKING_TYPE' | translate" [showClear]="true" styleClass="w-12rem"></p-select>
        <span class="flex align-items-center gap-2">
          <span class="text-sm text-muted-color">{{ 'COMMON.FROM' | translate }}</span>
          <p-inputnumber [(ngModel)]="filters.minMonthlySalary" mode="decimal" [minFractionDigits]="0" [placeholder]="'COMMON.MIN_SALARY' | translate" styleClass="w-9rem"></p-inputnumber>
          <span class="text-sm text-muted-color">{{ 'COMMON.TO' | translate }}</span>
          <p-inputnumber [(ngModel)]="filters.maxMonthlySalary" mode="decimal" [minFractionDigits]="0" [placeholder]="'COMMON.MAX_SALARY' | translate" styleClass="w-9rem"></p-inputnumber>
        </span>
        <p-button icon="pi pi-filter" [label]="'COMMON.SEARCH' | translate" size="small" (onClick)="applyFilters()"></p-button>
        <p-button icon="pi pi-times" [label]="'COMMON.DELETE' | translate" size="small" severity="secondary" (onClick)="resetFilters()"></p-button>
      </div>

      <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        <div *ngFor="let post of posts()" class="card cursor-pointer hover:shadow-lg transition-duration-300">
          <div class="mb-3">
            <p class="text-sm whitespace-pre-wrap line-height-3">{{ post.description | slice:0:150 }}...</p>
          </div>
          <div class="flex flex-wrap gap-2 mb-3">
            <p-chip [label]="getSpecLabel(post.specialization)"></p-chip>
            <p-chip *ngFor="let s of post.specializations || []" [label]="getSpecLabel(s)"></p-chip>
            <p-chip [label]="getTypeLabel(post.bookingType)"></p-chip>
          </div>
          <div class="flex align-items-center justify-content-between">
            <div>
              <span class="text-xl font-bold text-primary">{{ post.monthlySalary }} {{ post.currencyCode }}</span>
              <span class="text-muted-color text-sm">{{ post.bookingType === 0 ? ('COMMON.PER_DAY' | translate) : post.bookingType === 2 ? ('COMMON.PER_HOUR' | translate) : ('COMMON.PER_MONTH' | translate) }}</span>
            </div>
            <p-button [label]="'WORKERS.BOOK' | translate" icon="pi pi-send" [rounded]="true" (onClick)="openApplyDialog(post)"></p-button>
          </div>
          <div class="text-sm text-muted-color mt-2">
            <i class="pi pi-calendar mr-1"></i>{{ post.startDate | date:'shortDate' }}
          </div>
        </div>
      </div>
      <div *ngIf="posts().length === 0" class="text-center py-8">
        <i class="pi pi-briefcase text-4xl text-muted-color mb-4"></i>
        <p class="text-muted-color">{{ 'COMMON.NO_DATA' | translate }}</p>
      </div>
      <div *ngIf="totalCount > pageSize" class="mt-3">
        <p-paginator [totalRecords]="totalCount" [rows]="pageSize" [first]="(page - 1) * pageSize" (onPageChange)="onPageChange($event)"></p-paginator>
      </div>
    </div>

    <p-dialog [(visible)]="showDialog" [header]="'JOB_POST.APPLY' | translate" [modal]="true">
      <div class="mb-3">
        <label class="block font-bold mb-2">{{ 'JOB_POST.MESSAGE' | translate }}</label>
        <textarea pInputTextarea [(ngModel)]="applyMessage" rows="4" class="w-full" [placeholder]="'JOB_POST.MESSAGE_PLACEHOLDER' | translate"></textarea>
      </div>
      <p-button [label]="'JOB_POST.SUBMIT_APPLICATION' | translate" icon="pi pi-send" styleClass="w-full" (onClick)="submitApplication()"></p-button>
    </p-dialog>
  `
})
export class JobBrowse implements OnInit {
  private api = inject(ApiService);
  private msg = inject(MessageService);
  private translate = inject(TranslateService);
  posts = signal<any[]>([]);

  specOptions: any[] = [];
  typeOptions: any[] = [];

  filters: any = { specialization: null, bookingType: null, minMonthlySalary: null, maxMonthlySalary: null };

  page = 1;
  pageSize = 12;
  totalCount = 0;

  showDialog = false;
  selectedPostId = 0;
  applyMessage = '';

  ngOnInit() {
    this.specOptions = [
      { label: this.translate.instant('SPECIALIZATIONS.CLEANING'), value: 0 },
      { label: this.translate.instant('SPECIALIZATIONS.COOKING'), value: 1 },
      { label: this.translate.instant('SPECIALIZATIONS.CHILDCARE'), value: 2 },
      { label: this.translate.instant('SPECIALIZATIONS.ELDERLYCARE'), value: 3 },
      { label: this.translate.instant('SPECIALIZATIONS.GENERALHOUSEKEEPING'), value: 4 }
    ];
    this.typeOptions = [
      { label: this.translate.instant('WORKER_DETAIL.DAILY'), value: 0 },
      { label: this.translate.instant('WORKER_DETAIL.MONTHLY'), value: 1 },
      { label: this.translate.instant('WORKER_DETAIL.HOURLY'), value: 2 }
    ];
    this.load();
  }

  applyFilters() { this.page = 1; this.load(); }
  resetFilters() { this.filters = { specialization: null, bookingType: null, minMonthlySalary: null, maxMonthlySalary: null }; this.page = 1; this.load(); }

  onPageChange(event: any) {
    this.page = (event.first / event.rows) + 1;
    this.pageSize = event.rows;
    this.load();
  }

  load() {
    const params: any = {};
    if (this.filters.specialization !== null && this.filters.specialization !== undefined) params.specialization = this.filters.specialization;
    if (this.filters.bookingType !== null && this.filters.bookingType !== undefined) params.bookingType = this.filters.bookingType;
    if (this.filters.minMonthlySalary !== null && this.filters.minMonthlySalary !== undefined) params.minMonthlySalary = this.filters.minMonthlySalary;
    if (this.filters.maxMonthlySalary !== null && this.filters.maxMonthlySalary !== undefined) params.maxMonthlySalary = this.filters.maxMonthlySalary;
    params.page = this.page;
    params.pageSize = this.pageSize;
    this.api.getApprovedJobPosts(params).subscribe({
      next: (res) => { this.posts.set(res.data || []); this.totalCount = res.totalCount || 0; this.pageSize = res.pageSize || this.pageSize; }
    });
  }

  openApplyDialog(post: any) { this.selectedPostId = post.id; this.applyMessage = ''; this.showDialog = true; }

  submitApplication() {
    this.api.applyForJob(this.selectedPostId, this.applyMessage).subscribe({
      next: () => { this.msg.add({ severity: 'success', detail: this.translate.instant('JOB_POST.APPLICATION_SUBMITTED') }); this.showDialog = false; },
      error: () => this.msg.add({ severity: 'error', detail: this.translate.instant('JOB_POST.APPLICATION_SUBMIT_FAILED') })
    });
  }

  getSpecLabel(v: number) { return [this.translate.instant('SPECIALIZATIONS.CLEANING'), this.translate.instant('SPECIALIZATIONS.COOKING'), this.translate.instant('SPECIALIZATIONS.CHILDCARE'), this.translate.instant('SPECIALIZATIONS.ELDERLYCARE'), this.translate.instant('SPECIALIZATIONS.GENERALHOUSEKEEPING')][v] || ''; }
  getTypeLabel(t: number) { return [this.translate.instant('WORKER_DETAIL.DAILY'), this.translate.instant('WORKER_DETAIL.MONTHLY'), this.translate.instant('WORKER_DETAIL.HOURLY')][t] || ''; }
}