import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
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

@Component({
    selector: 'app-worker-search',
    standalone: true,
    imports: [CommonModule, FormsModule, RouterModule, CardModule, ButtonModule, InputTextModule, SelectModule, RatingModule, ChipModule, TranslatePipe],
    template: `
        <div class="card">
            <h2>البحث عن عاملة</h2>

            <div class="grid grid-cols-1 md:grid-cols-4 gap-4 mb-6">
                <div>
                    <label class="block font-bold mb-2">{{ 'COMMON.STATE' | translate }}</label>
                    <p-select
                        [(ngModel)]="filters.stateId"
                        [options]="statesOptions()"
                        [filter]="true"
                        [showClear]="true"
                        optionValue="id"
                        optionLabel="name"
                        (onChange)="onStateChange($event.value); search()"
                        [placeholder]="'WORKER_PROFILE.STATE_PLACEHOLDER' | translate"
                        class="w-full"
                    ></p-select>
                </div>

                <div>
                    <label class="block font-bold mb-2">{{ 'HOMEOWNER.CITY' | translate }}</label>
                    <p-select [(ngModel)]="filters.cityId" [showClear]="true" [options]="citiesOptions()" [filter]="true" optionValue="id" optionLabel="name" (onChange)="search()" [placeholder]="'WORKER_PROFILE.CITY_PLACEHOLDER' | translate" class="w-full"></p-select>
                </div>

                <div>
                    <label class="block font-bold mb-2">{{ 'WORKER_PROFILE.SPECIALIZATIONS_TITLE' | translate }}</label>
                    <p-select [(ngModel)]="filters.specialization" [showClear]="true" [options]="specializations" optionLabel="label" optionValue="value" placeholder="الكل" (onChange)="search()" styleClass="w-full"></p-select>
                </div>
                <div>
                    <label class="block font-bold mb-2">{{ 'WORKER.IS_LIVEIN' | translate }}</label>
                    <p-select [(ngModel)]="filters.isLiveIn" [options]="liveInOptions" optionLabel="label" optionValue="value" placeholder="الكل" (onChange)="search()" styleClass="w-full"></p-select>
                </div>
                <div>
                    <label class="block font-bold mb-2">أقصى أجر شهري</label>
                    <input pInputText [(ngModel)]="filters.maxRate" type="number" placeholder="5000" class="w-full" (input)="search()" />
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
                        <p-chip [label]="getSpecLabel(worker.specialization)"></p-chip>
                        <p-chip *ngIf="worker.isLiveIn" label="مقيمة"></p-chip>
                        <p-chip *ngIf="!worker.isLiveIn" label="يومية"></p-chip>
                    </div>

                    <div class="flex align-items-center justify-content-between">
                        <div>
                            <span class="text-2xl font-bold text-primary">{{ worker.monthlyRate | currency: 'EGP' : 'symbol' : '1.0-0' }}</span>
                            <span class="text-muted-color text-sm"> / شهرياً</span>
                        </div>
                        <p-button label="حجز" icon="pi pi-calendar" [rounded]="true" (onClick)="$event.stopPropagation(); viewWorker(worker.id)"></p-button>
                    </div>

                    <div class="text-sm text-muted-color mt-2">
                        <i class="pi pi-map-marker mr-1"></i>{{ worker.city || 'غير محدد' }}
                        <span class="ml-2">خبرة {{ worker.experienceYears }} سنوات</span>
                    </div>
                </div>
            </div>

            <div *ngIf="workers().length === 0 && !loading()" class="text-center py-8">
                <i class="pi pi-search text-4xl text-muted-color mb-4"></i>
                <p class="text-muted-color">لا توجد نتائج</p>
            </div>
        </div>
    `
})
export class WorkerSearch implements OnInit {
    private apiService = inject(ApiService);

    workers = signal<any[]>([]);
    loading = signal(false);
    statesOptions = signal<any[]>([]);
    citiesOptions = signal<any[]>([]);

    private globalizationSpecsService = inject(GlobalizationSpecsService);
    private messageService = inject(MessageService);
    private translate = inject(TranslateService);

    filters: any = {
        stateId: null,
        cityId: null,
        specialization: null,
        isLiveIn: null,
        maxRate: null
    };

    specializations = [
        { label: 'تنظيف', value: 0 },
        { label: 'طبخ', value: 1 },
        { label: 'رعاية أطفال', value: 2 },
        { label: 'رعاية مسنين', value: 3 },
        { label: 'عمل منزلي عام', value: 4 }
    ];

    liveInOptions = [
        { label: 'الكل', value: null },
        { label: 'مقيمة', value: true },
        { label: 'يومية', value: false }
    ];

    ngOnInit() {
        this.getStatesByCountryId();
        this.search();
    }

    search() {
        this.loading.set(true);
        const params: any = {};
        if (this.filters.stateId) params.stateId = this.filters.stateId;
        if (this.filters.cityId) params.cityId = this.filters.cityId;
        if (this.filters.specialization !== null) params.specialization = this.filters.specialization;
        if (this.filters.isLiveIn !== null) params.isLiveIn = this.filters.isLiveIn;
        if (this.filters.maxRate) params.maxRate = this.filters.maxRate;

        this.apiService.getWorkers(params).subscribe({
            next: (data) => {
                this.workers.set(data.data || []);
                this.loading.set(false);
            },
            error: () => this.loading.set(false)
        });
    }

    getSpecLabel(value: number): string {
        const labels: { [k: number]: string } = { 0: 'تنظيف', 1: 'طبخ', 2: 'رعاية أطفال', 3: 'رعاية مسنين', 4: 'عمل منزلي' };
        return labels[value] || 'غير محدد';
    }

    viewWorker(id: number) {
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
