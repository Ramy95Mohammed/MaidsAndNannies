import { Component, inject, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
    FormBuilder,
    FormArray,
    FormGroup,
    ReactiveFormsModule,
    Validators
} from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
import { SelectModule } from 'primeng/select';
import { CheckboxModule } from 'primeng/checkbox';
import { TextareaModule } from 'primeng/textarea';
import { DatePickerModule } from 'primeng/datepicker';
import { TableModule } from 'primeng/table';
import { FileUploadModule, FileSelectEvent } from 'primeng/fileupload';
import { TagModule } from 'primeng/tag';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';
import { LangChangeEvent, TranslatePipe, TranslateService } from '@ngx-translate/core';
import { ApiService } from '../../../core/services/api.service';
import { AuthService } from '../../../core/services/auth.service';
import { WorkerSpecializationSpec } from '@/core/interfaces/worker-specialization-spec';
import { GlobalizationSpecsService } from '@/core/services/globalization-specs.service';
import { WorkerProfile } from '@/core/interfaces/worker-profile';
import { WorkerService } from '@/core/services/worker-service';

// -- Domain enums (mirrors backend MaidsPlatform.API.Domain.Enums) --------
enum Specialization {
    Cleaning = 0,
    Cooking = 1,
    Childcare = 2,
    ElderlyCare = 3,
    GeneralHousekeeping = 4
}

enum DocumentType {
    Passport = 0,
    NationalId = 1,
    Selfie = 2,
    ProofOfAddress = 3,
    CriminalRecord = 4
}

enum VerificationStatus {
    Pending = 0,
    Verified = 1,
    Rejected = 2
}

// Translation keys under SPECIALIZATIONS.* (see public/i18n/{ar,en}.json)
const SPECIALIZATION_KEYS: { value: Specialization; key: string }[] = [
    { value: Specialization.Cleaning, key: 'CLEANING' },
    { value: Specialization.Cooking, key: 'COOKING' },
    { value: Specialization.Childcare, key: 'CHILDCARE' },
    { value: Specialization.ElderlyCare, key: 'ELDERLYCARE' },
    { value: Specialization.GeneralHousekeeping, key: 'GENERALHOUSEKEEPING' }
];

// NOTE: no currency enum was provided in the domain snippets shared -
// adjust this list to match the real backend enum once available.
// Translation keys live under WORKER_PROFILE.CURRENCY_*.
const CURRENCY_KEYS = [
    { value: 0, key: 'CURRENCY_EGP' },
    { value: 1, key: 'CURRENCY_USD' },
    { value: 2, key: 'CURRENCY_SAR' }
];

@Component({
    selector: 'app-worker-profile',
    standalone: true,
    imports: [
        CommonModule,
        ReactiveFormsModule,
        RouterModule,
        CardModule,
        ButtonModule,
        InputTextModule,
        InputNumberModule,
        SelectModule,
        CheckboxModule,
        TextareaModule,
        DatePickerModule,
        TableModule,
        FileUploadModule,
        TagModule,
        ToastModule,
        TranslatePipe
    ],
    providers: [MessageService],
    template: `
        <p-toast></p-toast>

        <form *ngIf="!loading()" [formGroup]="form" (ngSubmit)="saveProfile()" class="card flex flex-col gap-6">
            <h2>{{ 'WORKER_PROFILE.TITLE' | translate }}</h2>

            <!-- ================= البيانات الأساسية ================= -->
            <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
                <div class="flex flex-col gap-4">
                    <h3>{{ 'WORKER_PROFILE.BASIC_INFO' | translate }}</h3>

                    <div class="flex flex-col gap-2">
                        <label class="font-bold">{{ 'AUTH.FULL_NAME' | translate }}</label>
                        <input pInputText [value]="userFullName()" class="w-full" disabled />
                    </div>

                    <div class="flex flex-col gap-2">
                        <label class="font-bold">{{ 'AUTH.EMAIL' | translate }}</label>
                        <input pInputText  formControlName="email"  class="w-full"  />
                    </div>

                    <div class="grid grid-cols-2 gap-4">
                        <div class="flex flex-col gap-2">
                            <label class="font-bold">{{ 'AUTH.PHONE' | translate }}</label>
                            <input pInputText [value]="profile()?.phoneNumber ?? ''" class="w-full" disabled />
                        </div>
                        <div class="flex flex-col gap-2">
                            <label class="font-bold">{{ 'WORKER_PROFILE.WHATSAPP_NUMBER' | translate }}</label>
                            <input pInputText formControlName="whatsAppNumber" class="w-full" [placeholder]="'WORKER_PROFILE.WHATSAPP_PLACEHOLDER' | translate" />
                        </div>

                        <div class="flex flex-col gap-2">
                            <label class="font-bold">{{ 'WORKER.BIRTHDATE' | translate }}</label>
                            <p-datepicker formControlName="birthDate" dateFormat="dd/mm/yy" inputStyleClass="w-full" [showIcon]="true" class="w-full"></p-datepicker>
                        </div>
                        
                    </div>

                    <div class="flex flex-col gap-2">
                        <label class="font-bold">{{ 'WORKER.NATIONALITY' | translate }}</label>
                        <p-select
                            formControlName="nationalityId"
                            [options]="countriesOptions()"
                            [filter]="true"
                            [filterFields]="['nationality', 'native']"
                            optionValue="id"
                            optionLabel="nationality"
                            [placeholder]="'WORKER.NATIONALITY' | translate"
                            class="w-full"
                        >
                            <ng-template #selectedItem let-selectedOption>
                                @if (selectedOption) {
                                    <div class="flex items-center gap-3">
                                        <div>{{ selectedOption.nationality }}</div>
                                        <div class="text-color-secondary">{{ selectedOption.native }}</div>
                                    </div>
                                }
                            </ng-template>
                            <ng-template let-country #item>
                                <div class="flex items-center gap-3">
                                    <div>{{ country.nationality }}</div>
                                    <div class="text-color-secondary">{{ country.native }}</div>
                                </div>
                            </ng-template>
                            <ng-template #dropdownicon>
                                <i class="pi pi-flag"></i>
                            </ng-template>
                        </p-select>
                    </div>

                    <div class="flex flex-col gap-2">
                        <label class="font-bold">{{ 'WORKER_PROFILE.NATIONAL_ID_NUMBER' | translate }}</label>
                        <input pInputText formControlName="nationalIdNumber" class="w-full" />
                    </div>

                    <div class="grid grid-cols-2 gap-4">
                        <div class="flex flex-col gap-2">
                            <label class="font-bold">{{ 'WORKER.PASSPORT_NUMBER' | translate }}</label>
                            <input pInputText formControlName="passportNumber" class="w-full" />
                        </div>
                        <div class="flex flex-col gap-2">
                            <label class="font-bold">{{ 'WORKER.PASSPORT_EXPIRY' | translate }}</label>
                            <p-datepicker formControlName="passportExpiryDate" dateFormat="dd/mm/yy" [showIcon]="true" class="w-full"></p-datepicker>
                        </div>
                    </div>

                    <div class="flex flex-col gap-2">
                        <label class="font-bold">{{ 'WORKER.PASSPORT_COUNTRY' | translate }}</label>
                        <input pInputText formControlName="passportCountry" class="w-full" />
                    </div>
                </div>

                <!-- ================= الموقع ================= -->
                <div class="flex flex-col gap-4">
                    <h3>{{ 'WORKER_PROFILE.LOCATION' | translate }}</h3>

                    <div class="flex flex-col gap-2">
                        <label class="font-bold">{{ 'COMMON.COUNTRY' | translate }}</label>
                        <p-select
                            formControlName="countryId"
                            [options]="countriesOptions()"
                             [filter]="true"
                            [filterFields]="['nationality', 'native']"
                            optionValue="id"
                            optionLabel="name"
                            (onChange)="onCountryChange($event.value)"
                            [placeholder]="'COMMON.COUNTRY' | translate"
                            class="w-full"
                        > <ng-template #selectedItem let-selectedOption>
                                @if (selectedOption) {
                                    <div class="flex items-center gap-3">
                                        <div>{{ selectedOption.nationality }}</div>
                                        <div class="text-color-secondary">{{ selectedOption.native }}</div>
                                    </div>
                                }
                            </ng-template>
                            <ng-template let-country #item>
                                <div class="flex items-center gap-3">
                                    <div>{{ country.nationality }}</div>
                                    <div class="text-color-secondary">{{ country.native }}</div>
                                </div>
                            </ng-template>
                            <ng-template #dropdownicon>
                                <i class="pi pi-flag"></i>
                            </ng-template>
                        </p-select>
                    </div>

                    <div class="flex flex-col gap-2">
                        <label class="font-bold">{{ 'COMMON.STATE' | translate }}</label>
                        <p-select
                            formControlName="stateId"
                            [options]="statesOptions()"
                            [filter]="true"
                            optionValue="id"
                            optionLabel="name"
                            (onChange)="onStateChange($event.value)"
                            [placeholder]="'WORKER_PROFILE.STATE_PLACEHOLDER' | translate"
                            class="w-full"
                        ></p-select>
                    </div>

                    <div class="flex flex-col gap-2">
                        <label class="font-bold">{{ 'HOMEOWNER.CITY' | translate }}</label>
                        <p-select
                            formControlName="cityId"
                            [options]="citiesOptions()"
                            [filter]="true"
                            optionValue="id"
                            optionLabel="name"
                            [disabled]="!form.value.stateId"
                            [placeholder]="'WORKER_PROFILE.CITY_PLACEHOLDER' | translate"
                            class="w-full"
                        ></p-select>
                    </div>

                    <div class="flex flex-col gap-2">
                        <label class="font-bold">{{ 'WORKER_PROFILE.ADDRESS_DETAILED' | translate }}</label>
                        <textarea pTextarea formControlName="address" rows="3" class="w-full"></textarea>
                    </div>
                </div>
            </div>

            <!-- ================= المعلومات المهنية ================= -->
            <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
                <div class="flex flex-col gap-4">
                    <h3>{{ 'WORKER_PROFILE.PROFESSIONAL_INFO' | translate }}</h3>

                    <div class="flex flex-col gap-2">
                        <label class="font-bold">{{ 'WORKER.BIO' | translate }}</label>
                        <textarea pTextarea formControlName="bio" rows="4" class="w-full" [placeholder]="'WORKER_PROFILE.BIO_PLACEHOLDER' | translate"></textarea>
                    </div>

                    <div class="grid grid-cols-2 gap-4">
                        <div class="flex flex-col gap-2">
                            <label class="font-bold">{{ 'WORKER.EXPERIENCE' | translate }}</label>
                            <p-inputnumber formControlName="experienceYears" [min]="0" class="w-full"></p-inputnumber>
                        </div>
                        <div class="flex flex-col gap-2">
                            <label class="font-bold">{{ 'WORKER.LANGUAGES' | translate }}</label>
                            <input pInputText formControlName="languages" class="w-full" [placeholder]="'WORKER_PROFILE.LANGUAGES_PLACEHOLDER' | translate" />
                        </div>
                    </div>

                    <div class="flex flex-col gap-2">
                        <label class="font-bold">{{ 'WORKER_PROFILE.PREVIOUS_EMPLOYER' | translate }}</label>
                        <input pInputText formControlName="previousEmployer" class="w-full" />
                    </div>

                    <div class="flex align-items-center gap-2">
                        <p-checkbox formControlName="isLiveIn" [binary]="true"></p-checkbox>
                        <label>{{ 'WORKER.IS_LIVEIN' | translate }}</label>
                    </div>

                    <div class="flex align-items-center gap-2">
                        <p-checkbox formControlName="isAvailable" [binary]="true"></p-checkbox>
                        <label>{{ 'WORKER_PROFILE.AVAILABLE_LABEL' | translate }}</label>
                    </div>
                </div>

                <!-- ================= المعلومات المالية ================= -->
                <div class="flex flex-col gap-4">
                    <h3>{{ 'WORKER_PROFILE.FINANCIAL_INFO' | translate }}</h3>

                    <div class="flex flex-col gap-2">
                        <label class="font-bold">{{ 'WORKER.MONTHLY_RATE' | translate }}</label>
                        <p-inputnumber formControlName="monthlyRate" mode="decimal" [minFractionDigits]="0" [min]="0" class="w-full"></p-inputnumber>
                    </div>

                    <div class="flex flex-col gap-2">
                        <label class="font-bold">{{ 'WORKER.HOURLY_RATE' | translate }}</label>
                        <p-inputnumber formControlName="hourlyRate" mode="decimal" [minFractionDigits]="0" [min]="0" class="w-full"></p-inputnumber>
                    </div>

                    <div class="flex flex-col gap-2">
                        <label class="font-bold">{{ 'WORKER.CURRENCY' | translate }}</label>
                        <p-select formControlName="currency" [options]="currencyOptions()" optionLabel="label" optionValue="value" class="w-full"></p-select>
                    </div>

                    <h3>{{ 'WORKER_PROFILE.ACCOUNT_STATUS' | translate }}</h3>
                    <div class="flex align-items-center gap-3 flex-wrap">
                        <p-tag
                            [value]="verificationStatusLabel()"
                            [severity]="verificationStatusSeverity()"
                        ></p-tag>
                        <span>{{ 'WORKER_PROFILE.RATING' | translate: { rating: profile()?.averageRating ?? 0, count: profile()?.totalReviews ?? 0 } }}</span>
                    </div>
                </div>
            </div>

            <!-- ================= التخصصات ================= -->
            <div>
                <div class="flex justify-content-between align-items-center mb-3">
                    <h3 class="m-0">{{ 'WORKER_PROFILE.SPECIALIZATIONS_TITLE' | translate }}</h3>
                    <p-button [label]="'WORKER_PROFILE.ADD_SPECIALIZATION' | translate" icon="pi pi-plus" size="small" [outlined]="true" (onClick)="addSpecializationRow()"></p-button>
                </div>

                <p-table [value]="specializationsArray.controls" dataKey="index">
                    <ng-template #header>
                        <tr>
                            <th>{{ 'WORKER.SPECIALIZATION' | translate }}</th>
                            <th style="width: 6rem">{{ 'COMMON.DELETE' | translate }}</th>
                        </tr>
                    </ng-template>
                    <ng-template #body let-row let-i="rowIndex">
                        <tr [formGroup]="row">
                            <td>
                                <p-select
                                    formControlName="specialization"
                                    [options]="specializationOptions()"
                                    optionLabel="label"
                                    optionValue="value"
                                    [placeholder]="'WORKER_PROFILE.SELECT_SPECIALIZATION_PLACEHOLDER' | translate"
                                    appendTo="body"
                                    class="w-full"
                                ></p-select>
                            </td>
                            <td>
                                <p-button icon="pi pi-trash" severity="danger" [text]="true" (onClick)="removeSpecializationRow(i)"></p-button>
                            </td>
                        </tr>
                    </ng-template>
                    <ng-template #emptymessage>
                        <tr>
                            <td colspan="2" class="text-center text-color-secondary">{{ 'WORKER_PROFILE.NO_SPECIALIZATIONS' | translate }}</td>
                        </tr>
                    </ng-template>
                </p-table>
            </div>

            <!-- ================= المستندات ================= -->
            <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
                <div class="flex flex-col gap-3">
                    <h3>{{ 'WORKER_PROFILE.SELFIE_TITLE' | translate }}</h3>
                    <img *ngIf="selfieUrl()" width="150" height="150" [src]="selfieUrl()" [attr.alt]="'WORKER_PROFILE.SELFIE_TITLE' | translate" class="w-8rem h-8rem border-round object-fit-cover" />
                    <p-fileupload
                        mode="basic"
                        [chooseLabel]="'WORKER_PROFILE.CHOOSE_SELFIE' | translate"
                        accept="image/*"
                        [maxFileSize]="5000000"
                        [auto]="false"
                        [customUpload]="true"
                        (onSelect)="onDocumentSelect($event, documentType.Selfie)"
                    ></p-fileupload>
                    <small class="text-color-secondary">{{ 'WORKER_PROFILE.UPLOAD_HINT' | translate }}</small>
                </div>

                <div class="flex flex-col gap-3">
                    <h3>{{ 'WORKER_PROFILE.PASSPORT_IMAGE_TITLE' | translate }}</h3>
                    <img *ngIf="passportUrl()" width="150" height="150" [src]="passportUrl()" [attr.alt]="'WORKER_PROFILE.PASSPORT_IMAGE_TITLE' | translate" class="w-8rem h-8rem border-round object-fit-cover" />
                    <p-fileupload
                        mode="basic"
                        [chooseLabel]="'WORKER_PROFILE.CHOOSE_PASSPORT_IMAGE' | translate"
                        accept="image/*"
                        [maxFileSize]="5000000"
                        [auto]="false"
                        [customUpload]="true"
                        (onSelect)="onDocumentSelect($event, documentType.Passport)"
                    ></p-fileupload>
                    <small class="text-color-secondary">{{ 'WORKER_PROFILE.UPLOAD_HINT' | translate }}</small>
                </div>
            </div>

            <div class="flex justify-content-end gap-2 mt-4">
                <p-button [label]="'COMMON.CANCEL' | translate" [outlined]="true" routerLink="/worker/dashboard" type="button"></p-button>
                <p-button [label]="'WORKER_PROFILE.SAVE_CHANGES' | translate" icon="pi pi-save" type="submit" [loading]="saving()" [disabled]="form.invalid || saving()"></p-button>
            </div>
        </form>
    `
})
export class WorkerProfileComponent implements OnInit {
    private fb = inject(FormBuilder);
    // private apiService = inject(ApiService);
    private workerService = inject(WorkerService);
    private globalizationSpecsService = inject(GlobalizationSpecsService);
    private authService = inject(AuthService);
    private messageService = inject(MessageService);
    private translate = inject(TranslateService);
    private router = inject(Router);

    readonly documentType = DocumentType;

    profile = signal<WorkerProfile | null>(null);
    loading = signal(true);
    saving = signal(false);

    countriesOptions = signal<any[]>([]);
    statesOptions = signal<any[]>([]);
    citiesOptions = signal<any[]>([]);

    // Bumped whenever the active language changes, so the computed()s below
    // (which read translate.instant()) re-run and pick up the new strings.
    private currentLang = signal<string>(
    this.translate.currentLang() ?? this.translate.getCurrentLang() ?? 'ar'
);

    specializationOptions = computed(() => {
        this.currentLang();
        return SPECIALIZATION_KEYS.map(o => ({
            value: o.value,
            label: this.translate.instant(`SPECIALIZATIONS.${o.key}`)
        }));
    });

    currencyOptions = computed(() => {
        this.currentLang();
        return CURRENCY_KEYS.map(o => ({
            value: o.value,
            label: this.translate.instant(`WORKER_PROFILE.${o.key}`)
        }));
    });

    userFullName = computed(() => this.authService.currentUser()?.fullName ?? '');    
    userEmail = computed(() => this.authService.currentUser()?.email ?? '');

    // Files are only staged locally here; they are sent together with the
    // rest of the form fields in a single multipart request on saveProfile().
    selfieFile = signal<File | null>(null);
    passportFile = signal<File | null>(null);
    private selfiePreview = signal<string | null>(null);
    private passportPreview = signal<string | null>(null);

    selfieUrl = computed(
        () => this.selfiePreview() ?? this.profile()?.documents?.find(d => d.type === DocumentType.Selfie)?.documentImageUrl ?? null
    );
    passportUrl = computed(
        () => this.passportPreview() ?? this.profile()?.documents?.find(d => d.type === DocumentType.Passport)?.documentImageUrl ?? null
    );

    verificationStatusLabel = computed(() => {
        this.currentLang();
        switch (this.profile()?.verificationStatus) {
            case VerificationStatus.Verified: return this.translate.instant('WORKER_PROFILE.VERIFICATION_VERIFIED');
            case VerificationStatus.Rejected: return this.translate.instant('WORKER_PROFILE.VERIFICATION_REJECTED');
            default: return this.translate.instant('WORKER_PROFILE.VERIFICATION_PENDING');
        }
    });

    verificationStatusSeverity = computed(() => {
        switch (this.profile()?.verificationStatus) {
            case VerificationStatus.Verified: return 'success';
            case VerificationStatus.Rejected: return 'danger';
            default: return 'warn';
        }
    });

    form: FormGroup = this.fb.group({
        nationalityId: [null, Validators.required],
        nationalIdNumber: ['', Validators.required],
        whatsAppNumber: ['' ,  Validators.required],
        passportNumber: [null],
        passportExpiryDate: [null],
        birthDate: [null, Validators.required],
        passportCountry: [null],
        email:[{value:null , disabled:true}],
        countryId: [null],
        stateId: [null],
        cityId: [null],
        address: [''],

        bio: [''],
        experienceYears: [0, [Validators.required, Validators.min(0)]],
        languages: [null],
        previousEmployer: [null],
        isLiveIn: [false],
        isAvailable: [true],

        hourlyRate: [null],
        monthlyRate: [null],
        currency: [0],

        specializations: this.fb.array([])
    });

    get specializationsArray(): FormArray {
        return this.form.get('specializations') as FormArray;
    }

    constructor() {
         this.translate.onLangChange.subscribe((event: LangChangeEvent) => {
        this.currentLang.set(event.lang);
    });
    }

    ngOnInit() {
        this.loadCountries();
        this.loadProfile();
    }

    loadCountries() {
        this.globalizationSpecsService.getCountries().subscribe({
            next: (data: any[]) => this.countriesOptions.set(data),
            error: () => this.messageService.add({
                severity: 'error',
                summary: this.translate.instant('COMMON.ERROR'),
                detail: this.translate.instant('WORKER_PROFILE.TOAST_LOAD_COUNTRIES_ERROR')
            })
        });
    }

    onCountryChange(countryId: number, resetChildren: boolean = true) {
        if (resetChildren) {
            this.form.patchValue({ stateId: null, cityId: null });
            this.citiesOptions.set([]);
        }
        this.statesOptions.set([]);
        if (!countryId) return;
        this.globalizationSpecsService.getStatesByCountryId(countryId).subscribe({
            next: (data: any[]) => this.statesOptions.set(data),
            error: () => this.messageService.add({
                severity: 'error',
                summary: this.translate.instant('COMMON.ERROR'),
                detail: this.translate.instant('WORKER_PROFILE.TOAST_LOAD_STATES_ERROR')
            })
        });
    }

    onStateChange(stateId: number, resetCity: boolean = true) {
        if (resetCity) {
            this.form.patchValue({ cityId: null });
        }
        this.citiesOptions.set([]);
        if (!stateId) return;
        this.globalizationSpecsService.getCitiesByStateId(stateId).subscribe({
            next: (data: any[]) => this.citiesOptions.set(data),
            error: () => this.messageService.add({
                severity: 'error',
                summary: this.translate.instant('COMMON.ERROR'),
                detail: this.translate.instant('WORKER_PROFILE.TOAST_LOAD_CITIES_ERROR')
            })
        });
    }

    loadProfile() {
        this.loading.set(true);
        this.workerService.getWorkerProfile().subscribe({
            next: (data: any) => {
                this.profile.set(data);
                this.patchForm(data);
                if (data.countryId) {
                    this.onCountryChange(data.countryId, false);
                }
                if (data.stateId) {
                    this.onStateChange(data.stateId, false);
                }
                this.loading.set(false);
            },
            error: () => {
                this.messageService.add({
                    severity: 'error',
                    summary: this.translate.instant('COMMON.ERROR'),
                    detail: this.translate.instant('WORKER_PROFILE.TOAST_LOAD_PROFILE_ERROR')
                });
                this.loading.set(false);
            }
        });
    }

    private patchForm(data: any) {
        this.form.patchValue({
            nationalityId: data.nationalityId,
            nationalIdNumber: data.nationalIdNumber,
            birthDate: data.birthDate ? new Date(data.birthDate) : null,
            whatsAppNumber: data.whatsAppNumber,
            passportNumber: data.passportNumber,
            passportExpiryDate: data.passportExpiryDate ? new Date(data.passportExpiryDate) : null,
            passportCountry: data.passportCountry,
            email:data.email,
            countryId: data.countryId,
            stateId: data.stateId,
            cityId: data.cityId,
            address: data.address,

            bio: data.bio,
            experienceYears: data.experienceYears,
            languages: data.languages,
            previousEmployer: data.previousEmployer,
            isLiveIn: data.isLiveIn,
            isAvailable: data.isAvailable,

            hourlyRate: data.hourlyRate,
            monthlyRate: data.monthlyRate,
            currency: data.currency
        });

        this.specializationsArray.clear();
        (data.workerSpecializationSpecs ?? []).forEach((spec:WorkerSpecializationSpec) => {
            this.specializationsArray.push(
                this.fb.group({ specialization: [spec.workerSpecialization, Validators.required] })
            );
        });
    }

    addSpecializationRow() {
        this.specializationsArray.push(this.fb.group({ specialization: [null, Validators.required] }));
    }

    removeSpecializationRow(index: number) {
        this.specializationsArray.removeAt(index);
    }

    /**
     * Formats a Date as "yyyy-MM-dd" using its LOCAL year/month/day.
     * Sending the raw Date object instead would go through JSON.stringify ->
     * Date.toISOString(), which converts to UTC and shifts the date back a
     * day in any timezone ahead of UTC (e.g. Egypt, UTC+2/+3).
     */
    private toDateOnlyString(date: Date | null): string | null {
        if (!date) return null;
        const year = date.getFullYear();
        const month = String(date.getMonth() + 1).padStart(2, '0');
        const day = String(date.getDate()).padStart(2, '0');
        return `${year}-${month}-${day}`;
    }

    onDocumentSelect(event: FileSelectEvent, type: DocumentType) {
        const file = event.files?.[0];
        if (!file) return;

        const previewUrl = URL.createObjectURL(file);

        if (type === this.documentType.Selfie) {
            this.selfieFile.set(file);
            this.selfiePreview.set(previewUrl);
        } else if (type === this.documentType.Passport) {
            this.passportFile.set(file);
            this.passportPreview.set(previewUrl);
        }
    }

    saveProfile() {
        if (this.form.invalid) {
            this.form.markAllAsTouched();
            return;
        }

        const value = this.form.value;
        const payload: Partial<any> = {
            ...this.profile(),
            nationalityId: value.nationalityId,            
            nationalIdNumber: value.nationalIdNumber,
            whatsAppNumber: value.whatsAppNumber,
            passportNumber: value.passportNumber,
            passportExpiryDate: this.toDateOnlyString(value.passportExpiryDate),
            birthDate: this.toDateOnlyString(value.birthDate),
            passportCountry: value.passportCountry,
           
            countryId: value.countryId,
            stateId: value.stateId,
            cityId: value.cityId,
            address: value.address,

            bio: value.bio,
            experienceYears: value.experienceYears,
            languages: value.languages,
            previousEmployer: value.previousEmployer,
            isLiveIn: value.isLiveIn,
            isAvailable: value.isAvailable,

            hourlyRate: value.hourlyRate,
            monthlyRate: value.monthlyRate,
            currency: value.currency,

            workerSpecializationSpecs: value.specializations
                .filter((s: any) => s.specialization !== null && s.specialization !== undefined)
                .map((s: any) => ({ workerSpecialization: s.specialization }))
        };

        // Single multipart request: the profile JSON payload plus both
        // document files (only the ones the user actually selected) travel
        // together in one FormData, in one HTTP call.
        const formData = new FormData();
        formData.append('profile', JSON.stringify(payload));

        const selfie = this.selfieFile();
        if (selfie) {
            formData.append('selfieImage', selfie, selfie.name);
        }

        const passport = this.passportFile();
        if (passport) {
            formData.append('passportImage', passport, passport.name);
        }

        this.saving.set(true);
        this.workerService.updateWorkerProfile(formData).subscribe({
            next: () => {
                this.saving.set(false);
                this.messageService.add({
                    severity: 'success',
                    summary: this.translate.instant('COMMON.SUCCESS'),
                    detail: this.translate.instant('WORKER_PROFILE.TOAST_SAVE_SUCCESS')
                });
                setTimeout(() => this.router.navigate(['/worker/dashboard']), 1500);
            },
            error: () => {
                this.saving.set(false);
                this.messageService.add({
                    severity: 'error',
                    summary: this.translate.instant('COMMON.ERROR'),
                    detail: this.translate.instant('WORKER_PROFILE.TOAST_SAVE_ERROR')
                });
            }
        });
    }
}