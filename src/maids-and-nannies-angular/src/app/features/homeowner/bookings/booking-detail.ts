import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { CardModule } from 'primeng/card';
import { TagModule } from 'primeng/tag';
import { ButtonModule } from 'primeng/button';
import { SelectModule } from 'primeng/select';
import { InputTextModule } from 'primeng/inputtext';
import { FileUpload } from 'primeng/fileupload';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';
import { TranslatePipe } from '@ngx-translate/core';
import { BookingService, BookingDetailDto } from '../../../core/services/booking.service';

@Component({
    selector: 'app-booking-detail',
    standalone: true,
    imports: [
        CommonModule, ReactiveFormsModule, CardModule, TagModule,
        ButtonModule, SelectModule, InputTextModule, FileUpload,
        ToastModule, RouterModule, TranslatePipe
    ],
    providers: [MessageService],
    template: `
        <p-toast />
        <div class="card" *ngIf="booking">
            <h2>{{ 'BOOKING.MY_BOOKINGS' | translate }} #{{ booking.id }}</h2>

            <div class="grid grid-cols-12 gap-4 mt-4">
                <!-- Worker Info -->
                <div class="col-span-12 md:col-span-6">
                    <p-card header="{{ 'WORKER.REGISTER' | translate }}">
                        <p>{{ booking.workerFullName || '—' }}</p>
                        <p *ngIf="booking.workerPhone">{{ 'AUTH.PHONE' | translate }}: {{ booking.workerPhone }}</p>
                        <p *ngIf="!booking.workerPhone" class="text-warning">سيظهر رقم العاملة بعد تأكيد الدفع</p>
                        <p *ngIf="booking.workerWhatsApp">{{ 'COMMON.WHATSAPP_NUMBER' | translate }}: {{ booking.workerWhatsApp }}</p>
                        <p *ngIf="booking.workerProfileImage">
                            <img [src]="booking.workerProfileImage" alt="Worker" class="w-50 h-50 border-round" />
                        </p>
                    </p-card>
                </div>  

                <!-- Booking Status & Salary -->
                <div class="col-span-12 md:col-span-6">
                    <p-card header="{{ 'BOOKING.STATUS' | translate }}">
                        <p-tag [value]="statusLabel(booking.status)" [severity]="statusSeverity(booking.status)"></p-tag>
                        <p class="mt-2">{{ 'BOOKING.START_DATE' | translate }}: {{ booking.startDate | date:'shortDate' }}</p>
                        <p>{{ 'BOOKING.MONTHLY_SALARY' | translate }}: {{ booking.monthlySalary | currency:'EGP':'symbol':'1.0-0' }}</p>
                        <p>{{ 'BOOKING.COMMISSION' | translate }}: {{ booking.commissionAmount | currency:'EGP':'symbol':'1.0-0' }}</p>
                        <p>{{ 'BOOKING.COMMISSION_TYPE' | translate }}: {{ booking.commissionType === 0 ? ('BOOKING.ONETIME' | translate) : ('BOOKING.SUBSCRIPTION' | translate) }}</p>
                    </p-card>
                </div>
                

                     <!-- Replacement -->
                    <div class="col-span-12" *ngIf="canRequestReplacement()">
                        <p-card>
                            <div class="flex align-items-center justify-content-between">
                                <div>
                                    <strong>{{ 'BOOKING.REPLACEMENT' | translate }}</strong>
                                    <p class="text-sm text-muted-color">تم استخدام {{ booking.replacementCount }} من 2 استبدال</p>
                                </div>
                                <p-button label="طلب استبدال" icon="pi pi-refresh" severity="warn" (onClick)="requestReplacement()"></p-button>
                            </div>
                        </p-card>
                    </div>

                <!-- Payment Proof Upload (only when WaitingPayment) -->
                <div class="col-span-12" *ngIf="booking.status === 2">
                    <p-card header="{{ 'PAYMENT.UPLOAD_PROOF' | translate }}">
                        <form [formGroup]="paymentForm" class="grid grid-cols-12 gap-4">
                            <div class="col-span-12 md:col-span-4">
                                <label class="block font-bold mb-1">{{ 'PAYMENT.METHOD' | translate }}</label>
                                <p-select
                                    formControlName="paymentMethod"
                                    [options]="paymentMethods"
                                    optionValue="value"
                                    optionLabel="label"
                                    [placeholder]="'PAYMENT.METHOD' | translate"
                                    class="w-full">
                                </p-select>
                            </div>
                            <div class="col-span-12 md:col-span-4">
                                <label class="block font-bold mb-1">{{ 'PAYMENT.AMOUNT' | translate }}</label>
                                <input pInputText formControlName="commissionAmount" type="number" class="w-full" />
                            </div>
                            <div class="col-span-12 md:col-span-4">
                                <label class="block font-bold mb-1">{{ 'PAYMENT.TRANSACTION_REF' | translate }}</label>
                                <input pInputText formControlName="transactionReference" class="w-full" />
                            </div>
                            <div class="col-span-12">
                                <label class="block font-bold mb-1">{{ 'PAYMENT.UPLOAD_PROOF' | translate }}</label>
                                <p-fileupload
                                    name="proofImage"
                                    mode="basic"
                                    accept="image/*"
                                    maxFileSize="5000000"
                                    [auto]="false"
                                    chooseLabel="اختر صورة الإيصال"
                                    (onSelect)="onProofSelected($event)">
                                </p-fileupload>
                                <span *ngIf="proofFileName" class="text-sm text-muted-color">{{ proofFileName }}</span>
                            </div>
                            <div class="col-span-12 text-center">
                                <p-button
                                    [label]="'PAYMENT.UPLOAD_PROOF' | translate"
                                    icon="pi pi-upload"
                                    (onClick)="submitPaymentProof()"
                                    [loading]="isSubmitting"
                                    [disabled]="paymentForm.invalid || !proofFile">
                                </p-button>
                            </div>
                        </form>
                    </p-card>
                </div>

                                <!-- رسالة تأكيد الاستلام - المهمة دي -->
                <div class="col-span-12" *ngIf="booking.status === 8">
                    <p-card>
                        <div class="text-center py-4">
                            <i class="pi pi-check-circle text-4xl text-green-500 mb-3"></i>
                            <p class="text-lg font-bold">تم استلام إثبات الدفع</p>
                            <p class="text-muted-color">بانتظار مراجعة الإدارة وتأكيد الدفع</p>
                        </div>
                    </p-card>
                </div>
            </div>
        </div>
    `
})
export class BookingDetail implements OnInit {
    private route = inject(ActivatedRoute);
    private fb = inject(FormBuilder);
    private bookingService = inject(BookingService);
    private messageService = inject(MessageService);
    private router = inject(Router);

    booking: BookingDetailDto | null = null;
    isSubmitting = false;
    proofFile: File | null = null;
    proofFileName = '';

    paymentMethods = [
        { label: 'فودافون كاش', value: 0 },
        { label: 'انستاباي', value: 1 }
    ];

    paymentForm: FormGroup = this.fb.group({
        paymentMethod: [null, Validators.required],
        amount: [0, [Validators.required, Validators.min(1)]],
        commissionAmount: [0, [Validators.required, Validators.min(1)]],
        transactionReference: ['']
    });

    ngOnInit() {
        const id = Number(this.route.snapshot.paramMap.get('id'));
        this.loadBooking(id);
    }

    private loadBooking(id: number) {
        this.bookingService.getBookingById(id).subscribe({
            next: (data) => {
                this.booking = data;
                this.paymentForm.patchValue({ amount: data.monthlySalary , commissionAmount:data.commissionAmount });
            }
        });
    }

    onProofSelected(event: any) {
        const file = event.currentFiles?.[0];
        if (file) {
            this.proofFile = file;
            this.proofFileName = file.name;
        }
    }

    submitPaymentProof() {
        if (this.paymentForm.invalid || !this.proofFile || !this.booking) return;

        this.isSubmitting = true;
        const fd = new FormData();
        fd.append('PaymentMethod', this.paymentForm.get('paymentMethod')?.value);
        fd.append('Amount', this.paymentForm.get('amount')?.value);
        fd.append('CommissionAmount', this.paymentForm.get('commissionAmount')?.value);
        fd.append('TransactionReference', this.paymentForm.get('transactionReference')?.value || '');
        fd.append('proofImage', this.proofFile);

        this.bookingService.uploadPaymentProof(this.booking.id, fd).subscribe({
            next: () => {
                this.messageService.add({ severity: 'success', detail: 'تم رفع إثبات الدفع بنجاح' });
                this.isSubmitting = false;
                this.loadBooking(this.booking!.id);
            },
            error: () => {
                this.messageService.add({ severity: 'error', detail: 'فشل رفع إثبات الدفع' });
                this.isSubmitting = false;
            }
        });
    }

    canRequestReplacement(): boolean {
    return this.booking !== null
                && (this.booking.status === 3 || this.booking.status === 4)
                && this.booking.replacementCount < 2;
        }

        requestReplacement() {
            this.router.navigate(['/homeowner/workers'], {
                queryParams: { mode: 'replacement', bookingId: this.booking!.id }
            });
        }

    statusLabel(s: number): string {
        return ['في الانتظار','تم تأكيد العاملة','بانتظار الدفع','مدفوع','نشط','مكتمل','ملغي','طلب استبدال','قيد المراجعة'][s]||'—';
    }
    statusSeverity(s: number): any {
        return ['warn','info','warn','success','info','success','danger','warn','info'][s]||'secondary';
    }
}