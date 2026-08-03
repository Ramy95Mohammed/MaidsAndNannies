import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { PasswordModule } from 'primeng/password';
import { TextareaModule } from 'primeng/textarea';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { ApiService } from '../../../core/services/api.service';

@Component({
    selector: 'app-admin-register-homeowner',
    standalone: true,
    imports: [CommonModule, ReactiveFormsModule, CardModule, ButtonModule, InputTextModule, PasswordModule, TextareaModule, ToastModule, TranslatePipe],
    providers: [MessageService],
    template: `
        <p-toast />
        <div class="card">
            <h2>{{ 'SETTINGS.REGISTER_HOMEOWNER' | translate }}</h2>
            <form [formGroup]="form" (ngSubmit)="register()" class="grid grid-cols-1 md:grid-cols-2 gap-4 mt-4">
                <div class="flex flex-col gap-2">
                    <label class="font-bold">{{ 'AUTH.FULL_NAME' | translate }}</label>
                    <input pInputText formControlName="fullName" class="w-full" />
                </div>
                <div class="flex flex-col gap-2">
                    <label class="font-bold">{{ 'AUTH.EMAIL' | translate }}</label>
                    <input pInputText formControlName="email" type="email" class="w-full" />
                </div>
                <div class="flex flex-col gap-2">
                    <label class="font-bold">{{ 'AUTH.PHONE' | translate }}</label>
                    <input pInputText formControlName="phoneNumber" class="w-full" />
                </div>
                <div class="flex flex-col gap-2">
                    <label class="font-bold">{{ 'AUTH.PASSWORD' | translate }}</label>
                    <p-password formControlName="password" [toggleMask]="true" styleClass="w-full" [fluid]="true"></p-password>
                </div>
                <div class="flex flex-col gap-2">
                    <label class="font-bold">{{ 'HOMEOWNER.NATIONAL_ID' | translate }}</label>
                    <input pInputText formControlName="nationalIdNumber" class="w-full" />
                </div>
                <div class="flex flex-col gap-2">
                    <label class="font-bold">{{ 'HOMEOWNER.CITY' | translate }}</label>
                    <input pInputText formControlName="city" class="w-full" />
                </div>
                <div class="flex flex-col gap-2 md:col-span-2">
                    <label class="font-bold">{{ 'HOMEOWNER.ADDRESS' | translate }}</label>
                    <textarea pTextarea formControlName="address" rows="2" class="w-full"></textarea>
                </div>
                <div class="md:col-span-2 text-center mt-4">
                    <p-button [label]="'SETTINGS.REGISTER_HOMEOWNER' | translate" icon="pi pi-user-plus" type="submit" [loading]="loading"></p-button>
                </div>
            </form>
        </div>
    `
})
export class AdminRegisterHomeowner {
    private fb = inject(FormBuilder);
    private apiService = inject(ApiService);
    private messageService = inject(MessageService);
    private translate = inject(TranslateService);

    loading = false;

    form: FormGroup = this.fb.group({
        fullName: ['', Validators.required],
        email: ['', [Validators.required, Validators.email]],
        phoneNumber: ['', Validators.required],
        password: ['', [Validators.required, Validators.minLength(6)]],
        nationalIdNumber: ['', Validators.required],
        city: ['', Validators.required],
        address: ['', Validators.required]
    });

    register() {
        if (this.form.invalid) { this.form.markAllAsTouched(); return; }
        this.loading = true;
        this.apiService.adminRegisterHomeowner(this.form.value).subscribe({
            next: () => {
                this.messageService.add({ severity: 'success', detail: this.translate.instant('ADMIN.HOMEOWNER_REGISTERED') });
                this.loading = false;
                this.form.reset();
            },
            error: () => {
                this.messageService.add({ severity: 'error', detail: this.translate.instant('ADMIN.REGISTER_FAILED') });
                this.loading = false;
            }
        });
    }
}