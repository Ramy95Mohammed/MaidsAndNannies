import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { ApiService } from '../../../core/services/api.service';

@Component({
    selector: 'app-admin-payments',
    standalone: true,
    imports: [CommonModule, CardModule, ButtonModule, TableModule, TagModule, ToastModule, TranslatePipe],
    providers: [MessageService],
    template: `
        <p-toast></p-toast>
        <div class="card">
            <h2>{{ 'ADMIN.MANAGE_PAYMENTS' | translate }}</h2>
            <p-table [value]="payments()" [rows]="10" [paginator]="true">
                <ng-template pTemplate="header">
                    <tr>
                        <th>{{ 'ADMIN.TABLE_BOOKING_ID' | translate }}</th>
                         <th>{{ 'ADMIN.TABLE_HOMEOWNER_PAYMENTS' | translate }}</th>
                        <th>{{ 'ADMIN.TABLE_AMOUNT' | translate }}</th>
                        <th>{{ 'ADMIN.TABLE_COMMISSION' | translate }}</th>
                        <th>{{ 'ADMIN.TABLE_PAYMENT_METHOD' | translate }}</th>
                        <th>{{ 'ADMIN.TABLE_TRANSACTION' | translate }}</th>
                        <th>{{ 'ADMIN.TABLE_STATUS' | translate }}</th>
                        <th>{{ 'ADMIN.TABLE_ACTIONS' | translate }}</th>
                    </tr>
                </ng-template>
                <ng-template pTemplate="body" let-payment>
                    <tr>
                        <td>#{{ payment.bookingId }}</td>
                        <td>{{ payment.homeownerName }}</td>
                        <td>{{ payment.amount | currency:'EGP':'symbol':'1.0-0' }}</td>
                        <td>{{ payment.commissionAmount | currency:'EGP':'symbol':'1.0-0' }}</td>
                        <td>{{ getPaymentMethodName(payment.paymentMethod) }}</td>
                        <td>{{ payment.transactionReference }}</td>
                        <td>
                            <p-tag [value]="payment.isConfirmed ? ('PAYMENT.CONFIRM' | translate) : ('BOOKING_DETAIL.STATUS_LABEL_PENDING' | translate)" [severity]="payment.isConfirmed ? 'success' : 'warn'"></p-tag>
                        </td>
                        <td>
                            <p-button *ngIf="!payment.confirmed" [label]="'ADMIN.CONFIRM' | translate" icon="pi pi-check" [rounded]="true" [text]="true" (onClick)="confirmPayment(payment.id)"></p-button>
                            <p-button *ngIf="!payment.confirmed" [label]="'ADMIN.REJECT' | translate" icon="pi pi-times" [rounded]="true" [text]="true" severity="danger" (onClick)="rejectPayment(payment.id)"></p-button>
                        </td>
                    </tr>
                </ng-template>
            </p-table>

            <div *ngIf="payments().length === 0" class="text-center py-8">
                <p class="text-muted-color">{{ 'ADMIN.NO_PENDING_PAYMENTS' | translate }}</p>
            </div>
        </div>
    `
})
export class AdminPayments implements OnInit {
    private apiService = inject(ApiService);
    private messageService = inject(MessageService);
    private translate = inject(TranslateService);

    payments = signal<any[]>([]);

    ngOnInit() {
        this.loadPayments();
    }

    loadPayments() {
        this.apiService.getPendingPayments().subscribe({            
            next: (data) => {                
                this.payments.set(data || [])},
            error: () => this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل تحميل المدفوعات' })
        });
    }

   getPaymentMethodName(method: number): string {
    return method === 0 ? this.translate.instant('PAYMENT.VODAFONE_CASH') : this.translate.instant('PAYMENT.INSTAPAY');
}
    confirmPayment(id: number) {
        this.apiService.confirmPayment(id).subscribe({
            next: () => {
                this.messageService.add({ severity: 'success', summary: 'تم', detail: 'تم تأكيد الدفع' });
                this.loadPayments();
            },
            error: () => this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل تأكيد الدفع' })
        });
    }

    rejectPayment(id: number) {
        this.apiService.rejectPayment(id).subscribe({
            next: () => {
                this.messageService.add({ severity: 'warn', summary: 'تم', detail: 'تم رفض الدفع' });
                this.loadPayments();
            },
            error: () => this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل رفض الدفع' })
        });
    }
}
