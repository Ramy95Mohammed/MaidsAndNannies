import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { SelectModule } from 'primeng/select';
import { RatingModule } from 'primeng/rating';
import { ChipModule } from 'primeng/chip';
import { DividerModule } from 'primeng/divider';
import { TextareaModule } from 'primeng/textarea';
import { DatePickerModule } from 'primeng/datepicker';
import { DialogModule } from 'primeng/dialog';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';
import { ApiService } from '../../../core/services/api.service';
import { AuthService } from '../../../core/services/auth.service';
import { CurrencyService } from '@/core/services/currency.service';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { BookingDetailDto } from '@/core/services/booking.service';

@Component({
    selector: 'app-worker-detail',
    standalone: true,
    imports: [CommonModule, FormsModule, RouterModule, CardModule, ButtonModule, InputTextModule, SelectModule, RatingModule, ChipModule, DividerModule, TextareaModule, DatePickerModule, DialogModule, ToastModule, TranslatePipe],
    providers: [MessageService],
    template: `
        <p-toast></p-toast>
        <div class="card">
            <a routerLink="/homeowner/workers" class="text-primary cursor-pointer"> <i class="pi pi-arrow-left mr-2"></i>{{ 'WORKER_DETAIL.BACK_TO_SEARCH' | translate }} </a>
            <div *ngIf="worker()" class="mt-4">
                <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">
                    <div class="lg:col-span-2">
                        <div class="flex align-items-center gap-4 mb-4">
                            <div class="w-20 h-20 border-circle bg-primary flex align-items-center justify-content-center">
                                <i class="pi pi-user text-white text-3xl"></i>
                            </div>
                            <div>
                                <h2 class="text-2xl font-bold m-0">{{ worker().fullName }}</h2>
                                <p class="text-muted-color m-0">{{ worker().nationality }} - {{ worker().state || ('COMMON.UNSPECIFIED' | translate) }}</p>
                                <p-rating [(ngModel)]="worker().averageRating"></p-rating>
                                <span class="text-sm text-muted-color ml-2">{{ 'COMMON.RATING_COUNT' | translate: { count: worker().totalReviews } }}</span>
                            </div>
                        </div>

                        <p-divider></p-divider>

                        <h3>{{ 'WORKER_DETAIL.PERSONAL_INFO' | translate }}</h3>
                        <div class="grid grid-cols-2 gap-4">
                            <div>
                                <div>{{ 'WORKER_DETAIL.AGE' | translate: { years: worker().age } }}</div>
                                <div>{{ 'COMMON.EXPERIENCE_YEARS' | translate: { years: worker().experienceYears } }}</div>
                                <div>{{ 'WORKER_DETAIL.IS_LIVEIN' | translate: { value: worker().isLiveIn ? ('COMMON.YES' | translate) : ('COMMON.NO' | translate) } }}</div>
                            </div>

                            <div>
                                <div>{{ 'WORKER_DETAIL.DAILY_RATE' | translate: { rate: worker().dailyRate, currency: currenciesMap()[worker().currencyId] || 'EGP' } }}</div>
                                <div>{{ 'WORKER_DETAIL.MONTHLY_RATE' | translate: { rate: worker().monthlyRate, currency: currenciesMap()[worker().currencyId] || 'EGP' } }}</div>
                                <div>{{ 'WORKER_DETAIL.HOURLY_RATE' | translate: { rate: worker().hourlyRate, currency: currenciesMap()[worker().currencyId] || 'EGP' } }}</div>

                                <div class="mt-2">
                                    <div>
                                        <hr />
                                        <strong>{{ 'BOOKING.COMMISSION' | translate }}</strong>
                                    </div>

                                    <div class="mt-2">
                                         <p>{{ 'BOOKING.TOTAL_AMOUNT' | translate }}:{{ bookingCreationInfo()?.totalAmount | currency: bookingCreationInfo()?.currencyCode:'':'1.0-0' }} {{ bookingCreationInfo()?.currencyCode }}</p>
                                            <p>{{ 'BOOKING.TOTAL_AMOUNT_AFTER_CONVERSION' | translate }}:{{ bookingCreationInfo()?.totalAmountAfterConversion | currency:'EGP':'code':'1.0-0' }}</p>
                                            <p>{{ 'BOOKING.COMMISSION' | translate }}: {{ bookingCreationInfo()?.commissionAmount | currency:'EGP':'code':'1.0-0' }}</p>                                        
                                    </div>
                                </div>
                            </div>
                        </div>

                        <div class="mt-4">
                            <strong>{{ 'WORKER_DETAIL.SPECIALIZATION' | translate }}</strong>
                            <div class="flex flex-wrap gap-2 mt-2">
                                <p-chip [label]="getSpecLabel(worker().specializations)"></p-chip>
                            </div>
                        </div>

                        <div *ngIf="worker().bio" class="mt-4">
                            <strong>{{ 'WORKER_DETAIL.DESCRIPTION' | translate }}</strong>
                            <p class="mt-2">{{ worker().bio }}</p>
                        </div>

                        <div *ngIf="worker().languages" class="mt-4">
                            <strong>{{ 'WORKER_DETAIL.LANGUAGES' | translate }}</strong>
                            <div class="flex flex-wrap gap-2 mt-2">
                                <p-chip *ngFor="let lang of worker().languages.split(',')" [label]="lang.trim()"></p-chip>
                            </div>
                        </div>
                    </div>

                    <div class="card">
                        <h3 class="text-center">{{ 'WORKER_DETAIL.BOOK_NOW' | translate }}</h3>

                        <div class="mb-3">
                            <label class="block font-bold mb-2">{{ 'WORKER_DETAIL.BOOKING_TYPE' | translate }}</label>
                            <p-select [(ngModel)]="bookingType" (onChange)="disableOrEnableComissionTypeAndQuantity($event.value)" [options]="bookingTypes" optionLabel="label" optionValue="value" styleClass="w-full"></p-select>
                        </div>

                        <div class="mb-3">
                            <label class="block font-bold mb-2">{{ 'WORKER_DETAIL.COMMISSION_TYPE' | translate }}</label>
                            <p-select [(ngModel)]="commissionType" [disabled]="commissionTypeIsDisabled" [options]="commissionOptions" optionLabel="label" optionValue="value" styleClass="w-full"></p-select>
                        </div>

                        <div class="mb-3">
                            <label class="block font-bold mb-2">{{ 'WORKER_DETAIL.START_DATE' | translate }}</label>
                            <p-datepicker [(ngModel)]="startDate" dateFormat="yy-mm-dd" styleClass="w-full" [placeholder]="'WORKER_DETAIL.START_DATE' | translate"></p-datepicker>
                        </div>

                        <div class="mb-3">
                            <label class="block font-bold mb-2">{{ 'WORKER_DETAIL.QUANTITY' | translate }}</label>
                            <input pInputText [(ngModel)]="quantity" [disabled]="quantityIsDisabled" type="number" min="1" class="w-full" />
                        </div>

                        <div class="mb-3">
                            <label class="block font-bold mb-2">{{ 'WORKER_DETAIL.NOTES' | translate }}</label>
                            <textarea pTextarea [(ngModel)]="notes" rows="3" class="w-full" [placeholder]="'WORKER_DETAIL.NOTES_PLACEHOLDER' | translate"></textarea>
                        </div>

                        <p-button [label]="'WORKER_DETAIL.CALC_COMMISION' | translate" icon="pi pi-calculator" styleClass="w-full mb-2" (onClick)="getBookingCreationInfo()"></p-button>
                        <p-button [label]="'WORKER_DETAIL.CONFIRM_BOOKING' | translate" icon="pi pi-check" styleClass="w-full" (onClick)="createBooking()"></p-button>
                    </div>
                </div>

                <div class="mt-6">
                    <h3>{{ 'WORKER_DETAIL.REVIEWS' | translate }}</h3>
                    <div *ngFor="let review of worker().reviews || []" class="card mb-3">
                        <div class="flex align-items-center gap-2 mb-2">
                            <p-rating [(ngModel)]="review.rating"></p-rating>
                            <span class="text-sm text-muted-color">{{ review.createdAt | date: 'short' }}</span>
                        </div>
                        <p>{{ review.comment }}</p>
                    </div>
                    <div *ngIf="!worker().reviews || worker().reviews.length === 0" class="text-muted-color">
                        {{ 'WORKER_DETAIL.NO_REVIEWS' | translate }}
                    </div>
                </div>
            </div>
        </div>
    `
})
export class WorkerDetail implements OnInit {
    private apiService = inject(ApiService);
    private route = inject(ActivatedRoute);
    private router = inject(Router);
    private messageService = inject(MessageService);
    private currencyService = inject(CurrencyService);
    private translate = inject(TranslateService);

    currenciesMap = signal<{ [id: number]: string }>({});

    bookingCreationInfo = signal<BookingDetailDto | null>(null);

    worker = signal<any>(null);
    bookingType = 0;
    commissionType = 0;
    startDate: Date | null = null;
    quantity = 1;
    notes = '';

    commissionTypeIsDisabled: boolean = true;
    quantityIsDisabled: boolean = false;

    bookingTypes: any = [
        { label: 'يومي', value: 0 },
        { label: 'شهري', value: 1 },
        { label: 'ساعي', value: 2 }
    ];

    commissionOptions = [
        { label: 'عمولة من أول شهر', value: 0 },
        { label: 'اشتراك شهري', value: 1 }
    ];

    ngOnInit() {
        const id = this.route.snapshot.paramMap.get('id');
        if (id) {
            this.apiService.getWorker(id).subscribe({
                next: (data) => this.worker.set(data),
                error: () => this.messageService.add({ severity: 'error', summary: this.translate.instant('COMMON.ERROR'), detail: this.translate.instant('BOOKING_DETAIL.TOAST_WORKER_NOT_FOUND') })
            });
        }

        this.currencyService.loadCurrencies(this.currenciesMap);

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
        }, 1000);
    }

    getSpecLabel(values: number[]): string {
        const map: { [key: number]: string } = {
            0: this.translate.instant('SPECIALIZATIONS.CLEANING'),
            1: this.translate.instant('SPECIALIZATIONS.COOKING'),
            2: this.translate.instant('SPECIALIZATIONS.CHILDCARE'),
            3: this.translate.instant('SPECIALIZATIONS.ELDERLYCARE'),
            4: this.translate.instant('SPECIALIZATIONS.GENERALHOUSEKEEPING')
        };

        return values?.map((v: any) => map[v] ?? this.translate.instant('COMMON.UNSPECIFIED')).join(', ');
    }

    getBookingCreationInfo() {
        if (!this.startDate) {
            this.messageService.add({ severity: 'warn', summary: this.translate.instant('COMMON.ERROR'), detail: this.translate.instant('BOOKING_DETAIL.TOAST_SELECT_START_DATE') });
            return;
        }
        if (!this.quantity || this.quantity < 1) {
            this.messageService.add({ severity: 'warn', summary: this.translate.instant('COMMON.ERROR'), detail: this.translate.instant('BOOKING_DETAIL.TOAST_VALID_QUANTITY') });
            return;
        }

        this.apiService
            .getBookingCreationInfo({
                workerId: this.worker().id,
                serviceType: this.bookingType,
                bookingType: this.bookingType,
                quantity: this.quantity,
                startDate: this.toDateOnlyString(this.startDate),
                monthlySalary: this.worker().monthlyRate,
                dailySalary: this.worker().dailyRate,
                hourlySalary: this.worker().hourlyRate,
                commissionType: this.commissionType
            })
            .subscribe({
                next: (data) => {
                    this.bookingCreationInfo.set(data);
                },
                error: () => this.messageService.add({ severity: 'error', summary: this.translate.instant('COMMON.ERROR'), detail: this.translate.instant('BOOKING_DETAIL.TOAST_CREATE_ERROR') })
            });
    }

    createBooking() {
        if (!this.startDate) {
            this.messageService.add({ severity: 'warn', summary: this.translate.instant('COMMON.ERROR'), detail: this.translate.instant('BOOKING_DETAIL.TOAST_SELECT_START_DATE') });
            return;
        }
        if (!this.quantity || this.quantity < 1) {
            this.messageService.add({ severity: 'warn', summary: this.translate.instant('COMMON.ERROR'), detail: this.translate.instant('BOOKING_DETAIL.TOAST_VALID_QUANTITY') });
            return;
        }

        this.apiService
            .createBooking({
                workerId: this.worker().id,
                serviceType: this.bookingType,
                bookingType: this.bookingType,
                quantity: this.quantity,
                startDate: this.toDateOnlyString(this.startDate),
                monthlySalary: this.worker().monthlyRate,
                dailySalary: this.worker().dailyRate,
                hourlySalary: this.worker().hourlyRate,
                commissionType: this.commissionType
            })
            .subscribe({
                next: () => {
                    this.messageService.add({ severity: 'success', summary: this.translate.instant('COMMON.SUCCESS'), detail: this.translate.instant('BOOKING_DETAIL.TOAST_CREATE_SUCCESS') });
                    setTimeout(() => this.router.navigate(['/homeowner/bookings']), 1500);
                },
                error: () => this.messageService.add({ severity: 'error', summary: this.translate.instant('COMMON.ERROR'), detail: this.translate.instant('BOOKING_DETAIL.TOAST_CREATE_ERROR') })
            });
    }

    private toDateOnlyString(date: Date | null): string | null {
        if (!date) return null;
        const year = date.getFullYear();
        const month = String(date.getMonth() + 1).padStart(2, '0');
        const day = String(date.getDate()).padStart(2, '0');
        return `${year}-${month}-${day}`;
    }

    disableOrEnableComissionTypeAndQuantity(bookingType: number | null) {
        if (bookingType == null) return;

        if (bookingType == 0 || bookingType == 2) {
            this.commissionTypeIsDisabled = true;
            this.quantityIsDisabled = false;
        } else {
            this.commissionTypeIsDisabled = false;
            this.quantityIsDisabled = true;
        }
    }
}
