import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { SelectModule } from 'primeng/select';
import { DatePickerModule } from 'primeng/datepicker';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { ApiService } from '../../../core/services/api.service';
import { Textarea } from 'primeng/textarea';
import { CurrencyDto, CurrencyService } from '@/core/services/currency.service';
import { LanguageService } from '@/core/services/language.service';
import { MultiSelect } from 'primeng/multiselect';
import { BookingDetailDto } from '@/core/services/booking.service';
import { Checkbox } from 'primeng/checkbox';

@Component({
    selector: 'app-job-create',
    standalone: true,
    imports: [CommonModule, FormsModule, RouterModule, CardModule, ButtonModule, InputTextModule, Textarea, SelectModule, DatePickerModule, ToastModule, TranslatePipe, MultiSelect, Checkbox],
    providers: [MessageService],
    template: `
        <p-toast />
        <div class="card">
            <h2>{{ (editId ? 'JOB_POST.EDIT_TITLE' : 'JOB_POST.CREATE') | translate }}</h2>
            <div class="grid grid-cols-12 gap-4">
                <div class="col-span-12">
                    <label class="block font-bold mb-2">{{ 'JOB_POST.DESCRIPTION' | translate }}</label>
                    <textarea pInputTextarea [(ngModel)]="description" rows="5" class="w-full" [placeholder]="'JOB_POST.DESC_PLACEHOLDER' | translate"></textarea>
                </div>
                <div class="col-span-12 md:col-span-4">
                    <label class="block font-bold mb-2">{{ 'WORKER.MONTHLY_RATE' | translate }}</label>
                    <input pInputText [(ngModel)]="monthlySalary" type="number" class="w-full" />
                </div>
                <div class="col-span-12 md:col-span-4">
                    <label class="block font-bold mb-2">{{ 'WORKER.DAILY_RATE' | translate }}</label>
                    <input pInputText [(ngModel)]="dailySalary" type="number" class="w-full" />
                </div>
                <div class="col-span-12 md:col-span-4">
                    <label class="block font-bold mb-2">{{ 'WORKER.HOURLY_RATE' | translate }}</label>
                    <input pInputText [(ngModel)]="hourlySalary" type="number" class="w-full" />
                </div>
                <div class="col-span-12 md:col-span-4">
                    <label class="block font-bold mb-2">{{ 'BOOKING.TYPE' | translate }}</label>
                    <p-select (onChange)="disableOrEnableComissionTypeAndQuantity($event.value)" [(ngModel)]="bookingType" [options]="bookingTypes" optionLabel="label" optionValue="value" styleClass="w-full"></p-select>
                </div>
                <div class="col-span-12 md:col-span-4">
                    <label class="block font-bold mb-2">{{ 'BOOKING.COMMISSION_TYPE' | translate }}</label>
                    <p-select [(ngModel)]="commissionType" [disabled]="commissionTypeIsDisabled" [options]="commissionOptions" optionLabel="label" optionValue="value" styleClass="w-full"></p-select>
                </div>
                <div class="col-span-12 md:col-span-4">
                    <label class="block font-bold mb-2">{{ 'WORKER.SPECIALIZATION' | translate }}</label>
                    <p-select [(ngModel)]="specialization" [options]="specializations" optionLabel="label" optionValue="value" styleClass="w-full"></p-select>
                </div>

                <div class="col-span-12 md:col-span-8">
                    <label class="block font-bold mb-2">{{ 'WORKER.SPECIALIZATION_ADDITIONAL' | translate }}</label>
                    <p-multiselect [(ngModel)]="additionalSpecializations" [options]="specializations" optionLabel="label" optionValue="value" class="w-full"></p-multiselect>
                </div>

                <div class="col-span-12 md:col-span-4">
                    <label class="block font-bold mb-2">{{ 'BOOKING.START_DATE' | translate }}</label>
                    <p-datepicker [(ngModel)]="startDate" styleClass="w-full"></p-datepicker>
                </div>

                <div class="col-span-12 md:col-span-4">
                    <label class="block font-bold mb-2">{{ 'WORKER.CURRENCY' | translate }}</label>
                    <p-select [(ngModel)]="currencyId" [options]="currencyOptions()" optionLabel="label" optionValue="value" class="w-full"></p-select>
                </div>

                <div class="col-span-12 md:col-span-4">
                    <label class="block font-bold mb-2">{{ bookingType === 1 ? ('BOOKING.QUANTITY_MONTHLY' | translate) : ('BOOKING.QUANTITY' | translate) }}</label>                    
                    <input pInputText [(ngModel)]="quantity" [disabled]="quantityIsDisabled" type="number" min="1" class="w-full" />
                </div>
                <div class="col-span-12 text-center">

                <div class="col-span-12 text-center">
                    <p-button [label]="'WORKER_DETAIL.CALC_COMMISION' | translate" icon="pi pi-calculator" styleClass="w-100 mb-2" (onClick)="getJobPostCalculationInfo()"></p-button>
                    </div>
                   

                    <div class="mt-2">
                        <p>{{ 'BOOKING.TOTAL_AMOUNT' | translate }}:{{ JobPostCalculationInfo()?.totalAmount | currency: JobPostCalculationInfo()?.currencyCode : '' : '1.0-0' }} {{ JobPostCalculationInfo()?.currencyCode }}</p>
                        <p>{{ 'BOOKING.TOTAL_AMOUNT_AFTER_CONVERSION' | translate }}:{{ JobPostCalculationInfo()?.totalAmountAfterConversion | currency: 'EGP' : 'code' : '1.0-0' }}</p>
                        <p>{{ 'BOOKING.COMMISSION' | translate }}: {{ JobPostCalculationInfo()?.commissionAmount | currency: 'EGP' : 'code' : '1.0-0' }}</p>
                        <p>{{ 'BOOKING_DETAIL.PAYMENT_TOTAL' | translate }}: {{ JobPostCalculationInfo()?.paymentAmount | currency: 'EGP' : 'code' : '1.0-0' }}</p>
                    </div>

                     <div class="flex align-items-center gap-2 mb-3">
                        <p-checkbox [(ngModel)]="termsAccepted" binary="true" inputId="terms"></p-checkbox>
                        <label for="terms"
                            >{{ 'CONSENT.AGREE' | translate }}
                            <a (click)="goToPolicies()" class="text-primary cursor-pointer font-medium">{{ 'CONSENT.TERMS_LINK' | translate }}</a>
                        </label>
                    </div>

                    <div class="col-span-12 text-center">
                        <p-button [label]="(editId ? 'JOB_POST.SAVE' : 'JOB_POST.SUBMIT') | translate" icon="pi pi-send" (onClick)="submit()" [loading]="loading" styleClass="w-100 mb-2"></p-button>
                    </div>
                </div>
            </div>
        </div>
    `
})
export class JobCreate implements OnInit {
    private api = inject(ApiService);
    private router = inject(Router);
    private route = inject(ActivatedRoute);
    private msg = inject(MessageService);
    langService = inject(LanguageService);

    currencyId = 1;
    currencyOptions = signal<{ value: number; label: string }[]>([]);
    description = '';
    monthlySalary = 0;
    dailySalary = 0;
    hourlySalary = 0;
    bookingType = 0;
    commissionType = 0;
    specialization = 0;
    additionalSpecializations: number[] = [];
    startDate: Date | null = null;
    quantity = 1;
    loading = false;
    editId: number | null = null;
    private translate = inject(TranslateService);
    private currencyService = inject(CurrencyService);

    commissionTypeIsDisabled: boolean = true;
    quantityIsDisabled: boolean = false;
    bookingTypes: any;
    commissionOptions: any;
    specializations: any;
    monthlyWorkingDaysPerMonthSettingValue:string | null = null;

    termsAccepted = false;

    JobPostCalculationInfo = signal<BookingDetailDto | null>(null);

    ngOnInit() {
        this.editId = this.route.snapshot.params['id'] ? Number(this.route.snapshot.params['id']) : null;
        this.setOptions();
        this.loadCurrencies(() => {
            if (this.editId) this.loadPost();
        });

        this.getMonthlyWorkingDaysPerMonthSetting();
    }

     getMonthlyWorkingDaysPerMonthSetting(){
         this.api.getSettingByKey('MonthlyWorkingDaysPerMonth').subscribe({
                next: (data) => this.monthlyWorkingDaysPerMonthSettingValue = data.value,
                error: () => this.msg.add({ severity: 'error', summary: this.translate.instant('COMMON.ERROR'), detail: this.translate.instant('BOOKING_DETAIL.TOAST_WORKER_NOT_FOUND') })
            });
    }

    loadCurrencies(cb?: () => void) {
        this.currencyService.getCurrencies().subscribe({
            next: (data) => {
                const isAr = this.langService.getCurrentLanguage() === 'ar';
                this.currencyOptions.set(
                    data.map((c) => ({
                        value: c.id,
                        label: isAr ? `${c.nameAr} (${c.code})` : `${c.nameEn} (${c.code})`
                    }))
                );
                if (cb) cb();
            }
        });
    }

    private loadPost() {
        this.api.getJobPostById(this.editId!).subscribe({
            next: (p) => {
                this.description = p.description || '';
                this.monthlySalary = p.monthlySalary;
                this.dailySalary = p.dailySalary;
                this.hourlySalary = p.hourlySalary;
                this.bookingType = p.bookingType;
                this.commissionType = p.commissionType;
                this.specialization = p.specialization;
                this.additionalSpecializations = p.specializations || [];
                this.startDate = p.startDate ? new Date(p.startDate) : null;
                this.quantity = p.quantity;
                const match = this.currencyOptions().find((c) => c.label.includes(p.currencyCode));
                if (match) this.currencyId = match.value;
                this.disableOrEnableComissionTypeAndQuantity(this.bookingType);
            },
            error: () => this.router.navigate(['/homeowner/jobs'])
        });
    }

    setOptions() {
        setTimeout(() => {
            this.bookingTypes = [
                { label: this.translate.instant('WORKER_DETAIL.DAILY'), value: 0 },
                { label: this.translate.instant('WORKER_DETAIL.MONTHLY'), value: 1 },
                { label: this.translate.instant('WORKER_DETAIL.HOURLY'), value: 2 }
            ];
            this.commissionOptions = [
                { label: this.translate.instant('WORKER_DETAIL.COMMISSION_ONETIME'), value: 0 },
                { label: this.translate.instant('WORKER_DETAIL.COMMISSION_SUBSCRIPTION'), value: 1 }
            ];
            this.specializations = [
                { label: this.translate.instant('SPECIALIZATIONS.CLEANING'), value: 0 },
                { label: this.translate.instant('SPECIALIZATIONS.COOKING'), value: 1 },
                { label: this.translate.instant('SPECIALIZATIONS.CHILDCARE'), value: 2 },
                { label: this.translate.instant('SPECIALIZATIONS.ELDERLYCARE'), value: 3 },
                { label: this.translate.instant('SPECIALIZATIONS.GENERALHOUSEKEEPING'), value: 4 }
            ];
        }, 1000);
    }

    disableOrEnableComissionTypeAndQuantity(bookingType: number | null) {
        if (bookingType == null) return;

        if (bookingType == 1 && this.quantity === 1) {
            this.quantity = this.monthlyWorkingDaysPerMonthSettingValue != null? Number(this.monthlyWorkingDaysPerMonthSettingValue) : 26;
        }

        if (bookingType == 0 || bookingType == 2) {
            this.commissionTypeIsDisabled = true;
            this.quantityIsDisabled = false;
            this.quantity = 1;
        } else {
            this.commissionTypeIsDisabled = false;
            this.quantityIsDisabled = false;
        }
    }

    private toDateOnlyString(date: Date | null): string | null {
        if (!date) return null;
        const year = date.getFullYear();
        const month = String(date.getMonth() + 1).padStart(2, '0');
        const day = String(date.getDate()).padStart(2, '0');
        return `${year}-${month}-${day}`;
    }

    goToPolicies() {
        this.router.navigate(['/policies']);
    }

    get payload(): any {
        return {
            description: this.description,
            monthlySalary: this.monthlySalary,
            dailySalary: this.dailySalary,
            hourlySalary: this.hourlySalary,
            bookingType: this.bookingType,
            commissionType: this.commissionType,
            specialization: this.specialization,
            specializations: this.additionalSpecializations,
            startDate: this.toDateOnlyString(this.startDate),
            quantity: this.quantity,
            currencyId: this.currencyId
        };
    }
    getJobPostCalculationInfo() {
        if (!this.startDate) {
            this.msg.add({ severity: 'warn', summary: this.translate.instant('COMMON.ERROR'), detail: this.translate.instant('BOOKING_DETAIL.TOAST_SELECT_START_DATE') });
            return;
        }
        if (!this.quantity || this.quantity < 1) {
            this.msg.add({ severity: 'warn', summary: this.translate.instant('COMMON.ERROR'), detail: this.translate.instant('BOOKING_DETAIL.TOAST_VALID_QUANTITY') });
            return;
        }

        this.api.getJobPostCalculationInfo(this.payload).subscribe({
            next: (data) => {
                this.JobPostCalculationInfo.set(data);
            },
            error: () => this.msg.add({ severity: 'error', summary: this.translate.instant('COMMON.ERROR'), detail: this.translate.instant('BOOKING_DETAIL.TOAST_CREATE_ERROR') })
        });
    }

    submit() {
        if (!this.description || !this.startDate) {
            this.msg.add({ severity: 'warn', detail: this.translate.instant('JOB_POST.FILL_ALL_FIELDS') });
            return;
        }

        if (!this.termsAccepted) {
            this.msg.add({ severity: 'warn', summary: this.translate.instant('COMMON.ERROR'), detail: this.translate.instant('CONSENT.REQUIRED') });
            return;
        }
        this.loading = true;

        const call = this.editId ? this.api.updateJobPost(this.editId, this.payload) : this.api.createJobPost(this.payload);
        call.subscribe({
            next: () => {
                this.msg.add({ severity: 'success', detail: this.translate.instant(this.editId ? 'JOB_POST.UPDATE_SUCCESS' : 'JOB_POST.CREATED') });
                setTimeout(() => this.router.navigate(['/homeowner/jobs']), 1500);
            },
            error: (er) => {
                this.msg.add({ severity: 'error', detail: er.error?.message || this.translate.instant(this.editId ? 'JOB_POST.UPDATE_FAILED' : 'JOB_POST.CREATE_FAILED') });
                this.loading = false;
            },
            complete: () => (this.loading = false)
        });
    }
}
