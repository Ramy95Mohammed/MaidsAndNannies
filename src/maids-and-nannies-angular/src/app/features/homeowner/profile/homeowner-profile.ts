import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { FileUpload } from 'primeng/fileupload';
import { Toast } from 'primeng/toast';
import { MessageModule } from 'primeng/message';
import { TranslatePipe } from '@ngx-translate/core';
import { MessageService } from 'primeng/api';
import { HomeownerService, HomeownerProfile } from '../../../core/services/homeowner.service';

@Component({
    selector: 'app-homeowner-profile',
    standalone: true,
    imports: [CommonModule, FormsModule, ButtonModule, InputTextModule, FileUpload, Toast, MessageModule, TranslatePipe],
    providers: [MessageService],
    template: `
        <div class="card">
            <p-toast />
            <div class="text-center mb-6">
                <h2>{{ 'HOMEOWNER.PROFILE' | translate }}</h2>
            </div>

            <div *ngIf="errorMessage" class="mb-4">
                <p-message severity="error" [text]="errorMessage"></p-message>
            </div>

            <div class="grid grid-cols-12 gap-6">
                <!-- Basic Info (read-only) -->
                <div class="col-span-12 md:col-span-6">
                    <div class="p-4 border-round border-1 border-surface-200">
                        <h4 class="mb-4">{{ 'HOMEOWNER.BASIC_INFO' | translate }}</h4>
                        <div class="mb-3">
                            <label class="block font-bold mb-1">
                                {{ 'AUTH.FULL_NAME' | translate }}
                                <span class="text-red-500 mx-2">*</span>
                            </label>
                            <input pInputText class="w-full" [(ngModel)]="formData.fullName"   />
                        </div>
                        <div class="mb-3">
                            <label class="block font-bold mb-1">{{ 'AUTH.EMAIL' | translate }}
                                <span class="text-red-500 mx-2">*</span>
                            </label>
                            <input pInputText class="w-full" [value]="profile?.email || ''" disabled />
                        </div>
                        <div class="mb-3">
                            <label class="block font-bold mb-1">{{ 'AUTH.PHONE' | translate }}
                                <span class="text-red-500 mx-2">*</span>
                            </label>
                            <input pInputText class="w-full" [(ngModel)]="formData.phoneNumber"  />
                        </div>

                         <div class="mb-3">
                            <label class="block font-bold mb-1">{{ 'COMMON.WHATSAPP_NUMBER' | translate }}
                                <span class="text-red-500 mx-2">*</span>
                            </label>
                            <input pInputText class="w-full" [(ngModel)]="formData.whatsAppNumber"  />
                        </div>
                    </div>
                </div>

                <!-- Address Info (editable) -->
                <div class="col-span-12 md:col-span-6">
                    <div class="p-4 border-round border-1 border-surface-200">
                        <h4 class="mb-4">{{ 'HOMEOWNER.ADDRESS_INFO' | translate }}</h4>
                         <div class="mb-3">
                            <label class="block font-bold mb-1">{{ 'HOMEOWNER.STATE' | translate }}
                                <span class="text-red-500 mx-2">*</span>
                            </label>
                            <input pInputText class="w-full" [placeholder]="'COMMON.EX_CAIRO' | translate " [(ngModel)]="formData.state" />
                        </div>
                        <div class="mb-3">
                            <label class="block font-bold mb-1">{{ 'HOMEOWNER.CITY' | translate }}</label>
                            <input pInputText class="w-full"  [placeholder]="'COMMON.EX_FIFTHSETTLEMENT' | translate " [(ngModel)]="formData.city" />
                        </div>
                        <div class="mb-3">
                            <label class="block font-bold mb-1">{{ 'HOMEOWNER.DISTRICT' | translate }}</label>
                            <input pInputText class="w-full" [placeholder]="'COMMON.EX_90ThSTREET' | translate " [(ngModel)]="formData.district" />
                        </div>
                        <div class="mb-3">
                            <label class="block font-bold mb-1">{{ 'HOMEOWNER.ADDRESS' | translate }}</label>
                            <input pInputText class="w-full" [(ngModel)]="formData.address" />
                        </div>

                        <!-- <div class="mb-3">
                            <label class="block font-bold mb-1">{{ 'HOMEOWNER.NATIONAL_ID_NUMBER' | translate }}</label>
                            <input pInputText class="w-full" [(ngModel)]="formData.nationalIdNumber" />
                        </div> -->

                    </div>
                </div>

                <!-- National ID Image -->
                <!-- <div class="col-span-12 md:col-span-4">
                    <div class="p-4 border-round border-1 border-surface-200 text-center">
                        <h4 class="mb-3">{{ 'HOMEOWNER.NATIONAL_ID_IMAGE' | translate }}</h4>
                        <div  class="mb-3">
                            <img [src]="profile?.nationalIdImageUrl" alt="National ID" class="w-full border-round" style="max-height:150px;object-fit:cover;" />
                        </div>
                        <p-fileupload name="nationalIdImage" mode="basic" accept="image/*" maxFileSize="2000000" [auto]="false" chooseLabel="اختر صورة" (onSelect)="onFileSelected($event, 'nationalId')"> </p-fileupload>
                    </div>
                </div> -->

                <!-- Selfie Image -->
                <!-- <div class="col-span-12 md:col-span-4">
                    <div class="p-4 border-round border-1 border-surface-200 text-center">
                        <h4 class="mb-3">{{ 'HOMEOWNER.SELFIE_IMAGE' | translate }}</h4>
                        <div  class="mb-3">
                            <img [src]="profile?.selfieImageUrl" alt="Selfie" class="w-full border-round" style="max-height:150px;object-fit:cover;" />
                        </div>
                        <p-fileupload name="selfieImage" mode="basic" accept="image/*" maxFileSize="2000000" [auto]="false" chooseLabel="اختر صورة" (onSelect)="onFileSelected($event, 'selfie')"> </p-fileupload>
                    </div>
                </div> -->

                <!-- Proof of Address Image -->
                <!-- <div class="col-span-12 md:col-span-4">
                    <div class="p-4 border-round border-1 border-surface-200 text-center">
                        <h4 class="mb-3">{{ 'HOMEOWNER.PROOF_OF_ADDRESS' | translate }}</h4>
                        <div  class="mb-3">
                            <img [src]="profile?.proofOfAddressImageUrl" alt="Proof of Address" class="w-full border-round" style="max-height:150px;object-fit:cover;" />
                        </div>
                        <p-fileupload name="proofOfAddressImage" mode="basic" accept="image/*" maxFileSize="2000000" [auto]="false" chooseLabel="اختر صورة" (onSelect)="onFileSelected($event, 'proofOfAddress')"> </p-fileupload>
                    </div>
                </div> -->

                <!-- Verification Status -->
                <!-- <div class="col-span-12">
                    <div class="p-3 border-round border-1 border-surface-200">
                        <span class="font-bold">{{ 'HOMEOWNER.VERIFICATION_STATUS' | translate }}: </span>
                        <span [class.text-green-500]="profile?.verificationStatus === 1" [class.text-orange-500]="profile?.verificationStatus === 0" [class.text-red-500]="profile?.verificationStatus === 2">
                            {{ getStatusLabel(profile?.verificationStatus) }}
                        </span>
                        <span  class="block text-sm text-muted-color mt-1">{{ profile?.verificationNotes }}</span>
                    </div>
                </div> -->

                <!-- Save Button -->
                <div class="col-span-12 text-center">
                    <p-button [label]="'COMMON.SAVE' | translate" icon="pi pi-check" (onClick)="save()" [loading]="isLoading"></p-button>
                </div>
            </div>
        </div>
    `
})
export class HomeownerProfileComponent implements OnInit {
    private homeownerService = inject(HomeownerService);
    private messageService = inject(MessageService);

    profile: HomeownerProfile | null = null;
    isLoading = false;
    errorMessage = '';

    formData = { fullName:'' ,address: '' ,phoneNumber:'',whatsAppNumber:'' ,
         state:'' , city: '', district: '' , nationalIdNumber: '' };

    selectedFiles: { nationalId?: File; selfie?: File; proofOfAddress?: File } = {};

    ngOnInit() {
        this.loadProfile();
    }

    loadProfile() {
        this.homeownerService.getProfile().subscribe({
            next: (res) => {
                this.profile = res;
                this.formData.fullName = res.fullName;
                this.formData.phoneNumber = res.phoneNumber??'';
                this.formData.whatsAppNumber = res.whatsAppNumber;
                this.formData.state = res.state;
                this.formData.city = res.city;
                this.formData.district = res.district || '';
                this.formData.address = res.address;
                this.formData.nationalIdNumber = res.nationalIdNumber;
            },
            error: () => this.errorMessage = 'فشل تحميل الملف الشخصي'
        });
    }

    onFileSelected(event: any, field: 'nationalId' | 'selfie' | 'proofOfAddress') {
        const file = event.currentFiles?.[0];
        if (file) this.selectedFiles[field] = file;
    }

    save() {
        this.isLoading = true;
        this.errorMessage = '';

        const fd = new FormData();
        fd.append('FullName', this.formData.fullName);
        fd.append('Address', this.formData.address);
        fd.append('PhoneNumber', this.formData.phoneNumber);
        fd.append('WhatsAppNumber', this.formData.whatsAppNumber);
        fd.append('State', this.formData.state!);
        fd.append('City', this.formData.city);
        fd.append('District', this.formData.district);
        fd.append('NationalIdNumber', this.formData.nationalIdNumber);

        if (this.selectedFiles['nationalId'])
            fd.append('nationalIdImage', this.selectedFiles['nationalId']);
        if (this.selectedFiles['selfie'])
            fd.append('selfieImage', this.selectedFiles['selfie']);
        if (this.selectedFiles['proofOfAddress'])
            fd.append('proofOfAddressImage', this.selectedFiles['proofOfAddress']);

        this.homeownerService.updateProfile(fd).subscribe({
            next: (res) => {
                this.isLoading = false;
                this.messageService.add({ severity: 'success', detail: res.message, life: 3000 });
                this.loadProfile();
            },
            error: (err) => {                
                this.isLoading = false;
                this.errorMessage = err.error?.message || 'فشل التحديث';
            }
        });
    }

    getStatusLabel(status: number | undefined): string {
        const labels: { [k: number]: string } = { 0: 'في الانتظار', 1: 'موثق', 2: 'مرفوض' };
        return labels[status ?? 0] || 'غير معروف';
    }
}