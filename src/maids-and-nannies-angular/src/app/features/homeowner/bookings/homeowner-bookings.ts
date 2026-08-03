import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { PaginatorModule } from 'primeng/paginator';
import { TranslatePipe } from '@ngx-translate/core';
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
                <p-select [options]="statusOptions" [(ngModel)]="filters.status" optionLabel="label" optionValue="value" placeholder="الحالة" [showClear]="true" styleClass="w-14rem"></p-select>
                <p-select [options]="typeOptions" [(ngModel)]="filters.bookingType" optionLabel="label" optionValue="value" placeholder="نوع الحجز" [showClear]="true" styleClass="w-12rem"></p-select>
                <span class="p-input-icon-left p-date">
                    <i class="pi pi-calendar"></i>
                    <p-datepicker [(ngModel)]="filters.fromDate" dateFormat="dd/mm/yy" [showIcon]="true" placeholder="من تاريخ" [showClear]="true"></p-datepicker>
                    <p-datepicker [(ngModel)]="filters.toDate" dateFormat="dd/mm/yy" [showIcon]="true" placeholder="إلى تاريخ" [showClear]="true"></p-datepicker>
                </span>
                <span class="p-input-icon-right">
                    <i class="pi pi-search"></i>
                    <input pInputText [(ngModel)]="filters.search" placeholder="بحث عن اسم العاملة" class="w-16rem" />
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
                        <td>{{ (booking.bookingType == 1)?' __ ' : booking.quantity }}</td>
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
    bookings = signal<BookingListDto[]>([]);

    statusOptions = [
        { label: 'في الانتظار', value: 0 }, { label: 'تم تأكيد العاملة', value: 1 },
        { label: 'بانتظار الدفع', value: 2 }, { label: 'مدفوع', value: 3 }, { label: 'نشط', value: 4 },
        { label: 'مكتمل', value: 5 }, { label: 'ملغي', value: 6 }, { label: 'طلب استبدال', value: 7 },
        { label: 'قيد المراجعة', value: 8 }
    ];
    typeOptions = [{ label: 'يومي', value: 0 }, { label: 'شهري', value: 1 }, { label: 'ساعي', value: 2 }];

    filters: any = { status: null, bookingType: null, fromDate: null, toDate: null, search: '' };

    page = 1;
    pageSize = 10;
    totalCount = 0;

    ngOnInit() { this.loadBookings(); }

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
        return ['يومي', 'شهري', 'ساعي'][type] || '—';
    }

    getStatusLabel(status: number): string {
        const labels: { [k: number]: string } = {
            0: 'في الانتظار', 1: 'تم تأكيد العاملة', 2: 'بانتظار الدفع',
            3: 'مدفوع', 4: 'نشط', 5: 'مكتمل', 6: 'ملغي', 7: 'طلب استبدال', 8: 'قيد المراجعة'
        };
        return labels[status] || 'غير معروف';
    }

    getStatusSeverity(status: number): any {
        const s: { [k: number]: any } = {
            0: 'warn', 1: 'info', 2: 'warn', 3: 'success', 4: 'info', 5: 'success', 6: 'danger', 7: 'warn', 8: 'info'
        };
        return s[status] || 'secondary';
    }
}