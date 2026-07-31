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
import { TranslatePipe } from '@ngx-translate/core';
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

    groups: SettingGroup[] = [
        {
            title: 'الدفع',
            settings: [
                { key: 'CommissionBillingMode', label: 'المبلغ المطلوب عند الدفع', value: 'CommissionOnly', type: 'select',
                  options: [
                      { label: 'العمولة فقط (لصاحب التطبيق)', value: 'CommissionOnly' },
                      { label: 'العمولة + مرتب العاملة', value: 'CommissionPlusSalary' }
                  ],
                  description: 'هل يدفع صاحبة المنزل العمولة فقط أم العمولة مع مرتب العاملة الأول' },
                { key: 'RequirePaymentProof', label: 'طلب إثبات الدفع', value: 'true', type: 'select',
                  options: [
                      { label: 'نعم - ترفع صاحبة المنزل إثبات الدفع', value: 'true' },
                      { label: 'لا - يُعتبر الحجز مدفوعاً فور طلب الدفع', value: 'false' }
                  ],
                  description: 'للحجوزات والاشتراكات — إذا لا، يتم التأكيد عبر واتساب' },
            ]
        },
        {
            title: 'نسب العمولة',
            settings: [
                { key: 'CommissionDailyPercent', label: 'عمولة يومي (%)', value: 10, type: 'number', description: 'نسبة العمولة للحجوزات اليومية' },
                { key: 'CommissionHourlyPercent', label: 'عمولة ساعي (%)', value: 10, type: 'number', description: 'نسبة العمولة للحجوزات بالساعة' },
                { key: 'CommissionMonthlyOneTimePercent', label: 'عمولة شهري - مرة واحدة (%)', value: 10, type: 'number', description: 'نسبة العمولة للحجز الشهري (مرة واحدة)' },
                { key: 'CommissionMonthlySubscriptionPercent', label: 'عمولة شهري - اشتراك (%)', value: 10, type: 'number', description: 'نسبة العمولة للحجز الشهري (اشتراك شهري)' },
            ]
        },
        {
            title: 'الحدود',
            settings: [
                { key: 'MaxFaultReplacementCount', label: 'الحد الأقصى للاستبدال (تقصير العاملة)', value: 3, type: 'number', description: 'عدد مرات الاستبدال المجاني المسموح بها بسبب تقصير العاملة' },
                { key: 'MaxPreferenceReplacementCount', label: 'الحد الأقصى للاستبدال (رغبة شخصية)', value: 1, type: 'number', description: 'عدد مرات الاستبدال المسموح بها برغبة شخصية من صاحبة المنزل' },
                { key: 'MaxActiveBookingsPerHomeowner', label: 'أقصى حجوزات نشطة', value: 5, type: 'number', description: 'الحد الأقصى للحجوزات النشطة لكل صاحبة منزل' },
                { key: 'AutoCancelPendingBookingHours', label: 'إلغاء الحجز المعلق بعد (ساعة)', value: 48, type: 'number', description: 'إلغاء الحجوزات المعلقة تلقائياً بعد هذه المدة' },
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
            next: () => this.messageService.add({ severity: 'success', detail: 'تم حفظ الإعدادات' })
        });
    }
}