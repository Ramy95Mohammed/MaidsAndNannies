import { Component, inject, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { PasswordModule } from 'primeng/password';
import { SelectModule } from 'primeng/select';
import { RadioButtonModule } from 'primeng/radiobutton';
import { TextareaModule } from 'primeng/textarea';
import { MessageModule } from 'primeng/message';
import { StepperModule } from 'primeng/stepper';
import { translate, TranslatePipe } from '@ngx-translate/core';
import { AppFloatingConfigurator } from '../../layout/component/app.floatingconfigurator';
import { AuthService } from '../../core/services/auth.service';
import { LanguageService } from '../../core/services/language.service';
import { GlobalizationSpecsService } from '@/core/services/globalization-specs.service';
import { Country } from '@/core/interfaces/country';
import { State } from '@/core/interfaces/state';
import { Badge } from 'primeng/badge';
import { MessageService } from 'primeng/api';
import { PrimeNG } from 'primeng/config';
import { FileUpload } from 'primeng/fileupload';
import { ProgressBar } from 'primeng/progressbar';
import { Toast } from 'primeng/toast';

@Component({
    selector: 'app-register',
    standalone: true,
    imports: [
        CommonModule,
        FormsModule,
        ButtonModule,
        InputTextModule,
        PasswordModule,
        SelectModule,
        RadioButtonModule,
        TextareaModule,
        MessageModule,
        StepperModule,
        TranslatePipe,
        RouterModule,
        AppFloatingConfigurator,        
        FileUpload,        
        Toast
    ],
    template: `
        <app-floating-configurator />
        <div class="bg-surface-50 dark:bg-surface-950 flex items-center justify-center min-h-screen min-w-screen overflow-hidden">
            <div class="flex flex-col items-center justify-center w-full max-w-4xl px-4">
                <div style="border-radius: 56px; padding: 0.3rem; background: linear-gradient(180deg, var(--primary-color) 10%, rgba(33, 150, 243, 0) 30%)">
                    <div class="w-full bg-surface-0 dark:bg-surface-900 py-12 px-8 sm:px-16" style="border-radius: 53px">
                        <div class="text-center mb-8">
                            <div class="text-surface-900 dark:text-surface-0 text-3xl font-medium mb-4">{{ 'AUTH.REGISTER' | translate }}</div>
                        </div>

                        <div *ngIf="errorMessage" class="mb-4">
                            <p-message severity="error" [text]="errorMessage"></p-message>
                        </div>
                        <div *ngIf="successMessage" class="mb-4">
                            <p-message severity="success" [text]="successMessage"></p-message>
                        </div>

                        <!-- Step 1: Choose Role -->
                        <div *ngIf="step === 1" class="flex flex-col gap-6">
                            <div class="grid grid-cols-1 md:grid-cols-1 gap-4 ">
                                <div class="border-2 p-6 rounded-lg cursor-pointer text-center" [class.border-primary]="accountType === 1" [class.border-gray-300]="accountType !== 1" (click)="accountType = 1">
                                    <i class="pi pi-user text-4xl mb-4 text-primary"></i>
                                    <h4>{{ 'AUTH.REGISTER_AS_WORKER' | translate }}</h4>
                                    <p class="text-muted-color text-sm">اعثري عمل في المنازل</p>
                                </div>
                            </div>
                            <p-button [label]="'COMMON.NEXT' | translate" icon="pi pi-arrow-right" styleClass="w-full" (onClick)="step = 2" [disabled]="!accountType"></p-button>
                        </div>

                        <!-- Step 2: Basic Info -->
                        <div *ngIf="step === 2" class="flex flex-col gap-4">
                            <h5>{{ 'AUTH.BASIC_INFO' | translate }}</h5>
                            <div>
                                <label class="block font-bold mb-2">{{ 'AUTH.FULL_NAME' | translate }}</label>
                                <input pInputText [(ngModel)]="formData.fullName" class="w-full" />
                            </div>
                            <div>
                                <label class="block font-bold mb-2">{{ 'AUTH.EMAIL' | translate }}</label>
                                <input pInputText [(ngModel)]="formData.email" type="email" class="w-full" />
                            </div>
                            <div>
                                <label class="block font-bold mb-2">{{ 'AUTH.PHONE' | translate }}</label>
                                <input pInputText [(ngModel)]="formData.phoneNumber" class="w-full" />
                            </div>
                            <div>
                                <label class="block font-bold mb-2">{{ 'AUTH.PASSWORD' | translate }}</label>
                                <p-password [(ngModel)]="formData.password" [toggleMask]="true" styleClass="w-full" [fluid]="true"></p-password>
                            </div>
                            <div>
                                <label class="block font-bold mb-2">{{ 'AUTH.CONFIRM_PASSWORD' | translate }}</label>
                                <p-password [(ngModel)]="formData.confirmPassword" [toggleMask]="true" styleClass="w-full" [fluid]="true"></p-password>
                            </div>
                            <div class="flex gap-2">
                                <p-button [label]="'COMMON.NEXT' | translate" icon="pi pi-arrow-right" styleClass="flex-1" (onClick)="step = 3"></p-button>
                                <p-button [label]="'COMMON.PREVIOUS' | translate" icon="pi pi-arrow-left" severity="secondary" styleClass="flex-1" (onClick)="step = 1"></p-button>
                            </div>
                        </div>

                        <!-- Step 3: Profile Info -->
                        <div *ngIf="step === 3" class="flex flex-col gap-4">
                            <ng-container *ngIf="accountType === 1">
                                <h5>{{ 'AUTH.HOME_WORKER_INFO' | translate }}</h5>
                                <div>
                                    <label class="block font-bold mb-2">{{ 'WORKER.NATIONALITY' | translate }}</label>
                                    <p-select
                                        (onChange)="getStateByCountryId($event.value)"
                                        [options]="countriesOptions"
                                        [filter]="true"
                                        [filterFields]="['nationality', 'native']"
                                        [(ngModel)]="formData.nationalityId"
                                        optionValue="id"
                                        optionLabel="name"
                                        [placeholder]="'COMMON.COUNTRY' | translate"
                                        class="w-full"
                                    >
                                        <ng-template #selectedItem let-selectedOption>
                                            @if (selectedOption) {
                                                <div class="flex items-center gap-3">
                                                    <div>{{ selectedOption.nationality }}</div>
                                                    <div>{{ selectedOption.native }}</div>
                                                </div>
                                            }
                                        </ng-template>
                                        <ng-template let-country #item>
                                            <div class="flex items-center gap-3">
                                                <div>{{ country.nationality }}</div>
                                                <div>{{ country.native }}</div>
                                            </div>
                                        </ng-template>
                                        <ng-template #dropdownicon>
                                            <i class="pi pi-map"></i>
                                        </ng-template>
                                    </p-select>
                                </div>

                                <div>
                                    <label class="block font-bold mb-2">{{ 'COMMON.STATE' | translate }}</label>
                                    <p-select [options]="statesOptions" [filter]="true" [filterBy]="'name'" [(ngModel)]="formData.stateId" optionValue="id" optionLabel="name" [placeholder]="'COMMON.STATE' | translate" class="w-full">
                                        <ng-template #selectedItem let-selectedOption>
                                            @if (selectedOption) {
                                                <div class="flex items-center gap-3">
                                                    <div>{{ selectedOption.name }}</div>
                                                </div>
                                            }
                                        </ng-template>
                                        <ng-template let-state #item>
                                            <div class="flex items-center gap-3">
                                                <div>{{ state.name }}</div>
                                            </div>
                                        </ng-template>
                                        <ng-template #dropdownicon>
                                            <i class="pi pi-map"></i>
                                        </ng-template>
                                    </p-select>
                                </div>

                                <div class="grid grid-cols-2 gap-4">
                                    <div>
                                        <label class="block font-bold mb-2">{{ 'WORKER.EXPERIENCE' | translate }}</label>
                                        <input pInputText [(ngModel)]="formData.experienceYears" type="number" class="w-full" />
                                    </div>
                                    <div>
                                        <label class="block font-bold mb-2">{{ 'WORKER.MONTHLY_RATE' | translate }}</label>
                                        <input pInputText [(ngModel)]="formData.monthlyRate" type="number" class="w-full" />
                                    </div>
                                </div>

                                <div>
                                    <label class="block font-bold mb-2">{{ 'WORKER.BIO' | translate }}</label>
                                    <textarea pInputTextarea [(ngModel)]="formData.bio" rows="3" class="w-full"></textarea>
                                </div>
                            </ng-container>

                            <div class="flex gap-2">
                                <p-button [label]="'COMMON.NEXT' | translate" icon="pi pi-arrow-right" styleClass="flex-1" (onClick)="step = 4"></p-button>
                                <p-button [label]="'COMMON.PREVIOUS' | translate" icon="pi pi-arrow-left" severity="secondary" styleClass="flex-1" (onClick)="step = 2"></p-button>
                            </div>
                        </div>

                        <div *ngIf="step === 4" class="flex flex-col gap-4">
                            <ng-container *ngIf="accountType === 1">
                                <h5>{{ 'AUTH.HOME_WORKER_IMAGE' | translate }}</h5>

                                <p-toast />
                                <p-fileupload name="selfieImage" mode="basic" accept="image/*" maxFileSize="1000000" [auto]="false" chooseLabel="اختر صورة شخصية" (onSelect)="onSelfieSelected($event)"> </p-fileupload>

                                <div *ngIf="selfieImageFile" class="flex flex-col items-center gap-2 mt-4">
                                    <img [src]="selfiePreviewUrl" alt="selfie" width="120" height="120" style="object-fit:cover;border-radius:8px;" />
                                    <span class="text-sm">{{ selfieImageFile.name }}</span>
                                </div>
                               
                            </ng-container>

                            <div class="flex gap-2">
                                <p-button [label]="'COMMON.PREVIOUS' | translate" icon="pi pi-arrow-left" severity="secondary" styleClass="flex-1" (onClick)="step = 3"></p-button>
                                <p-button [label]="'AUTH.REGISTER' | translate" icon="pi pi-check" styleClass="flex-1" (onClick)="register()" [loading]="isLoading"></p-button>
                            </div>
                        </div>

                        <div class="text-center mt-6">
                            <span class="text-muted-color">{{ 'AUTH.HAS_ACCOUNT' | translate }}</span>
                            <a routerLink="/auth/login" class="font-medium text-primary ml-2 cursor-pointer">{{ 'AUTH.LOGIN' | translate }}</a>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    `
})
export class Register implements OnInit {
    step: number = 1;
    accountType: 0 | 1 = 1;
    isLoading: boolean = false;
    errorMessage: string = '';
    successMessage: string = '';
    countriesOptions!: Country[];
    statesOptions!: State[];

    //file

    selfiePreviewUrl: string | null = null;

    private authService = inject(AuthService);
    langService = inject(LanguageService);
    private globalizationSpecsService = inject(GlobalizationSpecsService);
    private router = inject(Router);

    selfieImageFile: File | null = null;

    formData: any = {
        fullName: '',
        email: '',
        phoneNumber: '',
        password: '',
        confirmPassword: '',
        nationalityId: null,
        countryId: null,        
        stateId: null,
        experienceYears: 0,
        monthlyRate: 0,
        bio: ''
    };

    register() {
        if (this.formData.password !== this.formData.confirmPassword) {
            this.errorMessage = 'كلمتا المرور غير متطابقتين';
            return;
        }
        
        if (!this.selfieImageFile) {
            this.errorMessage = 'الرجاء رفع الصورة الشخصية';
            return;
        }

        this.isLoading = true;
        this.errorMessage = '';

        const fd = new FormData();
        fd.append('fullName', this.formData.fullName);
        fd.append('email', this.formData.email);
        fd.append('phoneNumber', this.formData.phoneNumber);
        fd.append('password', this.formData.password);
        fd.append('confirmPassword', this.formData.confirmPassword);
        fd.append('nationalityId', this.formData.nationalityId);
        fd.append('countryId', this.formData.nationalityId);
        fd.append('stateId', this.formData.stateId);
        fd.append('experienceYears', this.formData.experienceYears);
        fd.append('monthlyRate', this.formData.monthlyRate);
        fd.append('bio', this.formData.bio ?? '');

        fd.append('selfieImage', this.selfieImageFile);

        this.authService.registerWorker(fd).subscribe({
            next: (response) => {
                this.isLoading = false;
                this.successMessage = response.message;
                setTimeout(() => this.router.navigate(['/auth/login']), 4000);
            },
            error: (error) => {
                this.isLoading = false;
                this.errorMessage = error.error?.message || 'فشل التسجيل';
            }
        });
    }

    ngOnInit(): void {
        this.getCountries();
    }

    getCountries() {
        this.globalizationSpecsService.getCountries().subscribe({
            next: (response) => {
                this.countriesOptions = response;
            },
            error: (error) => {
                this.errorMessage = error.error?.message || 'Get globalization data failed';
            }
        });
    }

    getStateByCountryId(countryId: number | null) {
        if (countryId == null) return;
        this.globalizationSpecsService.getStatesByCountryId(countryId).subscribe({
            next: (response) => {
                this.statesOptions = response;
            },
            error: (error) => {
                this.errorMessage = error.error?.message || 'Get globalization data failed';
            }
        });
    }

    //file
    onSelfieSelected(event: any) {
        const file = event.currentFiles?.[0];
        if (!file) return;
        this.selfieImageFile = file;
        this.selfiePreviewUrl = URL.createObjectURL(file);
    }
}
