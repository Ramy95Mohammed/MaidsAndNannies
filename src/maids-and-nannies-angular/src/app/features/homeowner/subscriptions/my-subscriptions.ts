import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { CardModule } from 'primeng/card';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { ButtonModule } from 'primeng/button';
import { SelectModule } from 'primeng/select';
import { InputTextModule } from 'primeng/inputtext';
import { FileUpload } from 'primeng/fileupload';
import { DialogModule } from 'primeng/dialog';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';
import { TranslatePipe } from '@ngx-translate/core';
import { SubscriptionService, SubscriptionDto } from '../../../core/services/subscription.service';

@Component({
    selector: 'app-my-subscriptions',
    standalone: true,
    imports: [
        CommonModule, ReactiveFormsModule, CardModule, TableModule, TagModule,
        ButtonModule, SelectModule, InputTextModule, FileUpload, DialogModule,
        ToastModule, TranslatePipe
    ],
    providers: [MessageService],
    template: `
        <p-toast />
        <div class="card">
            <h2>{{ 'SUBSCRIPTION.MY_SUBSCRIPTIONS' | translate }}</h2>

            <p-table [value]="subscriptions()" [rows]="10" [tableStyle]="{ 'min-width': '50rem' }">
                <ng-template #header>
                    <tr>
                        <th>{{ 'SUBSCRIPTION.PLAN' | translate }}</th>
                        <th>{{ 'SUBSCRIPTION.AMOUNT' | translate }}</th>
                        <th>{{ 'SUBSCRIPTION.START_DATE' | translate }}</th>
                        <th>{{ 'SUBSCRIPTION.END_DATE' | translate }}</th>
                        <th>{{ 'SUBSCRIPTION.STATUS' | translate }}</th>
                        <th>{{ 'COMMON.ACTIONS' | translate }}</th>
                    </tr>
                </ng-template>
                <ng-template #body let-s>
                    <tr>
                        <td>{{ s.planType === 0 ? ('BOOKING.ONETIME' | translate) : ('BOOKING.SUBSCRIPTION' | translate) }}</td>
                        <td>{{ s.amount | currency:'EGP':'symbol':'1.0-0' }}</td>
                        <td>{{ s.startDate | date:'shortDate' }}</td>
                        <td>{{ s.endDate | date:'shortDate' }}</td>
                        <td>
                            <p-tag *ngIf="s.isActive && s.daysRemaining > 0" [value]="'نشط - باقي ' + s.daysRemaining + ' يوم'" severity="success"></p-tag>
                            <p-tag *ngIf="s.isActive && s.daysRemaining <= 7 && s.daysRemaining > 0" [value]="'ينتهي قريباً - باقي ' + s.daysRemaining + ' يوم'" severity="warn"></p-tag>
                            <p-tag *ngIf="!s.isActive" value="بانتظار التأكيد" severity="warn"></p-tag>
                            <p-tag *ngIf="s.daysRemaining <= 0" value="منتهي" severity="danger"></p-tag>
                        </td>
                        <td>
                            <p-button *ngIf="s.isActive && s.daysRemaining <= 7" label="{{ 'SUBSCRIPTION.RENEW' | translate }}" size="small" severity="warn" (onClick)="openRenewDialog(s)"></p-button>
                            <p-button *ngIf="s.daysRemaining <= 0" label="{{ 'SUBSCRIPTION.RENEW' | translate }}" size="small" severity="danger" (onClick)="openRenewDialog(s)"></p-button>
                        </td>
                    </tr>
                </ng-template>
            </p-table>

            <div *ngIf="subscriptions().length === 0" class="text-center py-8 text-muted-color">
                {{ 'SUBSCRIPTION.NO_SUBSCRIPTIONS' | translate }}
            </div>
        </div>

        <!-- Renew Dialog -->
        <p-dialog header="{{ 'SUBSCRIPTION.RENEW' | translate }}" [modal]="true" [(visible)]="showRenewDialog" [style]="{ width: '450px' }">
            <form [formGroup]="renewForm" class="flex flex-col gap-4">
                <div>
                    <label class="block font-bold mb-1">{{ 'PAYMENT.METHOD' | translate }}</label>
                    <p-select formControlName="paymentMethod" [options]="paymentMethods" optionValue="value" optionLabel="label" [placeholder]="'PAYMENT.METHOD' | translate" class="w-full"></p-select>
                </div>
                <div>
                    <label class="block font-bold mb-1">{{ 'PAYMENT.AMOUNT' | translate }}</label>
                    <input pInputText formControlName="amount" type="number" class="w-full" />
                </div>
                <div>
                    <label class="block font-bold mb-1">{{ 'PAYMENT.TRANSACTION_REF' | translate }}</label>
                    <input pInputText formControlName="transactionReference" class="w-full" />
                </div>
                <div>
                    <label class="block font-bold mb-1">{{ 'PAYMENT.UPLOAD_PROOF' | translate }}</label>
                    <p-fileupload name="proofImage" mode="basic" accept="image/*" maxFileSize="5000000" [auto]="false" chooseLabel="اختر صورة" (onSelect)="onProofSelected($event)"></p-fileupload>
                </div>
                <p-button label="{{ 'SUBSCRIPTION.RENEW' | translate }}" icon="pi pi-check" (onClick)="submitRenew()" [disabled]="renewForm.invalid || !proofFile"></p-button>
            </form>
        </p-dialog>
    `
})
export class MySubscriptions implements OnInit {
    private subscriptionService = inject(SubscriptionService);
    private fb = inject(FormBuilder);
    private messageService = inject(MessageService);

    subscriptions = signal<SubscriptionDto[]>([]);
    showRenewDialog = false;
    selectedSub: SubscriptionDto | null = null;
    proofFile: File | null = null;

    paymentMethods = [
        { label: 'فودافون كاش', value: 0 },
        { label: 'انستاباي', value: 1 }
    ];

    renewForm: FormGroup = this.fb.group({
        paymentMethod: [null, Validators.required],
        amount: [0, [Validators.required, Validators.min(1)]],
        transactionReference: ['']
    });

    ngOnInit() { this.load(); }

    load() {
        this.subscriptionService.getMySubscriptions().subscribe({
            next: (data) => this.subscriptions.set(data)
        });
    }

    openRenewDialog(sub: SubscriptionDto) {
        this.selectedSub = sub;
        this.proofFile = null;
        this.renewForm.reset({ amount: sub.amount });
        this.showRenewDialog = true;
    }

    onProofSelected(event: any) {
        this.proofFile = event.currentFiles?.[0] || null;
    }

    submitRenew() {
        if (this.renewForm.invalid || !this.proofFile || !this.selectedSub) return;

        const fd = new FormData();
        fd.append('PaymentMethod', this.renewForm.get('paymentMethod')?.value);
        fd.append('Amount', this.renewForm.get('amount')?.value);
        fd.append('TransactionReference', this.renewForm.get('transactionReference')?.value || '');
        fd.append('proofImage', this.proofFile);

        this.subscriptionService.renewSubscription(this.selectedSub.id, fd).subscribe({
            next: () => {
                this.messageService.add({ severity: 'success', detail: 'تم إرسال طلب التجديد' });
                this.showRenewDialog = false;
                this.load();
            }
        });
    }
}