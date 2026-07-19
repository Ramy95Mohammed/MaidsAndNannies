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
import { TranslatePipe } from '@ngx-translate/core';
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

const SPECIALIZATION_OPTIONS = [
    { label: 'تنظيف', value: Specialization.Cleaning },
    { label: 'طهي', value: Specialization.Cooking },
    { label: 'رعاية أطفال', value: Specialization.Childcare },
    { label: 'رعاية مسنين', value: Specialization.ElderlyCare },
    { label: 'أعمال منزلية عامة', value: Specialization.GeneralHousekeeping }
];

// NOTE: no currency enum was provided in the domain snippets shared -
// adjust this list to match the real backend enum once available.
const CURRENCY_OPTIONS = [
    { label: 'جنيه مصري (EGP)', value: 0 },
    { label: 'دولار أمريكي (USD)', value: 1 },
    { label: 'ريال سعودي (SAR)', value: 2 }
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
            <h2>ملفي الشخصي</h2>

            <!-- ================= البيانات الأساسية ================= -->
            <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
                <div class="flex flex-col gap-4">
                    <h3>البيانات الأساسية</h3>

                    <div class="flex flex-col gap-2">
                        <label class="font-bold">الاسم الكامل</label>
                        <input pInputText [value]="userFullName()" class="w-full" disabled />
                    </div>

                    <div class="flex flex-col gap-2">
                        <label class="font-bold">البريد الإلكتروني</label>
                        <input pInputText  formControlName="email"  class="w-full"  />
                    </div>

                    <div class="flex flex-col gap-2">
                        <label class="font-bold">الجنسية</label>
                        <p-select
                            formControlName="nationalityId"
                            [options]="countriesOptions()"
                            [filter]="true"
                            [filterFields]="['nationality', 'native']"
                            optionValue="id"
                            optionLabel="nationality"
                            [placeholder]="'COMMON.NATIONALITY' | translate"
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
                        <label class="font-bold">رقم الهوية الوطنية</label>
                        <input pInputText formControlName="nationalIdNumber" class="w-full" />
                    </div>

                    <div class="grid grid-cols-2 gap-4">
                        <div class="flex flex-col gap-2">
                            <label class="font-bold">رقم جواز السفر</label>
                            <input pInputText formControlName="passportNumber" class="w-full" />
                        </div>
                        <div class="flex flex-col gap-2">
                            <label class="font-bold">تاريخ انتهاء الجواز</label>
                            <p-datepicker formControlName="passportExpiryDate" dateFormat="dd/mm/yy" [showIcon]="true" class="w-full"></p-datepicker>
                        </div>
                    </div>

                    <div class="flex flex-col gap-2">
                        <label class="font-bold">بلد إصدار الجواز</label>
                        <input pInputText formControlName="passportCountry" class="w-full" />
                    </div>
                </div>

                <!-- ================= الموقع ================= -->
                <div class="flex flex-col gap-4">
                    <h3>الموقع</h3>

                    <div class="flex flex-col gap-2">
                        <label class="font-bold">الدولة</label>
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
                        <label class="font-bold">المنطقة / الولاية</label>
                        <p-select
                            formControlName="stateId"
                            [options]="statesOptions()"
                            [filter]="true"
                            optionValue="id"
                            optionLabel="name"
                            (onChange)="onStateChange($event.value)"                            
                            placeholder="اختر المنطقة"
                            class="w-full"
                        ></p-select>
                    </div>

                    <div class="flex flex-col gap-2">
                        <label class="font-bold">المدينة</label>
                        <p-select
                            formControlName="cityId"
                            [options]="citiesOptions()"
                            [filter]="true"
                            optionValue="id"
                            optionLabel="name"
                            [disabled]="!form.value.stateId"
                            placeholder="اختر المدينة"
                            class="w-full"
                        ></p-select>
                    </div>

                    <div class="flex flex-col gap-2">
                        <label class="font-bold">العنوان بالتفصيل</label>
                        <textarea pTextarea formControlName="address" rows="3" class="w-full"></textarea>
                    </div>
                </div>
            </div>

            <!-- ================= المعلومات المهنية ================= -->
            <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
                <div class="flex flex-col gap-4">
                    <h3>المعلومات المهنية</h3>

                    <div class="flex flex-col gap-2">
                        <label class="font-bold">نبذة عني</label>
                        <textarea pTextarea formControlName="bio" rows="4" class="w-full" placeholder="اكتب وصفاً عن خبراتك..."></textarea>
                    </div>

                    <div class="grid grid-cols-2 gap-4">
                        <div class="flex flex-col gap-2">
                            <label class="font-bold">سنوات الخبرة</label>
                            <p-inputnumber formControlName="experienceYears" [min]="0" class="w-full"></p-inputnumber>
                        </div>
                        <div class="flex flex-col gap-2">
                            <label class="font-bold">اللغات (مفصولة بفاصلة)</label>
                            <input pInputText formControlName="languages" class="w-full" placeholder="عربي، إنجليزي" />
                        </div>
                    </div>

                    <div class="flex flex-col gap-2">
                        <label class="font-bold">جهة العمل السابقة</label>
                        <input pInputText formControlName="previousEmployer" class="w-full" />
                    </div>

                    <div class="flex align-items-center gap-2">
                        <p-checkbox formControlName="isLiveIn" [binary]="true"></p-checkbox>
                        <label>أعمل مقيمة</label>
                    </div>

                    <div class="flex align-items-center gap-2">
                        <p-checkbox formControlName="isAvailable" [binary]="true"></p-checkbox>
                        <label>متاحة للعمل حالياً</label>
                    </div>
                </div>

                <!-- ================= المعلومات المالية ================= -->
                <div class="flex flex-col gap-4">
                    <h3>المعلومات المالية</h3>

                    <div class="flex flex-col gap-2">
                        <label class="font-bold">الأجر الشهري</label>
                        <p-inputnumber formControlName="monthlyRate" mode="decimal" [minFractionDigits]="0" [min]="0" class="w-full"></p-inputnumber>
                    </div>

                    <div class="flex flex-col gap-2">
                        <label class="font-bold">أجر الساعة</label>
                        <p-inputnumber formControlName="hourlyRate" mode="decimal" [minFractionDigits]="0" [min]="0" class="w-full"></p-inputnumber>
                    </div>

                    <div class="flex flex-col gap-2">
                        <label class="font-bold">العملة</label>
                        <p-select formControlName="currency" [options]="currencyOptions" optionLabel="label" optionValue="value" class="w-full"></p-select>
                    </div>

                    <h3>حالة الحساب</h3>
                    <div class="flex align-items-center gap-3 flex-wrap">
                        <p-tag
                            [value]="verificationStatusLabel()"
                            [severity]="verificationStatusSeverity()"
                        ></p-tag>
                        <span>التقييم: {{ profile()?.averageRating ?? 0 }} ({{ profile()?.totalReviews ?? 0 }} تقييم)</span>
                    </div>
                </div>
            </div>

            <!-- ================= التخصصات ================= -->
            <div>
                <div class="flex justify-content-between align-items-center mb-3">
                    <h3 class="m-0">التخصصات</h3>
                    <p-button label="إضافة تخصص" icon="pi pi-plus" size="small" [outlined]="true" (onClick)="addSpecializationRow()"></p-button>
                </div>

                <p-table [value]="specializationsArray.controls" dataKey="index">
                    <ng-template #header>
                        <tr>
                            <th>التخصص</th>
                            <th style="width: 6rem">حذف</th>
                        </tr>
                    </ng-template>
                    <ng-template #body let-row let-i="rowIndex">
                        <tr [formGroup]="row">
                            <td>
                                <p-select
                                    formControlName="specialization"
                                    [options]="specializationOptions"
                                    optionLabel="label"
                                    optionValue="value"
                                    placeholder="اختر تخصصاً"
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
                            <td colspan="2" class="text-center text-color-secondary">لا يوجد تخصصات مضافة بعد</td>
                        </tr>
                    </ng-template>
                </p-table>
            </div>

            <!-- ================= المستندات ================= -->
            <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
                <div class="flex flex-col gap-3">
                    <h3>الصورة الشخصية</h3>
                    <img *ngIf="selfieUrl()" width="150" height="150" [src]="selfieUrl()" alt="الصورة الشخصية" class="w-8rem h-8rem border-round object-fit-cover" />
                    <p-fileupload
                        mode="basic"
                        chooseLabel="اختيار الصورة الشخصية"
                        accept="image/*"
                        [maxFileSize]="5000000"
                        [auto]="false"
                        [customUpload]="true"
                        (onSelect)="onDocumentSelect($event, documentType.Selfie)"
                    ></p-fileupload>
                    <small class="text-color-secondary">سيتم رفع الصورة مع باقي بيانات الملف الشخصي عند الضغط على "حفظ التعديلات"</small>
                </div>

                <div class="flex flex-col gap-3">
                    <h3>صورة جواز السفر</h3>
                    <img *ngIf="passportUrl()" width="150" height="150" [src]="passportUrl()" alt="صورة جواز السفر" class="w-8rem h-8rem border-round object-fit-cover" />
                    <p-fileupload
                        mode="basic"
                        chooseLabel="اختيار صورة الجواز"
                        accept="image/*"
                        [maxFileSize]="5000000"
                        [auto]="false"
                        [customUpload]="true"
                        (onSelect)="onDocumentSelect($event, documentType.Passport)"
                    ></p-fileupload>
                    <small class="text-color-secondary">سيتم رفع الصورة مع باقي بيانات الملف الشخصي عند الضغط على "حفظ التعديلات"</small>
                </div>
            </div>

            <div class="flex justify-content-end gap-2 mt-4">
                <p-button label="إلغاء" [outlined]="true" routerLink="/worker/dashboard" type="button"></p-button>
                <p-button label="حفظ التعديلات" icon="pi pi-save" type="submit" [loading]="saving()" [disabled]="form.invalid || saving()"></p-button>
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
    private router = inject(Router);

    readonly documentType = DocumentType;
    readonly specializationOptions = SPECIALIZATION_OPTIONS;
    readonly currencyOptions = CURRENCY_OPTIONS;

    profile = signal<WorkerProfile | null>(null);
    loading = signal(true);
    saving = signal(false);

    countriesOptions = signal<any[]>([]);
    statesOptions = signal<any[]>([]);
    citiesOptions = signal<any[]>([]);

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
        switch (this.profile()?.verificationStatus) {
            case VerificationStatus.Verified: return 'موثق';
            case VerificationStatus.Rejected: return 'مرفوض';
            default: return 'قيد المراجعة';
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
        passportNumber: [null],
        passportExpiryDate: [null],
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

    ngOnInit() {
        this.loadCountries();
        this.loadProfile();
    }

    loadCountries() {
        this.globalizationSpecsService.getCountries().subscribe({
            next: (data: any[]) => this.countriesOptions.set(data),
            error: () => this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل تحميل قائمة الدول' })
        });
    }

    onCountryChange(countryId: number) {
        this.form.patchValue({ stateId: null, cityId: null });
        this.statesOptions.set([]);
        this.citiesOptions.set([]);
        if (!countryId) return;
        this.globalizationSpecsService.getStatesByCountryId(countryId).subscribe({
            next: (data: any[]) => this.statesOptions.set(data),
            error: () => this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل تحميل قائمة المناطق' })
        });
    }

    onStateChange(stateId: number) {
        this.form.patchValue({ cityId: null });
        this.citiesOptions.set([]);
        if (!stateId) return;
        this.globalizationSpecsService.getCitiesByStateId(stateId).subscribe({
            next: (data: any[]) => this.citiesOptions.set(data),
            error: () => this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل تحميل قائمة المدن' })
        });
    }

    loadProfile() {
        this.loading.set(true);
        this.workerService.getWorkerProfile().subscribe({
            next: (data: any) => {
                this.profile.set(data);
                this.patchForm(data);
                if (data.countryId) {
                    this.onCountryChange(data.countryId);
                }
                this.loading.set(false);
            },
            error: () => {
                this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل تحميل الملف الشخصي' });
                this.loading.set(false);
            }
        });
    }

    private patchForm(data: any) {
        console.log(data);
        this.form.patchValue({
            nationalityId: data.nationalityId,
            nationalIdNumber: data.nationalIdNumber,
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
            passportNumber: value.passportNumber,
            passportExpiryDate: value.passportExpiryDate,
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
                this.messageService.add({ severity: 'success', summary: 'تم', detail: 'تم حفظ التعديلات' });
                setTimeout(() => this.router.navigate(['/worker/dashboard']), 1500);
            },
            error: () => {
                this.saving.set(false);
                this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل حفظ التعديلات' });
            }
        });
    }
}