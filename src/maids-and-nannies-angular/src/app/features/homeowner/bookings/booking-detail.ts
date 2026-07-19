import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { TagModule } from 'primeng/tag';
import { DividerModule } from 'primeng/divider';
import { ApiService } from '../../../core/services/api.service';

@Component({
    selector: 'app-booking-detail',
    standalone: true,
    imports: [CommonModule, FormsModule, RouterModule, CardModule, ButtonModule, TagModule, DividerModule],
    template: `
        <div class="card">
            <a routerLink="/homeowner/bookings" class="text-primary cursor-pointer"><i class="pi pi-arrow-left mr-2"></i>العودة للحجوزات</a>

            <div *ngIf="booking()" class="mt-4">
                <div class="flex align-items-center justify-content-between mb-4">
                    <h2 class="m-0">تفاصيل الحجز #{{ booking().id }}</h2>
                    <p-tag [value]="getStatusLabel(booking().status)" [severity]="getStatusSeverity(booking().status)"></p-tag>
                </div>

                <p-divider></p-divider>

                <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
                    <div>
                        <h3>معلومات الحجز</h3>
                        <div class="mb-2"><strong>صاحبة المنزل:</strong> {{ booking().homeowner?.fullName }}</div>
                        <div class="mb-2"><strong>العاملة:</strong> {{ booking().worker?.fullName }}</div>
                        <div class="mb-2"><strong>التخصص:</strong> {{ getSpecLabel(booking().serviceType) }}</div>
                        <div class="mb-2"><strong>تاريخ البداية:</strong> {{ booking().startDate | date:'longDate' }}</div>
                        <div class="mb-2"><strong>تاريخ النهاية:</strong> {{ booking().endDate ? (booking().endDate | date:'longDate') : '---' }}</div>
                        <div class="mb-2"><strong>المرتب الشهري:</strong> {{ booking().monthlySalary | currency:'EGP':'symbol':'1.0-0' }}</div>
                        <div class="mb-2"><strong>العمولة:</strong> {{ booking().commissionAmount | currency:'EGP':'symbol':'1.0-0' }}</div>
                    </div>

                    <div>
                        <h3>الدفع</h3>
                        <div class="mb-2"><strong>نوع العمولة:</strong> {{ booking().commissionType === 0 ? 'عمولة من أول شهر' : 'اشتراك شهري' }}</div>
                        <div class="mb-2"><strong>طريقة الدفع:</strong> {{ getPaymentMethodLabel(booking().paymentMethod) }}</div>
                        <div class="mb-2"><strong>حالة الدفع:</strong> {{ booking().isPaid ? 'مدفوع' : 'لم يتم الدفع' }}</div>
                    </div>
                </div>
            </div>
        </div>
    `
})
export class BookingDetail implements OnInit {
    private apiService = inject(ApiService);
    private route = inject(ActivatedRoute);

    booking = signal<any>(null);

    ngOnInit() {
        const id = this.route.snapshot.paramMap.get('id');
        if (id) {
            this.apiService.getBooking(id).subscribe({
                next: (data) => this.booking.set(data),
                error: () => {}
            });
        }
    }

    getStatusLabel(status: number): string {
        const labels: { [k: number]: string } = {
            0: 'في الانتظار', 1: 'مقبول', 2: 'قيد العمل',
            3: 'مكتمل', 4: 'بانتظار الدفع', 5: 'مدفوع', 6: 'ملغي'
        };
        return labels[status] || 'غير معروف';
    }

    getStatusSeverity(status: number): 'success' | 'info' | 'warn' | 'danger' | 'secondary' | 'contrast' | undefined {
        const s: { [k: number]: any } = {
            0: 'warn', 1: 'info', 2: 'info', 3: 'success', 4: 'warn', 5: 'success', 6: 'danger'
        };
        return s[status] || 'secondary';
    }

    getSpecLabel(value: number): string {
        const labels: { [k: number]: string } = { 0: 'تنظيف', 1: 'طبخ', 2: 'رعاية أطفال', 3: 'رعاية مسنين', 4: 'عمل منزلي' };
        return labels[value] || 'غير محدد';
    }

    getPaymentMethodLabel(method: number | null): string {
        if (method === null || method === undefined) return 'لم يتم الدفع';
        return method === 0 ? 'فودافون كاش' : 'انستاباي';
    }
}
