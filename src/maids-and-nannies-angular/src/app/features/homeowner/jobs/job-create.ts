import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
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

@Component({
  selector: 'app-job-create',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, CardModule, ButtonModule,
    InputTextModule, Textarea, SelectModule, DatePickerModule, ToastModule, TranslatePipe],
  providers: [MessageService],
  template: `
    <p-toast />
    <div class="card">
      <h2>{{ 'JOB_POST.CREATE' | translate }}</h2>
      <div class="grid grid-cols-12 gap-4">
        <div class="col-span-12">
          <label class="block font-bold mb-2">{{ 'JOB_POST.DESCRIPTION' | translate }}</label>
          <textarea pInputTextarea [(ngModel)]="description" rows="5" class="w-full"
            [placeholder]="'JOB_POST.DESC_PLACEHOLDER' | translate"></textarea>
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
          <p-select [(ngModel)]="bookingType" [options]="bookingTypes" optionLabel="label" optionValue="value" styleClass="w-full"></p-select>
        </div>
        <div class="col-span-12 md:col-span-4">
          <label class="block font-bold mb-2">{{ 'BOOKING.COMMISSION_TYPE' | translate }}</label>
          <p-select [(ngModel)]="commissionType" [options]="commissionOptions" optionLabel="label" optionValue="value" styleClass="w-full"></p-select>
        </div>
        <div class="col-span-12 md:col-span-4">
          <label class="block font-bold mb-2">{{ 'WORKER.SPECIALIZATION' | translate }}</label>
          <p-select [(ngModel)]="specialization" [options]="specializations" optionLabel="label" optionValue="value" styleClass="w-full"></p-select>
        </div>
        <div class="col-span-12 md:col-span-4">
          <label class="block font-bold mb-2">{{ 'BOOKING.START_DATE' | translate }}</label>
          <p-datepicker [(ngModel)]="startDate" styleClass="w-full"></p-datepicker>
        </div>
    
        <div class="col-span-12 md:col-span-4">
        <label class="block font-bold mb-2">{{ 'WORKER.CURRENCY' | translate }}</label>        
        <p-select  [(ngModel)]="currencyId" [options]="currencyOptions()" optionLabel="label" optionValue="value" class="w-full"></p-select>
        </div>

        <div class="col-span-12 md:col-span-4">
          <label class="block font-bold mb-2">{{ 'BOOKING.QUANTITY' | translate }}</label>
          <input pInputText [(ngModel)]="quantity" type="number" min="1" class="w-full" />
        </div>
        <div class="col-span-12 text-center">
          <p-button [label]="'JOB_POST.SUBMIT' | translate" icon="pi pi-send" (onClick)="submit()" [loading]="loading"></p-button>
        </div>
      </div>
    </div>
  `
})
export class JobCreate implements OnInit {
  private api = inject(ApiService);
  private router = inject(Router);
  private msg = inject(MessageService);
  langService = inject(LanguageService);
  

  currencyId = 1;
 currencyOptions = signal<{ value: number; label: string }[]>([]);
  description = '';
  monthlySalary = 0; dailySalary = 0; hourlySalary = 0;
  bookingType = 1; commissionType = 0; specialization = 0;
  startDate: Date | null = null; quantity = 1;
  loading = false;
  private translate = inject(TranslateService);
 private currencyService = inject(CurrencyService);

 bookingTypes:any;
 commissionOptions:any;
 specializations:any;


  
  ngOnInit(){
    this.loadCurrencies();
    this.setOptions();
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

     setOptions(){
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


 private toDateOnlyString(date: Date | null): string | null {
        if (!date) return null;
        const year = date.getFullYear();
        const month = String(date.getMonth() + 1).padStart(2, '0');
        const day = String(date.getDate()).padStart(2, '0');
        return `${year}-${month}-${day}`;
    }
  submit() {
    if (!this.description || !this.startDate) {
      this.msg.add({ severity: 'warn', detail: 'يرجى ملء جميع الحقول' });
      return;
    }
    this.loading = true;
    this.api.createJobPost({
      description: this.description,
      monthlySalary: this.monthlySalary,
      dailySalary: this.dailySalary,
      hourlySalary: this.hourlySalary,
      bookingType: this.bookingType,
      commissionType: this.commissionType,
      specialization: this.specialization,
      startDate: this.toDateOnlyString(this.startDate),
      quantity: this.quantity,
      currencyId: this.currencyId
    }).subscribe({
      next: () => { this.msg.add({ severity: 'success', detail: 'تم إنشاء الإعلان' }); setTimeout(() => this.router.navigate(['/homeowner/jobs']), 1500); },
      error: () => this.msg.add({ severity: 'error', detail: 'فشل إنشاء الإعلان' }),
      complete: () => this.loading = false
    });
  }

  
}