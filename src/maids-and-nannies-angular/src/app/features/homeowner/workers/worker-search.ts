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
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { ApiService } from '../../../core/services/api.service';
import { GlobalizationSpecsService } from '@/core/services/globalization-specs.service';
import { MessageService } from 'primeng/api';
import { BookingService } from '@/core/services/booking.service';
import { CurrencyService } from '@/core/services/currency.service';
import { Paginator } from "primeng/paginator";
import { LanguageService } from '@/core/services/language.service';
import { MultiSelect } from "primeng/multiselect";

@Component({
    selector: 'app-worker-search',
    standalone: true,
    imports: [CommonModule, FormsModule, RouterModule, CardModule, ButtonModule, InputTextModule, SelectModule, RatingModule, ChipModule, TranslatePipe, Paginator, MultiSelect],
    template: `
        <div class="card">
           <h2>{{ 'WORKERS.SEARCH' | translate }}</h2>
            <div class="grid grid-cols-1 md:grid-cols-4 gap-4 mb-6">
                <div>
                    <label class="block font-bold mb-2">{{ 'COMMON.STATE' | translate }}</label>
                    <p-select
                        [(ngModel)]="filters.stateId"
                        [options]="statesOptions()"
                        [filter]="true"
                        [filterFields]="['name_ar' , 'name_en']" 
                        [showClear]="true"
                        optionValue="id"
                        optionLabel="name"
                        (onChange)="onStateChange($event.value); search()"
                        [placeholder]="'COMMON.STATE' | translate"
                        class="w-full"
                    >
                      <ng-template #selectedItem let-selectedOption>
                                            @if (selectedOption) {
                                                <div class="flex items-center gap-3">
                                                    <div>{{ isAr ? selectedOption.name_ar:selectedOption.name_en }}</div>
                                                </div>
                                            }
                                        </ng-template>
                                        <ng-template let-state #item>
                                            <div class="flex items-center gap-3">
                                                  <div>{{ isAr ? state.name_ar:state.name_en }}</div>
                                            </div>
                                        </ng-template>
                                        <ng-template #dropdownicon>
                                            <i class="pi pi-map"></i>
                                        </ng-template>
                    </p-select>
                </div>

                <div>
                    <label class="block font-bold mb-2">{{ 'HOMEOWNER.CITY' | translate }}</label>
                    <p-select [(ngModel)]="filters.cityId" [showClear]="true"
                     [options]="citiesOptions()" 
                      [filterFields]="['name_ar' , 'name_en']"
                    [filter]="true" optionValue="id" optionLabel="name" (onChange)="search()" 
                    [placeholder]="'WORKER_PROFILE.CITY_PLACEHOLDER' | translate" class="w-full">
                     <ng-template #selectedItem let-selectedOption>
                                @if (selectedOption) {
                                    <div class="flex items-center gap-3">
                                          <div>{{ selectedOption.name_ar  }}</div>
                                   <div>{{ selectedOption.name_en }}</div>                                        
                                    </div>
                                }
                            </ng-template>
                            <ng-template let-city #item>
                                <div class="flex items-center gap-3">
                                   <div>{{ city.name_ar  }}</div>
                                   <div>{{ city.name_en }}</div>
                                </div>
                            </ng-template>
                            <ng-template #dropdownicon>
                                <i class="pi pi-flag"></i>
                            </ng-template>
                    </p-select>
                </div>

                <div>
                    <label class="block font-bold mb-2">{{ 'WORKER_PROFILE.SPECIALIZATIONS_TITLE' | translate }}</label>
                    <p-multiselect (onChange)="search()" [options]="specializations" [(ngModel)]="filters.specializations" optionLabel="label" [placeholder]="'COMMON.ALL' | translate" class="w-full md:w-80" />
                    <!-- <p-select [(ngModel)]="filters.specialization" [showClear]="true" [options]="specializations" optionLabel="label" optionValue="value" [placeholder]="'COMMON.ALL' | translate" (onChange)="search()" styleClass="w-full"></p-select> -->
                </div>
                <div>
                    <label class="block font-bold mb-2">{{ 'WORKER.IS_LIVEIN' | translate }}</label>
                    <p-select [(ngModel)]="filters.isLiveIn" [options]="liveInOptions" optionLabel="label" optionValue="value" [placeholder]="'COMMON.ALL' | translate" (onChange)="search()" styleClass="w-full"></p-select>
                </div>
                <div>
                    <label class="block font-bold mb-2">{{ 'WORKER.MAXIMUM_MONTHLY_SALARY' | translate }}</label>

                    <input pInputText [(ngModel)]="filters.maxRate" type="number" placeholder="5000" class="w-full" (input)="search()" />
                </div>

                  <div>
                      <label class="block font-bold mb-2">{{ 'WORKER.CURRENCY' | translate }}</label>
                        <p-select   (onChange)="search()" [(ngModel)]="filters.currencyId" [showClear]="true" [filter]="true"  filterBy="label"  [options]="currencyOptions()" optionLabel="label" optionValue="value" [placeholder]="'COMMON.ALL' | translate"  class="w-full"></p-select>
                  </div>
            </div>

            <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
                <div *ngFor="let worker of workers()" class="card cursor-pointer hover:shadow-lg transition-duration-300" (click)="viewWorker(worker.id)">
                    <div class="flex align-items-center gap-3 mb-3">
                        <div class="w-12 h-12 border-circle bg-primary flex align-items-center justify-content-center">
                            <i class="pi pi-user text-white"></i>
                        </div>
                        <div>
                            <div class="font-bold text-lg">{{ worker.fullName }}</div>
                            <div class="text-sm text-muted-color">{{ worker.nationality }}</div>
                        </div>
                    </div>

                    <div class="mb-3">
                        <p-rating [(ngModel)]="worker.averageRating"></p-rating>
                        <span class="text-sm text-muted-color ml-2">({{ worker.totalReviews }})</span>
                    </div>

                    <div class="flex flex-wrap gap-2 mb-3">
                        <p-chip [label]="getSpecLabel(worker.specializations)"></p-chip>
                        <p-chip *ngIf="worker.isLiveIn" [label]="'COMMON.YES' | translate"></p-chip>
                        <p-chip *ngIf="!worker.isLiveIn" [label]="'WORKER_DETAIL.DAILY' | translate"></p-chip>

                    </div>

                    <div class="flex align-items-center justify-content-between">
                        <div>
                            <span class="text-2xl font-bold text-primary">{{ worker.monthlyRate }} {{ currenciesMap()[worker.currencyId] || 'EGP' }}</span>
                            <span class="text-muted-color text-sm mx-2">{{ 'COMMON.PER_MONTH' | translate }}</span>

                        </div>
                         <div class="text-sm text-muted-color mt-1">
                            <span>{{ 'WORKER_DETAIL.DAILY' | translate }}: {{ worker.dailyRate  }} {{ currenciesMap()[worker.currencyId] || 'EGP' }}</span>
                            <span class="mx-2"> |  {{ 'WORKER_DETAIL.HOURLY' | translate }}: {{ worker.hourlyRate  }} {{ currenciesMap()[worker.currencyId] || 'EGP' }}</span>
                        </div>
                        <p-button [label]="'WORKERS.BOOK' | translate" icon="pi pi-calendar" [rounded]="true" (onClick)="$event.stopPropagation(); viewWorker(worker.id)"></p-button>
                    </div>

                    <div class="text-sm text-muted-color mt-2">
                        <i class="pi pi-map-marker mr-1"></i>{{ worker.state || ('COMMON.UNSPECIFIED' | translate) }} / 
                        <span class="ml-2">{{ 'COMMON.EXPERIENCE_YEARS' | translate:{years: worker.experienceYears} }}</span>
                    </div>
                </div>
            </div>

            <div *ngIf="totalCount > pageSize" class="mt-4">
                <p-paginator
                    [totalRecords]="totalCount"
                    [rows]="pageSize"
                    [first]="(page - 1) * pageSize"
                    (onPageChange)="onPageChange($event)"
                ></p-paginator>
            </div>

            <div *ngIf="isReplacementMode()" class="p-3 border-round mb-4 flex align-items-center gap-2">
                <i class="pi pi-refresh text-orange-500"></i>
                <span>{{ 'BOOKING_DETAIL.REPLACEMENT_SELECT' | translate:{id: replacementBookingId()} }}</span>
                <p-button [label]="'COMMON.CANCEL' | translate" size="small" severity="secondary" (onClick)="router.navigate(['/homeowner/bookings', replacementBookingId()!])" class="mr-auto"></p-button>
            </div>

            <div *ngIf="workers().length === 0 && !loading()" class="text-center py-8">
                <i class="pi pi-search text-4xl text-muted-color mb-4"></i>
                <p class="text-muted-color">{{ 'COMMON.NO_DATA' | translate }}</p>
            </div>
        </div>
    `
})
export class WorkerSearch implements OnInit {
    private apiService = inject(ApiService);
    private currencyService = inject(CurrencyService);
    langService = inject(LanguageService);

    isAr:boolean = true;

    workers = signal<any[]>([]);
    loading = signal(false);
    statesOptions = signal<any[]>([]);
    citiesOptions = signal<any[]>([]); 
    isReplacementMode = signal(false);
    replacementBookingId = signal<number | null>(null);
    replacementReason = signal<0 | 1>(1);
    currenciesMap = signal<{ [id: number]: string }>({});
    currencyOptions = signal<{ value: number; label: string }[]>([]);

    //paginator
    page = 1;
    pageSize = 12;
    totalCount = 0;



    private globalizationSpecsService = inject(GlobalizationSpecsService);
    private messageService = inject(MessageService);
    private translate = inject(TranslateService);
    private route = inject(ActivatedRoute);
    public router = inject(Router);
    private bookingService = inject(BookingService);

    filters: any = {
        stateId: null,
        cityId: null,
        specializations: null,
        isLiveIn: null,
        maxRate: null,
        currencyId:null
    };

   specializations:any = [];
   liveInOptions:any = [];

private getSpecializations() {
    const t = this.translate;
    return [
        { label: t.instant('SPECIALIZATIONS.CLEANING'), value: 0 },
        { label: t.instant('SPECIALIZATIONS.COOKING'), value: 1 },
        { label: t.instant('SPECIALIZATIONS.CHILDCARE'), value: 2 },
        { label: t.instant('SPECIALIZATIONS.ELDERLYCARE'), value: 3 },
        { label: t.instant('SPECIALIZATIONS.GENERALHOUSEKEEPING'), value: 4 }
    ];
}

private getliveInOptions(){
      const t = this.translate;
      return[
         { label: t.instant('WORKER_DETAIL.ALL'), value: null },
        { label:t.instant('WORKER_DETAIL.LIVE_IN'), value: true },
        { label: t.instant('WORKER_DETAIL.DAILY'), value: false }
      ]
}   
    ngOnInit() {

        this.isAr = this.langService.getCurrentLanguage() === 'ar';

           setTimeout(() => {            
          this.specializations =  this.getSpecializations();
          this.liveInOptions = this.getliveInOptions();
        }, 1000);



         this.route.queryParams.subscribe(params => {
        if (params['mode'] === 'replacement' && params['bookingId']) {
            this.isReplacementMode.set(true);
            this.replacementBookingId.set(Number(params['bookingId']));
            this.replacementReason.set(params['reason'] === '0' ? 0 : 1);
        }
      this.loadCurrencies();

    });
    
    this.currencyService.loadCurrencies(this.currenciesMap);
        this.getStatesByCountryId();
        this.search();
    }

    search() {                
        this.loading.set(true);
        const params: any = {};
        if (this.filters.stateId) params.stateId = this.filters.stateId;
        if (this.filters.cityId) params.cityId = this.filters.cityId;
        if (this.filters.specializations !== null) params.specializations = this.filters.specializations?.map((s:any)=>s.value);
        if (this.filters.isLiveIn !== null) params.isLiveIn = this.filters.isLiveIn;
        if (this.filters.maxRate) params.maxRate = this.filters.maxRate;
        if (this.filters.currencyId) params.currencyId = this.filters.currencyId;
        
        params.page = this.page;
        params.pageSize = this.pageSize;

        this.apiService.getWorkers(params).subscribe({
            next: (data) => {
                
                this.workers.set(data.data || []);
                this.totalCount = data.totalCount || 0;

                this.loading.set(false);
            },
            error: () => this.loading.set(false)
        });
    }

        onPageChange(event: any) {
        this.page = (event.first / event.rows) + 1;
        this.pageSize = event.rows;
        this.search();
    }

     loadCurrencies() {
        this.currencyService.getCurrencies().subscribe({
            next: (data) => {
                const isAr = this.langService.getCurrentLanguage() === 'ar';
                this.currencyOptions.set(data.map(c => ({
                    value: c.id,
                    label: isAr ? `${c.nameAr} (${c.code})` : `${c.nameEn} (${c.code})`
                })));
            }
        });
    }

   getSpecLabel(values: number[]): string {
  const map: { [key: number]: string } = {
    0: this.translate.instant('SPECIALIZATIONS.CLEANING'),
    1: this.translate.instant('SPECIALIZATIONS.COOKING'),
    2: this.translate.instant('SPECIALIZATIONS.CHILDCARE'),
    3: this.translate.instant('SPECIALIZATIONS.ELDERLYCARE'),
    4: this.translate.instant('SPECIALIZATIONS.GENERALHOUSEKEEPING')
  };

  return values
    .map(v => map[v] ?? this.translate.instant('COMMON.UNSPECIFIED'))
    .join(', ');
}
   

    viewWorker(id: number) {
    if (this.isReplacementMode()) {
        // وضع استبدال: يطلب التأكيد ثم يستبدل
        this.bookingService.requestReplacement(this.replacementBookingId()!, this.replacementReason(), id).subscribe({
            next: () => {
                this.messageService.add({ severity: 'success', detail: this.translate.instant('BOOKING_DETAIL.REPLACEMENT_REQUESTED') });
                this.router.navigate(['/homeowner/bookings', this.replacementBookingId()!]);
            }
        });
        return;
    }
    window.location.href = '/homeowner/workers/' + id;
}

    getStatesByCountryId() {
        this.statesOptions.set([]);
        this.globalizationSpecsService.getStatesByCountryId(65).subscribe({
            next: (data: any[]) => this.statesOptions.set(data),
            error: () =>
                this.messageService.add({
                    severity: 'error',
                    summary: this.translate.instant('COMMON.ERROR'),
                    detail: this.translate.instant('WORKER_PROFILE.TOAST_LOAD_STATES_ERROR')
                })
        });
    }

    onStateChange(stateId: number, resetCity: boolean = true) {
        this.citiesOptions.set([]);
        if (!stateId) return;
        this.globalizationSpecsService.getCitiesByStateId(stateId).subscribe({
            next: (data: any[]) => this.citiesOptions.set(data),
            error: () =>
                this.messageService.add({
                    severity: 'error',
                    summary: this.translate.instant('COMMON.ERROR'),
                    detail: this.translate.instant('WORKER_PROFILE.TOAST_LOAD_CITIES_ERROR')
                })
        });
    }
}