import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { RatingModule } from 'primeng/rating';
import { ToastModule } from 'primeng/toast';
import { SelectModule } from 'primeng/select';
import { DatePickerModule } from 'primeng/datepicker';
import { InputTextModule } from 'primeng/inputtext';
import { Paginator } from 'primeng/paginator';
import { MessageService } from 'primeng/api';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { BookingService, BookingListDto } from '../../../core/services/booking.service';

@Component({
    selector: 'app-worker-bookings',
    standalone: true,
    imports: [CommonModule, RouterModule, FormsModule, TableModule, TagModule, ButtonModule, DialogModule, RatingModule, ToastModule, SelectModule, DatePickerModule, InputTextModule, Paginator, TranslatePipe],
    providers: [MessageService],
    template: `
        <div class="card">
            <p-toast />
            <h2>{{ 'BOOKING.MY_BOOKINGS' | translate }}</h2>
            <div class="flex flex-wrap gap-3 mb-3 align-items-center">
                <p-select [options]="statusOptions" [(ngModel)]="filters.status" optionLabel="label" optionValue="value" [placeholder]="'COMMON.STATUS' | translate" [showClear]="true" styleClass="w-14rem"></p-select>
                <p-select [options]="typeOptions" [(ngModel)]="filters.bookingType" optionLabel="label" optionValue="value" [placeholder]="'COMMON.BOOKING_TYPE' | translate" [showClear]="true" styleClass="w-12rem"></p-select>
                <p-datepicker [(ngModel)]="filters.fromDate" dateFormat="dd/mm/yy" [showIcon]="true" [placeholder]="'COMMON.FROM_DATE' | translate" [showClear]="true"></p-datepicker>
                <p-datepicker [(ngModel)]="filters.toDate" dateFormat="dd/mm/yy" [showIcon]="true" [placeholder]="'COMMON.TO_DATE' | translate" [showClear]="true"></p-datepicker>
                <span class="p-input-icon-right">
                    <i class="pi pi-search"></i>
                    <input pInputText [(ngModel)]="filters.search" [placeholder]="'COMMON.SEARCH_BY_NAME' | translate" class="w-16rem" />
                </span>
                <p-button icon="pi pi-filter"[label]="'COMMON.SEARCH' | translate" size="small" (onClick)="applyFilters()"></p-button>
                <p-button icon="pi pi-times" [label]="'COMMON.DELETE' | translate" size="small" severity="secondary" (onClick)="resetFilters()"></p-button>
            </div>
            <p-table [value]="bookings()" [rows]="pageSize" [tableStyle]="{ 'min-width': '50rem' }">
                <ng-template #header>
                    <tr>
                        <th>{{ 'COMMON.ID' | translate }}</th>
                        <th>{{ 'BOOKING.HOMEOWNER' | translate }}</th>
                        <th>{{ 'BOOKING.START_DATE' | translate }}</th>
                        <th>{{ 'BOOKING.MONTHLY_SALARY' | translate }}</th>
                        <th>{{ 'BOOKING.TYPE' | translate }}</th>
                        <th>{{ 'BOOKING.QUANTITY' | translate }}</th>
                        <th>{{ 'BOOKING.TOTAL_AMOUNT' | translate }}</th>
                        <th>{{ 'BOOKING.STATUS' | translate }}</th>
                    </tr>
                </ng-template>
                <ng-template #body let-booking>
                    <tr>
                        <td>{{ booking.id }}</td>
                        <td>{{ booking.homeownerName }}</td>
                        <td>{{ booking.startDate | date:'shortDate' }}</td>
                        <td>{{ (booking.bookingType == 0)? booking.dailySalary:(booking.bookingType == 1)?booking.monthlySalary:booking.hourlySalary | currency:booking.currencyCode:'':'1.0-0' }} {{ booking.currencyCode }}</td>
                        <td>{{ getBookingTypeLabel(booking.bookingType) }}</td>
                        <td>{{ booking.quantity}}</td>
                        <td>{{ booking.totalAmount | currency:booking.currencyCode:'':'1.0-0' }} {{ booking.currencyCode }}</td>
                        <td><p-tag [value]="getStatusLabel(booking.status)" [severity]="getStatusSeverity(booking.status)"></p-tag></td>
                    </tr>
                </ng-template>
            </p-table>
            <div *ngIf="totalCount > pageSize" class="mt-3">
                <p-paginator [totalRecords]="totalCount" [rows]="pageSize" [first]="(page - 1) * pageSize" (onPageChange)="onPageChange($event)"></p-paginator>
            </div>

            <p-dialog [(visible)]="showReviewDialog" [header]="'REVIEW.TITLE' | translate" [modal]="true">
                <div class="flex flex-column gap-3">
                    <p-rating [(ngModel)]="reviewRating"></p-rating>
                    <textarea pInputTextarea [(ngModel)]="reviewComment" rows="3" class="w-full"
                        [placeholder]="'REVIEW.COMMENT' | translate"></textarea>
                    <p-button [label]="'REVIEW.SUBMIT' | translate" (onClick)="submitReview()" [loading]="isSubmitting"></p-button>
                </div>
            </p-dialog>
        </div>
    `
})
export class WorkerBookings implements OnInit {
    private bookingService = inject(BookingService);
    private translate = inject(TranslateService);
    private messageService = inject(MessageService);
    bookings = signal<BookingListDto[]>([]);

    statusOptions: any[] = [];
    typeOptions: any[] = [];

    filters: any = { status: null, bookingType: null, fromDate: null, toDate: null, search: '' };

    page = 1;
    pageSize = 10;
    totalCount = 0;

    showReviewDialog = false;
    reviewRating = 5;
    reviewComment = '';
    isSubmitting = false;
    private currentBooking: BookingListDto | null = null;

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
        this.bookingService.getWorkerBookings(params).subscribe({
            next: (res) => { this.bookings.set(res.data || []); this.totalCount = res.totalCount || 0; this.pageSize = res.pageSize || this.pageSize; }
        });
    }

    openReview(b: BookingListDto) {
        this.currentBooking = b;
        this.reviewRating = 5;
        this.reviewComment = '';
        this.showReviewDialog = true;
    }

    submitReview() {
        if (!this.currentBooking) return;
        this.isSubmitting = true;
        this.bookingService.reviewBooking(this.currentBooking.id, this.reviewRating, this.reviewComment || null).subscribe({
            next: () => {
                this.isSubmitting = false;
                this.showReviewDialog = false;
                if (this.currentBooking) this.currentBooking.hasReviewed = true;
                this.messageService.add({ severity: 'success', detail: this.translate.instant('REVIEW.SUCCESS') });
            },
            error: () => {
                this.isSubmitting = false;
                this.messageService.add({ severity: 'error', detail: this.translate.instant('REVIEW.ERROR') });
            }
        });
    }

    getBookingTypeLabel(type: number): string {
        return [this.translate.instant('WORKER_DETAIL.DAILY'),
                this.translate.instant('WORKER_DETAIL.MONTHLY'),
                this.translate.instant('WORKER_DETAIL.HOURLY')][type] || '—';
    }
    getStatusLabel(status: number): string {
        return [this.translate.instant('BOOKING_DETAIL.STATUS_LABEL_PENDING'),
            this.translate.instant('ADMIN.WORKER_CONFIRMED'),
            this.translate.instant('BOOKING_DETAIL.STATUS_LABEL_WAITING_PAYMENT'),
            this.translate.instant('BOOKING_DETAIL.STATUS_LABEL_PAID'),
            this.translate.instant('BOOKING_DETAIL.STATUS_LABEL_ACTIVE'),
            this.translate.instant('BOOKING_DETAIL.STATUS_LABEL_COMPLETED'),
            this.translate.instant('BOOKING_DETAIL.STATUS_LABEL_CANCELLED'),
            this.translate.instant('BOOKING_DETAIL.STATUS_LABEL_REPLACEMENT'),
            this.translate.instant('BOOKING_DETAIL.STATUS_LABEL_REVIEW')][status] || '—';
    }

    getStatusSeverity(status: number): any {
        const s: { [k: number]: any } = {
            0: 'warn', 1: 'info', 2: 'warn', 3: 'success', 4: 'info', 5: 'success', 6: 'danger', 7: 'warn', 8: 'info'
        };
        return s[status] || 'secondary';
    }
}