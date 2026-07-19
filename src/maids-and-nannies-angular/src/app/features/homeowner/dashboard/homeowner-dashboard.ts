import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { AuthService } from '../../../core/services/auth.service';
import { ApiService } from '../../../core/services/api.service';

@Component({
    selector: 'app-homeowner-dashboard',
    standalone: true,
    imports: [CommonModule, RouterModule, CardModule, ButtonModule, TableModule, TagModule],
    template: `
        <div class="grid grid-cols-12 gap-8">
            <div class="col-span-12">
                <h2>مرحباً {{ authService.currentUser()?.fullName }}</h2>
                <p class="text-muted-color" *ngIf="authService.currentUser()?.verificationStatus != 1">
                    حسابك بانتظار التأكيد من الإدارة
                </p>
            </div>

            <div class="col-span-12 md:col-span-4">
                <p-card styleClass="mb-0 cursor-pointer" routerLink="/homeowner/workers">
                    <div class="flex align-items-center gap-4">
                        <div class="flex align-items-center justify-content-center w-12 h-12 border-round bg-blue-100">
                            <i class="pi pi-search text-blue-500 text-xl"></i>
                        </div>
                        <div>
                            <span class="text-muted-color text-sm">البحث عن عاملة</span>
                            <div class="text-lg font-bold">ابحثي الآن</div>
                        </div>
                    </div>
                </p-card>
            </div>

            <div class="col-span-12 md:col-span-4">
                <p-card styleClass="mb-0 cursor-pointer" routerLink="/homeowner/bookings">
                    <div class="flex align-items-center gap-4">
                        <div class="flex align-items-center justify-content-center w-12 h-12 border-round bg-green-100">
                            <i class="pi pi-calendar text-green-500 text-xl"></i>
                        </div>
                        <div>
                            <span class="text-muted-color text-sm">حجوزاتي</span>
                            <div class="text-lg font-bold">{{ bookings().length }}</div>
                        </div>
                    </div>
                </p-card>
            </div>

            <div class="col-span-12 md:col-span-4">
                <p-card styleClass="mb-0">
                    <div class="flex align-items-center gap-4">
                        <div class="flex align-items-center justify-content-center w-12 h-12 border-round bg-purple-100">
                            <i class="pi pi-envelope text-purple-500 text-xl"></i>
                        </div>
                        <div>
                            <span class="text-muted-color text-sm">الرسائل</span>
                            <div class="text-lg font-bold">0</div>
                        </div>
                    </div>
                </p-card>
            </div>

            <div class="col-span-12">
                <p-card>
                    <ng-template #header>
                        <div class="flex align-items-center justify-content-between px-4 pt-4">
                            <h5 class="m-0">حجوزاتي الأخيرة</h5>
                            <p-button label="بحث عن عاملة" routerLink="/homeowner/workers" size="small"></p-button>
                        </div>
                    </ng-template>
                    <p-table [value]="bookings()" [rows]="5" [tableStyle]="{ 'min-width': '40rem' }">
                        <ng-template #header>
                            <tr>
                                <th>العاملة</th>
                                <th>التاريخ</th>
                                <th>المبلغ</th>
                                <th>الحالة</th>
                            </tr>
                        </ng-template>
                        <ng-template #body let-booking>
                            <tr>
                                <td>{{ booking.workerName }}</td>
                                <td>{{ booking.startDate | date:'shortDate' }}</td>
                                <td>{{ booking.monthlySalary | currency:'EGP':'symbol':'1.0-0' }}</td>
                                <td>
                                    <p-tag [value]="getStatusLabel(booking.status)" [severity]="getStatusSeverity(booking.status)"></p-tag>
                                </td>
                            </tr>
                        </ng-template>
                    </p-table>
                </p-card>
            </div>
        </div>
    `
})
export class HomeownerDashboard implements OnInit {
    authService = inject(AuthService);
    private apiService = inject(ApiService);

    bookings = signal<any[]>([]);

    ngOnInit() {
        this.loadBookings();
    }

    loadBookings() {
        this.apiService.getMyBookings().subscribe({
            next: (data) => this.bookings.set(data.Data || [])
        });
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
}
