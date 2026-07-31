import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { CardModule } from 'primeng/card';
import { TagModule } from 'primeng/tag';
import { ButtonModule } from 'primeng/button';
import { SelectModule } from 'primeng/select';
import { InputTextModule } from 'primeng/inputtext';
import { FileUpload } from 'primeng/fileupload';
import { ToastModule } from 'primeng/toast';
import { DialogModule } from 'primeng/dialog';
import { RadioButtonModule } from 'primeng/radiobutton';
import { MessageModule } from 'primeng/message';
import { MessageService } from 'primeng/api';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { BookingService, BookingDetailDto } from '../../../core/services/booking.service';
import { ApiService } from '@/core/services/api.service';
import { TableModule } from "primeng/table";
import { Rating } from "primeng/rating";

@Component({
    selector: 'app-booking-detail',
    standalone: true,
    imports: [
    CommonModule, ReactiveFormsModule, CardModule, TagModule,
    ButtonModule, SelectModule, InputTextModule, FileUpload,
    ToastModule, RouterModule, TranslatePipe,
    TableModule, DialogModule, RadioButtonModule, MessageModule,
    Rating , FormsModule
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
                        <p *ngIf="!booking.workerPhone" class="text-warning">{{ 'BOOKING_DETAIL.WORKER_PHONE_WILL_APPEAR' | translate }}</p>
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
                        <p>{{ 'BOOKING.TYPE' | translate }}: {{ getBookingTypeLabel(booking.bookingType) }}</p>
                        <p>{{ 'BOOKING.SALARY' | translate }}: {{ (booking.bookingType == 0)? booking.dailySalary:(booking.bookingType == 1)?booking.monthlySalary:booking.hourlySalary | currency:booking.currencyCode:'':'1.0-0' }} {{ booking.currencyCode }}</p>
                        <p>{{ 'BOOKING.QUANTITY' | translate }}: {{ booking.quantity }}</p>
                        <p>{{ 'BOOKING.TOTAL_AMOUNT' | translate }}:{{ booking.totalAmount | currency: booking.currencyCode:'':'1.0-0' }} {{ booking.currencyCode }}</p>
                        <p>{{ 'BOOKING.TOTAL_AMOUNT_AFTER_CONVERSION' | translate }}:{{ booking.totalAmountAfterConversion | currency:'EGP':'code':'1.0-0' }}</p>
                        <p>{{ 'BOOKING.COMMISSION' | translate }}: {{ booking.commissionAmount | currency:'EGP':'code':'1.0-0' }}</p>
                       @if(booking.bookingType == 1){ <p>{{ 'BOOKING.COMMISSION_TYPE' | translate }}: {{ booking.commissionType === 0 ? ('BOOKING.ONETIME' | translate) : ('BOOKING.SUBSCRIPTION' | translate) }}</p>}
                    </p-card>
                </div>

                <!-- Outstanding amount after replacement -->
                <div class="col-span-12" *ngIf=" booking.status === 2 && booking.outstandingAmount > 0 ">
                    <p-message severity="warn" [text]="'BOOKING_DETAIL.OUTSTANDING_AMOUNT_TITLE' | translate">
                    </p-message>
                    <p class="text-sm text-muted-color mt-2">
                        {{ 'BOOKING_DETAIL.OUTSTANDING_AMOUNT_DESC' | translate }}:
                        <strong>{{ booking.outstandingAmount | currency:'EGP':'code':'1.0-0' }}</strong>
                    </p>
                </div>
                

                     <!-- Replacement -->
                    <div class="col-span-12" *ngIf="canRequestReplacement()">
                        <p-card>
                            <div class="flex align-items-center justify-content-between">
                                <div>
                                    <strong>{{ 'BOOKING.REPLACEMENT' | translate }}</strong>
                                    <p class="text-sm text-muted-color">{{ 'BOOKING_DETAIL.REPLACEMENT_USED_DETAILED' | translate:{fault: remainingFaultReplacements(), preference: remainingPreferenceReplacements()} }}</p>
                                </div>
                                <p-button [label]="'BOOKING_DETAIL.REQUEST_REPLACEMENT' | translate" icon="pi pi-refresh" severity="warn" (onClick)="openReasonDialog('navigate')"></p-button>
                            </div>
                        </p-card>
                    </div>

                    <!-- Replacement reason dialog -->
                    <p-dialog [header]="'BOOKING_DETAIL.REPLACEMENT_REASON_TITLE' | translate"
                              [(visible)]="showReasonDialog" [modal]="true" [style]="{width: '28rem'}">
                        <div class="flex flex-column gap-3">
                            <div class="flex align-items-start gap-2">
                                <p-radioButton name="reason" [value]="0" [(ngModel)]="selectedReason" inputId="reasonFault"></p-radioButton>
                                <label for="reasonFault" class="cursor-pointer">
                                    <div class="font-bold">{{ 'BOOKING_DETAIL.REPLACEMENT_REASON_FAULT' | translate }}</div>
                                    <div class="text-sm text-muted-color">{{ 'BOOKING_DETAIL.REPLACEMENT_REASON_FAULT_DESC' | translate }}</div>
                                </label>
                            </div>
                            <div class="flex align-items-start gap-2">
                                <p-radioButton name="reason" [value]="1" [(ngModel)]="selectedReason" inputId="reasonPreference"></p-radioButton>
                                <label for="reasonPreference" class="cursor-pointer">
                                    <div class="font-bold">{{ 'BOOKING_DETAIL.REPLACEMENT_REASON_PREFERENCE' | translate }}</div>
                                    <div class="text-sm text-muted-color">{{ 'BOOKING_DETAIL.REPLACEMENT_REASON_PREFERENCE_DESC' | translate }}</div>
                                </label>
                            </div>
                        </div>
                        <ng-template #footer>
                            <p-button [label]="'BOOKING_DETAIL.CONFIRM' | translate" icon="pi pi-check" (onClick)="confirmReason()"></p-button>
                        </ng-template>
                    </p-dialog>

                    <!-- Job Applicants for Replacement -->
                    <div class="col-span-12" *ngIf="booking.jobPostId && canRequestReplacement()">
                    <p-card header="{{ 'JOB_POST.APPLICATIONS' | translate }}">
                        <p-table [value]="applicants()" *ngIf="applicants().length > 0">
                        <ng-template #header>
                            <tr>
                            <th>{{ 'ADMIN.NAME' | translate }}</th>
                            <th>{{ 'REVIEW.RATING' | translate }}</th>
                            <th>{{ 'ADMIN.TABLE_ACTIONS' | translate }}</th>
                            </tr>
                        </ng-template>
                        <ng-template #body let-a>
                            <tr>
                            <td>{{ a.workerName }}</td>
                            <td><p-rating [(ngModel)]="a.workerRating" [readonly]="true"></p-rating></td>
                            <td>
                                <p-button label="{{ 'BOOKING_DETAIL.SET_AS_REPLACEMENT' | translate }}"
                                size="small" severity="warn" (onClick)="openReasonDialog('applicant', a.id)"></p-button>
                            </td>
                            </tr>
                        </ng-template>
                        </p-table>
                        <p *ngIf="applicants().length === 0" class="text-muted-color">
                        {{ 'JOB_POST.NO_APPLICANTS' | translate }}
                        </p>
                    </p-card>
                    </div>

                <!-- Payment Proof Upload (WaitingPayment, or ReplacementRequested with a pending difference) -->
                <!-- <div class="col-span-12" *ngIf="booking.status === 2 && booking.requirePaymentProof">
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
                              <div class="col-span-12 text-sm text-muted-color">
                                {{ 'BOOKING_DETAIL.PAYMENT_TOTAL' | translate }}: <strong>{{ booking.paymentAmount | currency:'EGP':'code':'1.0-0' }}</strong>
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
                                    [chooseLabel]="'BOOKING_DETAIL.CHOOSE_RECEIPT' | translate"
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

                                            
                                    <div class="col-span-12" *ngIf="booking.status === 3 && !booking.requirePaymentProof && booking.isPaid">
                                        <p-message severity="success" [text]="'BOOKING_DETAIL.PAID_DIRECTLY' | translate"></p-message>
                                    </div>
                        </form>
                    </p-card>
                </div> -->

                                <!-- المبلغ الإجمالي المطلوب عند الدفع -->
                               <!-- المبلغ الإجمالي المطلوب عند الدفع -->
                <div class="col-span-12" *ngIf="booking.status === 2">
                    <p-card header="{{ 'PAYMENT.AMOUNT' | translate }}">
                        <div class="flex flex-column gap-2">
                            <p>{{ 'BOOKING.COMMISSION' | translate }}: {{ booking.commissionAmount | currency:'EGP':'code':'1.0-0' }}</p>
                            <p>{{ 'BOOKING_DETAIL.PAYMENT_TOTAL' | translate }}: <strong>{{ booking.paymentAmount | currency:'EGP':'code':'1.0-0' }}</strong></p>
                            <p-message *ngIf="!booking.requirePaymentProof" severity="warn" [text]="'BOOKING_DETAIL.SEND_PROOF_WHATSAPP' | translate"></p-message>
                        </div>
                    </p-card>
                </div>

                <!-- Payment Proof Upload -->
                <div class="col-span-12" *ngIf="booking.status === 2 && booking.requirePaymentProof">
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
                                    [chooseLabel]="'BOOKING_DETAIL.CHOOSE_RECEIPT' | translate"
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
                            <p class="text-lg font-bold">{{ 'BOOKING_DETAIL.PROOF_RECEIVED' | translate }}</p>
                            <p class="text-muted-color">{{ 'BOOKING_DETAIL.PENDING_REVIEW' | translate }}</p>
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
    private translate = inject(TranslateService);

    booking: BookingDetailDto | null = null;
    isSubmitting = false;
    proofFile: File | null = null;
    proofFileName = '';

    showReasonDialog = false;
    selectedReason: 0 | 1 = 1;
    private pendingAction: 'navigate' | 'applicant' | null = null;
    private pendingApplicationId: number | null = null;

    private api = inject(ApiService);
        applicants = signal<any[]>([]);
        showApplicants = signal(false);

    paymentMethods = [
        { label: 'فودافون كاش', value: 0 },
        { label: 'انستاباي', value: 1 }
    ];


    paymentForm: FormGroup = this.fb.group({
        paymentMethod: [null, Validators.required],
        // amount: [0, [Validators.required, Validators.min(1)]],
        commissionAmount: [0, [Validators.required, Validators.min(1)]],
        transactionReference: ['']
    });

    ngOnInit() {
        const id = Number(this.route.snapshot.paramMap.get('id'));
        this.loadBooking(id);

        setTimeout(() => {
            this.paymentMethods = [
                { label: this.translate.instant('PAYMENT.VODAFONE_CASH'), value: 0 },
                { label: this.translate.instant('PAYMENT.INSTAPAY'), value: 1 }
            ];
        }, 1000);        
    }

    private loadBooking(id: number) {
        this.bookingService.getBookingById(id).subscribe({
            next: (data:BookingDetailDto) => {
                this.booking = data;

                  const prefillAmount = (data.outstandingAmount > 0)
                            ? data.outstandingAmount
                            : (data.paymentAmount ?? data.commissionAmount);

                this.paymentForm.patchValue({ amount: data.monthlySalary, commissionAmount: prefillAmount });
                if (data.jobPostId) {
                this.loadApplicants(data.jobPostId);
                }
            }
        });
    }

    
loadApplicants(postId: number) {
  this.api.getJobApplications(postId).subscribe({
    next: (d) => this.applicants.set(d.filter((a: any) => a.status === 0))
  });
}

/** تفتح Dialog اختيار سبب الاستبدال وتحفظ الإجراء المطلوب تنفيذه بعد التأكيد */
openReasonDialog(action: 'navigate' | 'applicant', applicationId?: number) {
    this.pendingAction = action;
    this.pendingApplicationId = applicationId ?? null;
    this.selectedReason = 1; // الافتراضي: رغبة شخصية
    this.showReasonDialog = true;
}

/** بعد ما صاحبة المنزل تختار السبب وتأكد، ننفذ الإجراء المحفوظ */
confirmReason() {
    this.showReasonDialog = false;
    if (!this.booking) return;

    if (this.pendingAction === 'navigate') {
        this.router.navigate(['/homeowner/workers'], {
            queryParams: { mode: 'replacement', bookingId: this.booking.id, reason: this.selectedReason }
        });
    } else if (this.pendingAction === 'applicant' && this.pendingApplicationId != null) {
        this.replaceWithApplicant(this.pendingApplicationId, this.selectedReason);
    }

    this.pendingAction = null;
    this.pendingApplicationId = null;
}

replaceWithApplicant(applicationId: number, reason: 0 | 1 = 1) {
    if(this.booking != null)
  this.bookingService.requestReplacement(this.booking.id, reason, null, applicationId).subscribe({
    next: () => {
      this.messageService.add({ severity: 'success', detail: 'تم طلب الاستبدال بنجاح' });      
      if(this.booking != null)
      this.loadBooking(this.booking.id);
    }
  });
}

/** عدد الاستبدالات المتبقية بسبب مشكلة في العاملة */
remainingFaultReplacements(): number {
    if (!this.booking) return 0;
    return Math.max(0, this.booking.maxFaultReplacement - this.booking.replacementCount);
}

/** عدد الاستبدالات المتبقية برغبة شخصية */
remainingPreferenceReplacements(): number {
    if (!this.booking) return 0;
    return Math.max(0, this.booking.maxPreferenceReplacement - this.booking.replacementCount);
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
        // fd.append('Amount', this.paymentForm.get('amount')?.value);
        fd.append('CommissionAmount', this.paymentForm.get('commissionAmount')?.value);
        fd.append('TransactionReference', this.paymentForm.get('transactionReference')?.value || '');
        fd.append('proofImage', this.proofFile);

        this.bookingService.uploadPaymentProof(this.booking.id, fd).subscribe({
            next: () => {
                this.messageService.add({ severity: 'success', detail: this.translate.instant('BOOKING_DETAIL.TOAST_UPLOAD_SUCCESS') });
                this.isSubmitting = false;
                this.loadBooking(this.booking!.id);
            },
            error: () => {
                this.messageService.add({ severity: 'error', detail: this.translate.instant('BOOKING_DETAIL.TOAST_UPLOAD_ERROR') });
                this.isSubmitting = false;
            }
        });
    }

    canRequestReplacement(): boolean {
    return this.booking !== null
                && (this.booking.status === 3 || this.booking.status === 4)
                && this.booking.replacementCount < this.booking.maxReplacement;
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
    statusSeverity(s: number): any {
        return ['warn','info','warn','success','info','success','danger','warn','info'][s]||'secondary';
    }
}