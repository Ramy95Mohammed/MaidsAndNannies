import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { ButtonModule } from 'primeng/button';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { SelectModule } from 'primeng/select';
import { DatePickerModule } from 'primeng/datepicker';
import { InputTextModule } from 'primeng/inputtext';
import { Paginator } from 'primeng/paginator';
import { MessageService, ConfirmationService } from 'primeng/api';
import { ApiService } from '../../../core/services/api.service';
import { BookingService } from '@/core/services/booking.service';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';

@Component({
    selector: 'app-admin-bookings',
    standalone: true,
    imports: [CommonModule, FormsModule, TableModule, TagModule, ButtonModule, ToastModule, ConfirmDialogModule, SelectModule, DatePickerModule, InputTextModule, Paginator, TranslatePipe],
    providers: [MessageService, ConfirmationService],
    template: `
        <p-toast />
        <p-confirmdialog />
        <div class="card">
            <h2>{{ 'ADMIN.MANAGE_BOOKINGS' | translate }}</h2>

            <div class="flex flex-wrap gap-3 mb-3 align-items-center">
                <p-select [options]="statusOptions" [(ngModel)]="filters.status" optionLabel="label" optionValue="value" [placeholder]="'COMMON.STATUS' | translate" [showClear]="true" styleClass="w-14rem"></p-select>
                <p-select [options]="paidOptions" [(ngModel)]="filters.isPaid" optionLabel="label" optionValue="value" [placeholder]="'COMMON.PAYMENT_STATUS' | translate" [showClear]="true" styleClass="w-12rem"></p-select>
                <p-datepicker [(ngModel)]="filters.fromDate" dateFormat="dd/mm/yy" [showIcon]="true" [placeholder]="'COMMON.FROM_DATE' | translate" [showClear]="true"></p-datepicker>
                <p-datepicker [(ngModel)]="filters.toDate" dateFormat="dd/mm/yy" [showIcon]="true" [placeholder]="'COMMON.TO_DATE' | translate" [showClear]="true"></p-datepicker>
                <span class="p-input-icon-right">
                    <i class="pi pi-search"></i>
                    <input pInputText [(ngModel)]="filters.search" [placeholder]="'COMMON.SEARCH_WORKER_OWNER' | translate" class="w-18rem" />
                </span>
                <p-button icon="pi pi-filter" [label]="'COMMON.SEARCH' | translate" size="small" (onClick)="applyFilters()"></p-button>
                <p-button icon="pi pi-times" [label]="'COMMON.DELETE' | translate" size="small" severity="secondary" (onClick)="resetFilters()"></p-button>
            </div>

            <p-table [value]="bookings()" [rows]="pageSize" [tableStyle]="{ 'min-width': '100rem' }">
                <ng-template #header>
                    <tr>
                        <th>#</th>
                        <th>{{ 'ADMIN.TABLE_HOMEOWNER' | translate }}</th>
                        <th>{{ 'ADMIN.TABLE_WORKER' | translate }}</th>
                        <th>{{ 'ADMIN.TABLE_SALARY' | translate }}</th>
                        <th>{{ 'ADMIN.TABLE_TYPE' | translate }}</th>
                        <th>{{ 'ADMIN.TABLE_QUANTITY' | translate }}</th>
                        <th>{{ 'ADMIN.TABLE_TOTAL' | translate }}</th>
                        <th>{{ 'ADMIN.TABLE_TOTAL_AFTER_CONVERSION' | translate }}</th>
                        <th>{{ 'ADMIN.TABLE_COMMISSION' | translate }}</th>
                        <th>{{ 'ADMIN.TABLE_PAYMENT_AMOUNT' | translate }}</th>
                        <th>{{ 'ADMIN.TABLE_STATUS' | translate }}</th>
                        <th>{{ 'ADMIN.TABLE_REPLACEMENT' | translate }}</th>
                        <th>{{ 'ADMIN.TABLE_ACTIONS' | translate }}</th>
                    </tr>
                </ng-template>
                <ng-template #body let-b>
                    <tr>
                        <td>{{ b.id }}</td>
                        <td>{{ b.homeownerName }}</td>
                        <td>{{ b.workerName }}</td>
                        <td>{{ (b.bookingType==0)? b.dailySalary:(b.bookingType==1)?b.monthlySalary:b.hourlySalary | currency:b.currencyCode:'':'1.0-0' }} {{ b.currencyCode }}</td>
                        <td>{{ getBookingTypeLabel(b.bookingType) }}</td>
                        <td>{{(b.bookingType == 1)?"__": b.quantity }}</td>
                        <td>{{ b.totalAmount | currency:b.currencyCode:'':'1.0-0' }} {{ b.currencyCode }}</td>
                        <td>{{ b.totalAmountAfterConversion | currency:'EGP':'code':'1.0-0' }}</td>
                        <td>{{ b.commissionAmount | currency:'EGP':'code':'1.0-0' }}</td>
                        <td>{{ b.paymentAmount | currency:'EGP':'code':'1.0-0' }}</td>
                        <td><p-tag [value]="statusLabel(b.status)" [severity]="statusSeverity(b.status)"></p-tag></td>
                        <td>{{ b.replacementCount }}/{{b.maxReplacement}}</td>
                        <td>
                            <div class="flex gap-1">
                                <p-button *ngIf="b.status === 0" [label]="'ADMIN.CONFIRM_WORKER' | translate" size="small" (onClick)="confirmWorker(b.id)"></p-button>
                                <p-button *ngIf="b.status === 1" [label]="'ADMIN.REQUEST_PAYMENT' | translate" size="small" (onClick)="requestPayment(b.id)"></p-button>
                                <p-button *ngIf="b.status === 2 || b.status === 8" [label]="'ADMIN.CONFIRM_PAYMENT' | translate" size="small" severity="success" (onClick)="confirmPayment(b.id)"></p-button>
                                <p-button *ngIf="b.status === 3" [label]="'ADMIN.START_WORK' | translate" size="small" (onClick)="startWork(b.id)"></p-button>
                                <p-button *ngIf="b.status === 4" [label]="'ADMIN.COMPLETE' | translate" severity="success" (onClick)="completeWork(b.id)"></p-button>
                                <p-button *ngIf="b.status === 7" [label]="'ADMIN.CONFIRM_REPLACEMENT' | translate" size="small" severity="warn" (onClick)="confirmWorker(b.id)"></p-button>
                            </div>
                        </td>
                    </tr>
                </ng-template>
            </p-table>
            <div *ngIf="totalCount > pageSize" class="mt-3">
                <p-paginator [totalRecords]="totalCount" [rows]="pageSize" [first]="(page - 1) * pageSize" (onPageChange)="onPageChange($event)"></p-paginator>
            </div>
        </div>
    `
})
export class AdminBookings implements OnInit {
    private apiService = inject(ApiService);
    private bookingService = inject(BookingService);
    private messageService = inject(MessageService);
    private confirmationService = inject(ConfirmationService);
    private translate = inject(TranslateService);

    bookings = signal<any[]>([]);

    statusOptions: any[] = [];
    paidOptions: any[] = [];

    filters: any = { status: null, isPaid: null, fromDate: null, toDate: null, search: '' };

    page = 1;
    pageSize = 15;
    totalCount = 0;

    ngOnInit() {
        this.statusOptions = [
            { label: this.translate.instant('BOOKING_DETAIL.STATUS_LABEL_PENDING'), value: 0 },
            { label: this.translate.instant('ADMIN.WORKER_CONFIRMED'), value: 1 },
            { label: this.translate.instant('BOOKING_DETAIL.STATUS_LABEL_WAITING_PAYMENT'), value: 2 },
            { label: this.translate.instant('BOOKING_DETAIL.STATUS_LABEL_PAID'), value: 3 },
            { label: this.translate.instant('BOOKING_DETAIL.STATUS_LABEL_ACTIVE'), value: 4 },
            { label: this.translate.instant('BOOKING_DETAIL.STATUS_LABEL_COMPLETED'), value: 5 },
            { label: this.translate.instant('BOOKING_DETAIL.STATUS_LABEL_CANCELLED'), value: 6 },
            { label: this.translate.instant('BOOKING_DETAIL.STATUS_LABEL_REPLACEMENT'), value: 7 },
            { label: this.translate.instant('BOOKING_DETAIL.STATUS_LABEL_REVIEW'), value: 8 }
        ];
        this.paidOptions = [
            { label: this.translate.instant('BOOKING_DETAIL.STATUS_LABEL_PAID'), value: true },
            { label: this.translate.instant('COMMON.UNPAID'), value: false }
        ];
        this.load();
    }

    applyFilters() { this.page = 1; this.load(); }
    resetFilters() { this.filters = { status: null, isPaid: null, fromDate: null, toDate: null, search: '' }; this.page = 1; this.load(); }

    onPageChange(event: any) {
        this.page = (event.first / event.rows) + 1;
        this.pageSize = event.rows;
        this.load();
    }

    private toParam(d: Date): string {
        return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;
    }

    load() {
        const params: any = {};
        if (this.filters.status !== null && this.filters.status !== undefined) params.status = this.filters.status;
        if (this.filters.isPaid !== null && this.filters.isPaid !== undefined) params.isPaid = this.filters.isPaid;
        if (this.filters.fromDate) params.fromDate = this.toParam(this.filters.fromDate);
        if (this.filters.toDate) params.toDate = this.toParam(this.filters.toDate);
        if (this.filters.search && this.filters.search.trim()) params.search = this.filters.search.trim();
        params.page = this.page;
        params.pageSize = this.pageSize;
        this.apiService.getAllBookings(params).subscribe({
            next: (res) => { this.bookings.set(res.data || []); this.totalCount = res.totalCount || 0; this.pageSize = res.pageSize || this.pageSize; }
        });
    }

    confirmWorker(id: number) {
        this.confirmationService.confirm({
            message: this.translate.instant('ADMIN.CONFIRM_WORKER_MSG'),
            header: this.translate.instant('ADMIN.CONFIRM'),
            icon: 'pi pi-exclamation-triangle',
            accept: () => {
                this.bookingService.confirmWorker(id).subscribe({
                    next: () => { this.messageService.add({ severity:'success', detail: this.translate.instant('ADMIN.WORKER_CONFIRMED') }); this.load(); }
                });
            }
        });
    }

    requestPayment(id: number) {
        this.bookingService.requestPayment(id).subscribe({
            next: () => { this.messageService.add({ severity:'success', detail: this.translate.instant('ADMIN.PAYMENT_REQUESTED') }); this.load(); }
        });
    }

    startWork(id: number) {
        this.bookingService.startWork(id).subscribe({
            next: () => { this.messageService.add({ severity:'success', detail: this.translate.instant('ADMIN.WORK_STARTED') }); this.load(); }
        });
    }

    completeWork(id: number) {
        this.bookingService.completeWork(id).subscribe({
            next: () => { this.messageService.add({ severity:'success', detail: this.translate.instant('ADMIN.BOOKING_COMPLETED') }); this.load(); }
        });
    }

    getBookingTypeLabel(type: number): string {
        return [this.translate.instant('WORKER_DETAIL.DAILY'),
                this.translate.instant('WORKER_DETAIL.MONTHLY'),
                this.translate.instant('WORKER_DETAIL.HOURLY')][type] || '—';
    }

    confirmPayment(id: number) {
        this.confirmationService.confirm({
            message: this.translate.instant('BOOKING_DETAIL.CONFIRM_PAYMENT_MSG'),
            header: this.translate.instant('ADMIN.CONFIRM'),
            icon: 'pi pi-exclamation-triangle',
            accept: () => {
                this.bookingService.confirmPayment(id).subscribe({
                    next: () => { this.messageService.add({ severity: 'success', detail: this.translate.instant('ADMIN.TOAST_PAYMENT_CONFIRMED') }); this.load(); }
                });
            }
        });
    }

    statusLabel(s: number): string {
        return [this.translate.instant('BOOKING_DETAIL.STATUS_LABEL_PENDING'),
                this.translate.instant('ADMIN.WORKER_CONFIRMED'),
                this.translate.instant('BOOKING_DETAIL.STATUS_LABEL_WAITING_PAYMENT'),
                this.translate.instant('BOOKING_DETAIL.STATUS_LABEL_PAID'),
                this.translate.instant('BOOKING_DETAIL.STATUS_LABEL_ACTIVE'),
                this.translate.instant('BOOKING_DETAIL.STATUS_LABEL_COMPLETED'),
                this.translate.instant('BOOKING_DETAIL.STATUS_LABEL_CANCELLED'),
                this.translate.instant('BOOKING_DETAIL.STATUS_LABEL_REPLACEMENT'),
                this.translate.instant('BOOKING_DETAIL.STATUS_LABEL_REVIEW')][s] || '—';
    }
    statusSeverity(s: number): string {
        return ['warn','info','warn','success','info','success','danger','warn','info'][s]||'secondary';
    }
}