import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
import { SelectModule } from 'primeng/select';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { ApiService } from '../../../core/services/api.service';

interface SettingItem {
    key: string;
    label: string;
    value: any;
    type: 'number' | 'select';
    options?: { label: string; value: string }[];
    description: string;
}

interface SettingGroup {
    title: string;
    settings: SettingItem[];
}

@Component({
    selector: 'app-admin-settings',
    standalone: true,
    imports: [CommonModule, FormsModule, CardModule, ButtonModule, InputTextModule, InputNumberModule, SelectModule, ToastModule, TranslatePipe],
    providers: [MessageService],
    template: `
        <p-toast />
        <div class="card">
            <div class="flex justify-content-between mb-4">
                <h2>{{ 'SETTINGS.TITLE' | translate }}</h2>
                <p-button [label]="'COMMON.SAVE' | translate" icon="pi pi-save" (onClick)="save()"></p-button>
            </div>

            <div *ngFor="let group of groups" class="mb-5">
                <h3 class="text-lg font-bold mb-3">{{ group.title }}</h3>
                <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
                    <div *ngFor="let s of group.settings" class="card p-3">
                        <label class="block font-bold mb-1">{{ s.label }}</label>
                        <p-inputnumber *ngIf="s.type === 'number'" [(ngModel)]="s.value" [min]="0" class="w-full"></p-inputnumber>
                        <p-select *ngIf="s.type === 'select'" [(ngModel)]="s.value" [options]="s.options" optionValue="value" optionLabel="label" class="w-full"></p-select>
                        <small class="text-muted-color">{{ s.description }}</small>
                    </div>
                </div>
            </div>
        </div>
    `
})
export class AdminSettings implements OnInit {
    private apiService = inject(ApiService);
    private messageService = inject(MessageService);
    private translate = inject(TranslateService);

    groups: SettingGroup[] = [
        {
            title: this.translate.instant('SETTINGS.PAYMENT'),
            settings: [
                { key: 'CommissionBillingMode', label: this.translate.instant('SETTINGS.PAYMENT_AMOUNT_LABEL'), value: 'CommissionOnly', type: 'select',
                  options: [
                      { label: this.translate.instant('SETTINGS.COMMISSION_ONLY'), value: 'CommissionOnly' },
                      { label: this.translate.instant('SETTINGS.COMMISSION_PLUS_SALARY'), value: 'CommissionPlusSalary' }
                  ],
                  description: this.translate.instant('SETTINGS.PAYMENT_MODE_DESC') },
                { key: 'RequirePaymentProof', label: this.translate.instant('SETTINGS.REQUIRE_PAYMENT_PROOF'), value: 'true', type: 'select',
                  options: [
                      { label: this.translate.instant('SETTINGS.PROOF_YES'), value: 'true' },
                      { label: this.translate.instant('SETTINGS.PROOF_NO'), value: 'false' }
                  ],
                  description: this.translate.instant('SETTINGS.PROOF_DESC') },
            ]
        },
        {
            title: this.translate.instant('SETTINGS.COMMISSION_RATES'),
            settings: [
                { key: 'CommissionDailyPercent', label: this.translate.instant('SETTINGS.DAILY_LABEL'), value: 10, type: 'number', description: this.translate.instant('SETTINGS.DAILY_DESC') },
                { key: 'CommissionHourlyPercent', label: this.translate.instant('SETTINGS.HOURLY_LABEL'), value: 10, type: 'number', description: this.translate.instant('SETTINGS.HOURLY_DESC') },
                { key: 'CommissionMonthlyOneTimePercent', label: this.translate.instant('SETTINGS.MONTHLY_ONETIME_LABEL'), value: 10, type: 'number', description: this.translate.instant('SETTINGS.MONTHLY_ONETIME_DESC') },
                { key: 'CommissionMonthlySubscriptionPercent', label: this.translate.instant('SETTINGS.MONTHLY_SUBSCRIPTION_LABEL'), value: 10, type: 'number', description: this.translate.instant('SETTINGS.MONTHLY_SUBSCRIPTION_DESC') },
            ]
        },
        {
            title: this.translate.instant('SETTINGS.LIMITS'),
            settings: [
                { key: 'MaxFaultReplacementCount', label: this.translate.instant('SETTINGS.MAX_FAULT_LABEL'), value: 3, type: 'number', description: this.translate.instant('SETTINGS.MAX_FAULT_DESC') },
                { key: 'MaxPreferenceReplacementCount', label: this.translate.instant('SETTINGS.MAX_PREFERENCE_LABEL'), value: 1, type: 'number', description: this.translate.instant('SETTINGS.MAX_PREFERENCE_DESC') },
                { key: 'MaxActiveBookingsPerHomeowner', label: this.translate.instant('SETTINGS.MAX_ACTIVE_LABEL'), value: 5, type: 'number', description: this.translate.instant('SETTINGS.MAX_ACTIVE_DESC') },
                { key: 'AutoCancelPendingBookingHours', label: this.translate.instant('SETTINGS.PENDING_CANCEL_LABEL'), value: 48, type: 'number', description: this.translate.instant('SETTINGS.PENDING_CANCEL_DESC') },
            ]
        }
    ];

    ngOnInit() {
        this.apiService.getSettings().subscribe({
            next: (data: any[]) => {
                for (const g of this.groups) {
                    for (const s of g.settings) {
                        const found = data.find((d: any) => d.key === s.key);
                        if (found) s.value = s.type === 'number' ? (parseInt(found.value) || 0) : found.value;
                    }
                }
            }
        });
    }

    save() {
        const items: any[] = [];
        for (const g of this.groups) {
            for (const s of g.settings) {
                items.push({ key: s.key, value: String(s.value) });
            }
        }
        this.apiService.updateSettings(items).subscribe({
            next: () => this.messageService.add({ severity: 'success', detail: this.translate.instant('SETTINGS.SAVED') })
        });
    }
}