import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { DialogModule } from 'primeng/dialog';
import { FileUploadModule } from 'primeng/fileupload';
import { ToastModule } from 'primeng/toast';
import { SelectModule } from 'primeng/select';
import { RatingModule } from 'primeng/rating';
import { MessageService } from 'primeng/api';
import { ApiService } from '../../../core/services/api.service';
import { AuthService } from '../../../core/services/auth.service';

@Component({
    selector: 'app-homeowner-bookings',
    standalone: true,
    imports: [
        CommonModule, FormsModule, RouterModule, CardModule, ButtonModule,
        TableModule, TagModule, DialogModule, FileUploadModule, ToastModule,
        SelectModule, RatingModule
    ],
    providers: [MessageService],
    template: `
        <p-toast></p-toast>
        <div class="card">
            <h2>حجوزاتي</h2>

            <p-table [value]="bookings()" [rows]="10" [paginator]="true" [globalFilterFields]="['workerName','status']">
                <ng-template pTemplate="header">
                    <tr>
                        <th>العاملة</th>
                        <th>التاريخ</th>
                        <th>المبلغ</th>
                        <th>الحالة</th>
                        <th>إجراءات</th>
                    </tr>
                </ng-template>
                <ng-template pTemplate="body" let-booking>
                    <tr>
                        <td>{{ booking.workerName }}</td>
                        <td>{{ booking.startDate | date:'short' }}</td>
                        <td>{{ booking.monthlySalary | currency:'EGP':'symbol':'1.0-0' }}</td>
                        <td>
                            <p-tag [value]="getStatusLabel(booking.status)" [severity]="getStatusSeverity(booking.status)"></p-tag>
                        </td>
                        <td>
                            <p-button icon="pi pi-eye" [rounded]="true" [text]="true" (onClick)="viewBooking(booking.id)" pTooltip="عرض"></p-button>
                            <p-button *ngIf="booking.status === 4" icon="pi pi-star" [rounded]="true" [text]="true" (onClick)="openReviewDialog(booking)" pTooltip="تقييم"></p-button>
                        </td>
                    </tr>
                </ng-template>
            </p-table>

            <div *ngIf="bookings().length === 0" class="text-center py-8">
                <i class="pi pi-calendar text-4xl text-muted-color mb-4"></i>
                <p class="text-muted-color">لا توجد حجوزات بعد</p>
                <p-button label="ابحث عن عاملة" routerLink="/homeowner/workers" styleClass="mt-4"></p-button>
            </div>
        </div>

        <!-- Payment Dialog -->
        <p-dialog header="رفع إثبات الدفع" [(visible)]="showPaymentDialog" [modal]="true" [style]="{width: '450px'}">
            <div class="mb-3">
                <label class="block font-bold mb-2">طريقة الدفع</label>
                <p-select [(ngModel)]="paymentMethod" [options]="paymentMethods" optionLabel="label" optionValue="value" styleClass="w-full"></p-select>
            </div>
            <div class="mb-3">
                <label class="block font-bold mb-2">رقم المعاملة</label>
                <input pInputText [(ngModel)]="transactionId" class="w-full" placeholder="أدخل رقم المعاملة" />
            </div>
            <div class="mb-3">
                <label class="block font-bold mb-2">المبلغ</label>
                <input pInputText [(ngModel)]="paymentAmount" type="number" class="w-full" placeholder="المبلغ بالجنيه" />
            </div>
            <p-button label="إرسال" icon="pi pi-check" (onClick)="submitPayment()" styleClass="w-full"></p-button>
        </p-dialog>

        <!-- Review Dialog -->
        <p-dialog header="تقييم العاملة" [(visible)]="showReviewDialog" [modal]="true" [style]="{width: '450px'}">
            <div class="mb-3">
                <label class="block font-bold mb-2">التقييم</label>
                <p-rating [(ngModel)]="reviewRating"></p-rating>
            </div>
            <div class="mb-3">
                <label class="block font-bold mb-2">التعليق</label>
                <textarea pTextarea [(ngModel)]="reviewComment" rows="3" class="w-full" placeholder="اكتب تعليقك..."></textarea>
            </div>
            <p-button label="إرسال التقييم" icon="pi pi-check" (onClick)="submitReview()" styleClass="w-full"></p-button>
        </p-dialog>
    `
})
export class HomeownerBookings implements OnInit {
    private apiService = inject(ApiService);
    private authService = inject(AuthService);
    private messageService = inject(MessageService);

    bookings = signal<any[]>([]);
    showPaymentDialog = false;
    showReviewDialog = false;

    selectedBooking: any = null;
    paymentMethod = 0;
    transactionId = '';
    paymentAmount = 0;
    reviewRating = 5;
    reviewComment = '';

    paymentMethods = [
        { label: 'فودافون كاش', value: 0 },
        { label: 'انستاباي', value: 1 }
    ];

    ngOnInit() {
        this.loadBookings();
    }

    loadBookings() {
        this.apiService.getMyBookings().subscribe({
            next: (data) => this.bookings.set(data.Data || []),
            error: () => this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل تحميل الحجوزات' })
        });
    }

    getStatusLabel(status: number): string {
        const labels: { [k: number]: string } = {
            0: 'في الانتظار', 1: 'مقبول', 2: 'قيد العمل',
            3: 'مكتمل', 4: 'بانتظار الدفع', 5: 'مدفوع', 6: 'ملغي'
        };
        return labels[status] || 'غير معروف';
    }

    getStatusSeverity(status: number): 'success' | 'info' | 'warn' | 'danger' | 'secondary' | 'contrast' | undefined {
        const s: { [k: number]: any } = {
            0: 'warn', 1: 'info', 2: 'info', 3: 'success', 4: 'warn', 5: 'success', 6: 'danger'
        };
        return s[status] || 'secondary';
    }

    viewBooking(id: number) {
        window.location.href = '/homeowner/bookings/' + id;
    }

    openPaymentDialog(booking: any) {
        this.selectedBooking = booking;
        this.paymentAmount = booking.totalAmount;
        this.showPaymentDialog = true;
    }

    submitPayment() {
        if (!this.transactionId) {
            this.messageService.add({ severity: 'warn', summary: 'تنبيه', detail: 'أدخل رقم المعاملة' });
            return;
        }
        this.apiService.uploadPaymentProof(this.selectedBooking.id, {
            paymentMethod: this.paymentMethod,
            amount: this.paymentAmount,
            proofImageUrl: '',
            transactionReference: this.transactionId
        }).subscribe({
            next: () => {
                this.messageService.add({ severity: 'success', summary: 'تم', detail: 'تم رفع إثبات الدفع' });
                this.showPaymentDialog = false;
                this.loadBookings();
            },
            error: () => this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل رفع إثبات الدفع' })
        });
    }

    openReviewDialog(booking: any) {
        this.selectedBooking = booking;
        this.reviewRating = 5;
        this.reviewComment = '';
        this.showReviewDialog = true;
    }

    submitReview() {
        this.apiService.createReview({
            bookingId: this.selectedBooking.id,
            rating: this.reviewRating,
            comment: this.reviewComment
        }).subscribe({
            next: () => {
                this.messageService.add({ severity: 'success', summary: 'تم', detail: 'تم التقييم بنجاح' });
                this.showReviewDialog = false;
                this.loadBookings();
            },
            error: () => this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل التقييم' })
        });
    }
}
