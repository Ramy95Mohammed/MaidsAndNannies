import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { PaginatorModule } from 'primeng/paginator';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { BookingService, BookingListDto } from '../../../core/services/booking.service';
import { DatePicker } from "primeng/datepicker";
import { Select } from "primeng/select";

@Component({
    selector: 'app-homeowner-bookings',
    standalone: true,
    imports: [CommonModule, RouterModule, FormsModule, TableModule, TagModule, ButtonModule, InputTextModule, PaginatorModule, TranslatePipe, DatePicker, Select],
    template: `
        <div class="card">
            <div class="flex justify-content-between mb-4">
                <h2>{{ 'BOOKING.MY_BOOKINGS' | translate }}</h2>
            </div>
            <div class="flex flex-wrap gap-3 mb-3 align-items-center">
                <p-select [options]="statusOptions" [(ngModel)]="filters.status" optionLabel="label" optionValue="value" [placeholder]="'COMMON.STATUS' | translate" [showClear]="true" styleClass="w-14rem"></p-select>
                <p-select [options]="typeOptions" [(ngModel)]="filters.bookingType" optionLabel="label" optionValue="value" [placeholder]="'COMMON.BOOKING_TYPE' | translate" [showClear]="true" styleClass="w-12rem"></p-select>
                <span class="p-input-icon-left p-date">
                    <i class="pi pi-calendar"></i>
                    <p-datepicker [(ngModel)]="filters.fromDate" dateFormat="dd/mm/yy" [showIcon]="true" [placeholder]="'COMMON.FROM_DATE' | translate" [showClear]="true"></p-datepicker>
                    <p-datepicker [(ngModel)]="filters.toDate" dateFormat="dd/mm/yy" [showIcon]="true" [placeholder]="'COMMON.TO_DATE' | translate" [showClear]="true"></p-datepicker>
                </span>
                <span class="p-input-icon-right">
                    <i class="pi pi-search"></i>
                    <input pInputText [(ngModel)]="filters.search" [placeholder]="'COMMON.SEARCH_BY_NAME' | translate" class="w-16rem" />
                </span>
                <p-button icon="pi pi-filter" [label]="'COMMON.SEARCH' | translate" size="small" (onClick)="applyFilters()"></p-button>
                <p-button icon="pi pi-times" [label]="'COMMON.DELETE' | translate" size="small" severity="secondary" (onClick)="resetFilters()"></p-button>
            </div>
            <p-table [value]="bookings()" [rows]="pageSize" [tableStyle]="{ 'min-width': '50rem' }">
                <ng-template #header>
                    <tr>
                        <th>{{ 'COMMON.ID' | translate }}</th>
                        <th>{{ 'WORKER.REGISTER' | translate }}</th>
                        <th>{{ 'BOOKING.START_DATE' | translate }}</th>
                        <th>{{ 'BOOKING.SALARY' | translate }}</th>
                        <th>{{ 'BOOKING.TYPE' | translate }}</th>
                        <th>{{ 'BOOKING.QUANTITY' | translate }}</th>
                        <th>{{ 'BOOKING.TOTAL_AMOUNT' | translate }}</th>
                        <th>{{ 'BOOKING.STATUS' | translate }}</th>
                        <th>{{ 'COMMON.TABLE_ACTIONS' | translate }}</th>
                    </tr>
                </ng-template>
                <ng-template #body let-booking>
                    <tr>
                        <td>{{ booking.id }}</td>
                        <td>{{ booking.workerName }}</td>
                        <td>{{ booking.startDate | date:'shortDate' }}</td>
                        @if(booking.bookingType == 0) { <td>
                            {{ booking.dailySalary | currency:booking.currencyCode:'':'1.0-0' }}
                            {{ booking.currencyCode }}
                            </td>}
                        @if(booking.bookingType == 1) { <td>{{ booking.monthlySalary | currency:booking.currencyCode:'':'1.0-0' }}
                             {{ booking.currencyCode }}
                        </td>}
                        @if(booking.bookingType == 2) { <td>{{ booking.hourlySalary | currency:booking.currencyCode:'':'1.0-0' }}
                             {{ booking.currencyCode }}
                        </td>}
                        <td>{{ getBookingTypeLabel(booking.bookingType) }}</td>
                        <td>{{  booking.quantity }}</td>
                        <td>{{ booking.totalAmount | currency:booking.currencyCode:'':'1.0-0' }} {{ booking.currencyCode }}</td>
                        <td><p-tag [value]="getStatusLabel(booking.status)" [severity]="getStatusSeverity(booking.status)"></p-tag></td>
                        <td>
                            <p-button [routerLink]="['/homeowner/bookings', booking.id]" [label]="'COMMON.VIEW' | translate" size="small"></p-button>
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
export class HomeownerBookings implements OnInit {
    private bookingService = inject(BookingService);
    private translate = inject(TranslateService);
    bookings = signal<BookingListDto[]>([]);

    statusOptions: any[] = [];
    typeOptions: any[] = [];

    filters: any = { status: null, bookingType: null, fromDate: null, toDate: null, search: '' };

    page = 1;
    pageSize = 10;
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
        this.typeOptions = [
            { label: this.translate.instant('WORKER_DETAIL.DAILY'), value: 0 },
            { label: this.translate.instant('WORKER_DETAIL.MONTHLY'), value: 1 },
            { label: this.translate.instant('WORKER_DETAIL.HOURLY'), value: 2 }
        ];
        this.loadBookings();
    }

    applyFilters() { this.page = 1; this.loadBookings(); }
    resetFilters() { this.filters = { status: null, bookingType: null, fromDate: null, toDate: null, search: '' }; this.page = 1; this.loadBookings(); }

    onPageChange(event: any) {
        this.page = (event.first / event.rows) + 1;
        this.pageSize = event.rows;
        this.loadBookings();
    }

    private toParam(d: Date): string {
        return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;
    }

    loadBookings() {
        const params: any = {};
        if (this.filters.status !== null && this.filters.status !== undefined) params.status = this.filters.status;
        if (this.filters.bookingType !== null && this.filters.bookingType !== undefined) params.bookingType = this.filters.bookingType;
        if (this.filters.fromDate) params.fromDate = this.toParam(this.filters.fromDate);
        if (this.filters.toDate) params.toDate = this.toParam(this.filters.toDate);
        if (this.filters.search && this.filters.search.trim()) params.search = this.filters.search.trim();
        params.page = this.page;
        params.pageSize = this.pageSize;
        this.bookingService.getMyBookings(params).subscribe({
            next: (res) => { this.bookings.set(res.data || []); this.totalCount = res.totalCount || 0; this.pageSize = res.pageSize || this.pageSize; }
        });
    }

    getBookingTypeLabel(type: number): string {
        return [this.translate.instant('WORKER_DETAIL.DAILY'),
                this.translate.instant('WORKER_DETAIL.MONTHLY'),
                this.translate.instant('WORKER_DETAIL.HOURLY')][type] || '—';
    }

    getStatusLabel(status: number): string {
        const labels: { [k: number]: string } = {
            0: this.translate.instant('BOOKING_DETAIL.STATUS_LABEL_PENDING'),
            1: this.translate.instant('ADMIN.WORKER_CONFIRMED'),
            2: this.translate.instant('BOOKING_DETAIL.STATUS_LABEL_WAITING_PAYMENT'),
            3: this.translate.instant('BOOKING_DETAIL.STATUS_LABEL_PAID'),
            4: this.translate.instant('BOOKING_DETAIL.STATUS_LABEL_ACTIVE'),
            5: this.translate.instant('BOOKING_DETAIL.STATUS_LABEL_COMPLETED'),
            6: this.translate.instant('BOOKING_DETAIL.STATUS_LABEL_CANCELLED'),
            7: this.translate.instant('BOOKING_DETAIL.STATUS_LABEL_REPLACEMENT'),
            8: this.translate.instant('BOOKING_DETAIL.STATUS_LABEL_REVIEW')
        };
        return labels[status] || this.translate.instant('COMMON.UNKNOWN');
    }

    getStatusSeverity(status: number): any {
        const s: { [k: number]: any } = {
            0: 'warn', 1: 'info', 2: 'warn', 3: 'success', 4: 'info', 5: 'success', 6: 'danger', 7: 'warn', 8: 'info'
        };
        return s[status] || 'secondary';
    }
}