import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';
import { TranslatePipe } from '@ngx-translate/core';
import { ApiService } from '../../../core/services/api.service';

@Component({
    selector: 'app-admin-payments',
    standalone: true,
    imports: [CommonModule, CardModule, ButtonModule, TableModule, TagModule, ToastModule, TranslatePipe],
    providers: [MessageService],
    template: `
        <p-toast></p-toast>
        <div class="card">
            <h2>إدارة المدفوعات</h2>

            <p-table [value]="payments()" [rows]="10" [paginator]="true">
                <ng-template pTemplate="header">
                    <tr>
                        <th>رقم الحجز</th>
                        <th>الصاحبة</th>
                        <th>المبلغ</th>
                        <th>العمولة</th>
                        <th>طريقة الدفع</th>
                        <th>رقم المعاملة</th>
                        <th>الحالة</th>
                        <th>إجراءات</th>
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
                            <p-tag [value]="payment.isConfirmed ? 'مؤكد' : 'في الانتظار'" [severity]="payment.isConfirmed ? 'success' : 'warn'"></p-tag>
                        </td>
                        <td>
                            <p-button *ngIf="!payment.confirmed" label="تأكيد" icon="pi pi-check" [rounded]="true" [text]="true" (onClick)="confirmPayment(payment.id)"></p-button>
                            <p-button *ngIf="!payment.confirmed" label="رفض" icon="pi pi-times" [rounded]="true" [text]="true" severity="danger" (onClick)="rejectPayment(payment.id)"></p-button>
                        </td>
                    </tr>
                </ng-template>
            </p-table>

            <div *ngIf="payments().length === 0" class="text-center py-8">
                <p class="text-muted-color">لا توجد مدفوعات معلقة</p>
            </div>
        </div>
    `
})
export class AdminPayments implements OnInit {
    private apiService = inject(ApiService);
    private messageService = inject(MessageService);

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
        return method === 0 ? 'فودافون كاش' : 'انستاباي';
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
