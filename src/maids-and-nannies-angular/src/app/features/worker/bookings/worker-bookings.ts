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
import { MessageService } from 'primeng/api';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { BookingService, BookingListDto } from '../../../core/services/booking.service';

@Component({
    selector: 'app-worker-bookings',
    standalone: true,
    imports: [CommonModule, RouterModule, FormsModule, TableModule, TagModule, ButtonModule, DialogModule, RatingModule, ToastModule, TranslatePipe],
    providers: [MessageService],
    template: `
        <div class="card">
            <p-toast />
            <h2>{{ 'BOOKING.MY_BOOKINGS' | translate }}</h2>
            <p-table [value]="bookings()" [rows]="10" [tableStyle]="{ 'min-width': '50rem' }">
                <ng-template #header>
                    <tr>
                        <th>{{ 'COMMON.ID' | translate }}</th>
                        <th>{{ 'BOOKING.START_DATE' | translate }}</th>
                        <th>{{ 'BOOKING.MONTHLY_SALARY' | translate }}</th>
                        <th>{{ 'BOOKING.TYPE' | translate }}</th>
                        <th>{{ 'BOOKING.QUANTITY' | translate }}</th>
                        <th>{{ 'BOOKING.TOTAL_AMOUNT' | translate }}</th>
                        <th>{{ 'BOOKING.STATUS' | translate }}</th>
                        <!-- <th>{{ 'REVIEW.RATING' | translate }}</th> -->
                    </tr>
                </ng-template>
                <ng-template #body let-booking>
                    <tr>
                        <td>{{ booking.id }}</td>
                        <td>{{ booking.startDate | date:'shortDate' }}</td>
                        <td>{{ (booking.bookingType == 0)? booking.dailySalary:(booking.bookingType == 1)?booking.monthlySalary:booking.hourlySalary | currency:booking.currencyCode:'':'1.0-0' }} {{ booking.currencyCode }}</td>
                        <td>{{ getBookingTypeLabel(booking.bookingType) }}</td>
                        <td>{{ (booking.bookingType == 1)?"__": booking.quantity }}</td>
                        <td>{{ booking.totalAmount | currency:booking.currencyCode:'':'1.0-0' }} {{ booking.currencyCode }}</td>
                        <td><p-tag [value]="getStatusLabel(booking.status)" [severity]="getStatusSeverity(booking.status)"></p-tag></td>
                        <!-- <td>
                            <p-button *ngIf="booking.status === 5 && !booking.hasReviewed"
                                      [label]="'REVIEW.RATE_HOMEOWNER' | translate" size="small" severity="success"
                                      (onClick)="openReview(booking)"></p-button>
                            <span *ngIf="booking.hasReviewed" class="text-sm text-muted-color">{{ 'REVIEW.DONE' | translate }}</span>
                        </td> -->
                    </tr>
                </ng-template>
            </p-table>

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

    showReviewDialog = false;
    reviewRating = 5;
    reviewComment = '';
    isSubmitting = false;
    private currentBooking: BookingListDto | null = null;

    ngOnInit() {
        this.bookingService.getWorkerBookings().subscribe({
            next: (data) => this.bookings.set(data)
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
            this.translate.instant('ADMIN.WORKER_CONFIRMED'),
            this.translate.instant('ADMIN.WORKER_CONFIRMED'),
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