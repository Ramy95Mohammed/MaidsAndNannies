import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { ButtonModule } from 'primeng/button';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { MessageService, ConfirmationService } from 'primeng/api';
import { ApiService } from '../../../core/services/api.service';
import { BookingService } from '@/core/services/booking.service';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';

interface BookingRow {
    id: number;
    homeownerName: string;
    workerName: string;
    monthlySalary: number;
    status: number;
    replacementCount: number;
    bookingType: number;
    quantity: number;
    totalAmount: number;
    commissionAmount: number;
}

@Component({
    selector: 'app-admin-bookings',
    standalone: true,
    imports: [CommonModule, ReactiveFormsModule, TableModule, TagModule, ButtonModule, ToastModule, ConfirmDialogModule , TranslatePipe],
    providers: [MessageService, ConfirmationService],
    template: `
        <p-toast />
        <p-confirmdialog />
        <div class="card">
            <h2>{{ 'ADMIN.MANAGE_BOOKINGS' | translate }}</h2>
            <p-table [value]="bookings()" [rows]="15" [tableStyle]="{ 'min-width': '65rem' }">
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
                        <td>{{ (b.bookingType == 0)? b.dailySalary:(b.bookingType == 1)?b.monthlySalary:b.hourlySalary | currency:b.currencyCode:'':'1.0-0' }} {{ b.currencyCode }}</td>
                        <td>{{ getBookingTypeLabel(b.bookingType) }}</td>
                        <td>{{(b.bookingType == 1)?"__": b.quantity }}</td>
                        <td>{{ b.totalAmount | currency:b.currencyCode:'':'1.0-0' }} {{ b.currencyCode }}</td>
                        <td>{{ b.totalAmountAfterConversion | currency:'EGP':'code':'1.0-0' }}</td>
                        <td>{{ b.commissionAmount | currency:'EGP':'code':'1.0-0' }}</td>
                        <td><p-tag [value]="statusLabel(b.status)" [severity]="statusSeverity(b.status)"></p-tag></td>
                        <td>{{ b.replacementCount }}/{{b.maxReplacement}}</td>
                        <td>
                            <div class="flex gap-1">
                                <p-button *ngIf="b.status === 0" [label]="'ADMIN.CONFIRM_WORKER' | translate" size="small" (onClick)="confirmWorker(b.id)"></p-button>
                                <p-button *ngIf="b.status === 1" [label]="'ADMIN.REQUEST_PAYMENT' | translate" size="small" (onClick)="requestPayment(b.id)"></p-button>
                                <p-button *ngIf="b.status === 3" [label]="'ADMIN.START_WORK' | translate" size="small" (onClick)="startWork(b.id)"></p-button>
                                <p-button *ngIf="b.status === 4" [label]="'ADMIN.COMPLETE' | translate" severity="success" (onClick)="completeWork(b.id)"></p-button>                                
                                <p-button *ngIf="b.status === 7" [label]="'ADMIN.CONFIRM_REPLACEMENT' | translate" size="small" severity="warn" (onClick)="confirmWorker(b.id)"></p-button>
                            </div>
                        </td>
                    </tr>
                </ng-template>
            </p-table>
        </div>
    `
})
export class AdminBookings implements OnInit {
    private apiService = inject(ApiService);
    private bookingService = inject(BookingService);
    private messageService = inject(MessageService);
    private confirmationService = inject(ConfirmationService);
    private translate = inject(TranslateService);

    bookings = signal<BookingRow[]>([]);

    ngOnInit() { this.load(); }

    load() {
        this.apiService.getAllBookings().subscribe({
            next: (data) => this.bookings.set(data)
        });
    }

    confirmWorker(id: number) {
        this.confirmationService.confirm({
            message: 'تأكيد العاملة لهذا الحجز؟',
            header: 'تأكيد',
            icon: 'pi pi-exclamation-triangle',
            accept: () => {
                this.bookingService.confirmWorker(id).subscribe({
                    next: () => { this.messageService.add({ severity:'success', detail:'تم تأكيد العاملة' }); this.load(); }
                });
            }
        });
    }

    requestPayment(id: number) {
        this.bookingService.requestPayment(id).subscribe({
            next: () => { this.messageService.add({ severity:'success', detail:'تم طلب الدفع' }); this.load(); }
        });
    }

    startWork(id: number) {
        this.bookingService.startWork(id).subscribe({
            next: () => { this.messageService.add({ severity:'success', detail:'تم بدء العمل' }); this.load(); }
        });
    }

    completeWork(id: number) {
        this.bookingService.completeWork(id).subscribe({
            next: () => { this.messageService.add({ severity:'success', detail:'تم إنهاء الحجز' }); this.load(); }
        });
    }
      getBookingTypeLabel(type: number): string {
    return [this.translate.instant('WORKER_DETAIL.DAILY'),
            this.translate.instant('WORKER_DETAIL.MONTHLY'),
            this.translate.instant('WORKER_DETAIL.HOURLY')][type] || '—';
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