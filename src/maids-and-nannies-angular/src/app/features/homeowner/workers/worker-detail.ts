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

@Component({
    selector: 'app-worker-detail',
    standalone: true,
    imports: [
        CommonModule, FormsModule, RouterModule, CardModule, ButtonModule,
        InputTextModule, SelectModule, RatingModule, ChipModule, DividerModule,
        TextareaModule, DatePickerModule, DialogModule, ToastModule
    ],
    providers: [MessageService],
    template: `
        <p-toast></p-toast>
        <div class="card">
            <a routerLink="/homeowner/workers" class="text-primary cursor-pointer"><i class="pi pi-arrow-left mr-2"></i>العودة للبحث</a>

            <div *ngIf="worker()" class="mt-4">
                <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">
                    <div class="lg:col-span-2">
                        <div class="flex align-items-center gap-4 mb-4">
                            <div class="w-20 h-20 border-circle bg-primary flex align-items-center justify-content-center">
                                <i class="pi pi-user text-white text-3xl"></i>
                            </div>
                            <div>
                                <h2 class="text-2xl font-bold m-0">{{ worker().fullName }}</h2>
                                <p class="text-muted-color m-0">{{ worker().nationality }} - {{ worker().city || 'غير محدد' }}</p>
                                <p-rating [(ngModel)]="worker().averageRating"></p-rating>
                                <span class="text-sm text-muted-color ml-2">({{ worker().totalReviews }} تقييم)</span>
                            </div>
                        </div>

                        <p-divider></p-divider>

                        <h3>المعلومات الشخصية</h3>
                        <div class="grid grid-cols-2 gap-4">
                            <div><strong>العمر:</strong> {{ worker().age }} سنة</div>
                            <div><strong>الخبرة:</strong> {{ worker().experienceYears }} سنوات</div>
                            <div><strong>مقيمة:</strong> {{ worker().isLiveIn ? 'نعم' : 'لا' }}</div>
                            <div><strong>الأجر اليومي:</strong> {{ worker().dailyRate }} {{ currenciesMap()[worker().currencyId] || 'EGP' }} </div>
                            <div><strong>الأجر الشهري:</strong> {{ worker().monthlyRate }} {{ currenciesMap()[worker().currencyId] || 'EGP' }}</div>
                            <div><strong>الأجر الساعة:</strong> {{ worker().hourlyRate }} {{ currenciesMap()[worker().currencyId] || 'EGP' }}</div>
                        </div>

                        <div class="mt-4">
                            <strong>التخصص:</strong>
                            <div class="flex flex-wrap gap-2 mt-2">
                                <p-chip [label]="getSpecLabel(worker().specialization)"></p-chip>
                            </div>
                        </div>

                        <div *ngIf="worker().bio" class="mt-4">
                            <strong>الوصف:</strong>
                            <p class="mt-2">{{ worker().bio }}</p>
                        </div>

                        <div *ngIf="worker().languages" class="mt-4">
                            <strong>اللغات:</strong>
                            <div class="flex flex-wrap gap-2 mt-2">
                                <p-chip *ngFor="let lang of worker().languages.split(',')" [label]="lang.trim()"></p-chip>
                            </div>
                        </div>
                    </div>

                    <div class="card">
                        <h3 class="text-center">احجز الآن</h3>

                        <div class="mb-3">
                            <label class="block font-bold mb-2">نوع الحجز</label>
                            <p-select [(ngModel)]="bookingType"
                            (onChange)="disableOrEnableComissionTypeAndQuantity($event.value)"
                             [options]="bookingTypes" optionLabel="label" optionValue="value" styleClass="w-full"></p-select>
                        </div>

                        <div class="mb-3">
                            <label class="block font-bold mb-2">نوع العمولة</label>
                            <p-select [(ngModel)]="commissionType"
                            [disabled]="commissionTypeIsDisabled"
                             [options]="commissionOptions" optionLabel="label" optionValue="value" styleClass="w-full"></p-select>
                        </div>

                        <div class="mb-3">
                            <label class="block font-bold mb-2">تاريخ البداية</label>
                            <p-datepicker [(ngModel)]="startDate" dateFormat="yy-mm-dd" styleClass="w-full" placeholder="اختر التاريخ"></p-datepicker>
                        </div>

                                                <div class="mb-3">
                            <label class="block font-bold mb-2">الكمية (عدد الأيام/الساعات)</label>
                            <input pInputText [(ngModel)]="quantity" [disabled]="quantityIsDisabled" type="number" min="1" class="w-full" />
                        </div>

                        <div class="mb-3">
                            <label class="block font-bold mb-2">ملاحظات</label>
                            <textarea pTextarea [(ngModel)]="notes" rows="3" class="w-full" placeholder="أي ملاحظات إضافية"></textarea>
                        </div>

                        <p-button label="تأكيد الحجز" icon="pi pi-check" styleClass="w-full" (onClick)="createBooking()"></p-button>
                    </div>
                </div>

                <div class="mt-6">
                    <h3>التقييمات</h3>
                    <div *ngFor="let review of worker().reviews || []" class="card mb-3">
                        <div class="flex align-items-center gap-2 mb-2">
                            <p-rating [(ngModel)]="review.rating"></p-rating>
                            <span class="text-sm text-muted-color">{{ review.createdAt | date:'short' }}</span>
                        </div>
                        <p>{{ review.comment }}</p>
                    </div>
                    <div *ngIf="!worker().reviews || worker().reviews.length === 0" class="text-muted-color">
                        لا توجد تقييمات بعد
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

   currenciesMap = signal<{ [id: number]: string }>({});
    worker = signal<any>(null);
    bookingType = 0;
    commissionType = 0;
    startDate: Date | null = null;
    quantity = 1;
    notes = '';

    commissionTypeIsDisabled:boolean = true;
    quantityIsDisabled:boolean = false;

    bookingTypes = [
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
                error: () => this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'لم يتم العثور على العاملة' })
            });
        }

         this.currencyService.loadCurrencies(this.currenciesMap);
    }

    getSpecLabel(value: number): string {
        const labels: { [k: number]: string } = { 0: 'تنظيف', 1: 'طبخ', 2: 'رعاية أطفال', 3: 'رعاية مسنين', 4: 'عمل منزلي' };
        return labels[value] || 'غير محدد';
    }

       createBooking() {
        if (!this.startDate) {
            this.messageService.add({ severity: 'warn', summary: 'تنبيه', detail: 'اختر تاريخ البداية' });
            return;
        }
        if (!this.quantity || this.quantity < 1) {
            this.messageService.add({ severity: 'warn', summary: 'تنبيه', detail: 'أدخل كمية صحيحة' });
            return;
        }

        this.apiService.createBooking({
            workerId: this.worker().id,
            serviceType: this.bookingType,
            bookingType: this.bookingType,
            quantity: this.quantity,
            startDate: this.startDate.toISOString().split('T')[0],
            monthlySalary: this.worker().monthlyRate,
            dailySalary: this.worker().dailyRate,
            hourlySalary: this.worker().hourlyRate,
            commissionType: this.commissionType
        }).subscribe({
            next: () => {
                this.messageService.add({ severity: 'success', summary: 'تم', detail: 'تم إنشاء الحجز بنجاح' });
                setTimeout(() => this.router.navigate(['/homeowner/bookings']), 1500);
            },
            error: () => this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل إنشاء الحجز' })
        });
    }

    disableOrEnableComissionTypeAndQuantity(bookingType:number | null){
      if(bookingType == null) return;

      if(bookingType == 0 || bookingType == 2){
        this.commissionTypeIsDisabled = true;  
        this.quantityIsDisabled = false;      
      }
      else{
         this.commissionTypeIsDisabled = false;
         this.quantityIsDisabled = true;
      }
    }
}
