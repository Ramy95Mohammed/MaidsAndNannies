import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { AuthService } from '../../../core/services/auth.service';
import { ApiService } from '../../../core/services/api.service';
import { SubscriptionService } from '@/core/services/subscription.service';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';

@Component({
    selector: 'app-homeowner-dashboard',
    standalone: true,
    imports: [CommonModule, RouterModule, CardModule, ButtonModule, TableModule, TagModule , TranslatePipe],
    template: `
        <div class="grid grid-cols-12 gap-8">
            <div class="col-span-12">
                <h2>{{ 'HOMEOWNER_DASHBOARD.WELCOME' | translate:{name: authService.currentUser()?.fullName} }}</h2>
                <p class="text-muted-color" *ngIf="authService.currentUser()?.verificationStatus != 1">
                {{ 'HOMEOWNER_DASHBOARD.PENDING_VERIFICATION' | translate }}                            
            </p>
            </div>

            <div class="col-span-12 md:col-span-4">
                <p-card styleClass="mb-0 cursor-pointer" routerLink="/homeowner/workers">
                    <div class="flex align-items-center gap-4">
                        <div class="flex align-items-center justify-content-center w-12 h-12 border-round bg-blue-100">
                            <i class="pi pi-search text-blue-500 text-xl"></i>
                        </div>
                        <div>
                            <span class="text-muted-color text-sm">{{ 'HOMEOWNER_DASHBOARD.SEARCH_WORKER' | translate }}</span>
                            <div class="text-lg font-bold">{{ 'HOMEOWNER_DASHBOARD.SEARCH_NOW' | translate }}</div>
                        </div>
                    </div>
                </p-card>
            </div>

            <div class="col-span-12 md:col-span-4">
                <p-card styleClass="mb-0 cursor" routerLink="/homeowner/bookings">
                    <div class="flex align-items-center gap-4">
                        <div class="flex align-items-center justify-content-center w-12 h-12 border-round bg-green-100">
                            <i class="pi pi-calendar text-green-500 text-xl"></i>
                        </div>
                        <div>
                            <span class="text-muted-color text-sm">{{ 'HOMEOWNER_DASHBOARD.MY_BOOKINGS' | translate }}</span>
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
                            <span class="text-muted-color text-sm">{{ 'HOMEOWNER_DASHBOARD.MESSAGES' | translate }}</span>
                            <div class="text-lg font-bold">0</div>
                        </div>
                    </div>
                </p-card>
            </div>

            <div class="col-span-12" *ngIf="subscriptionWarning()">
                <p-card styleClass="border-1 border-orange-300 bg-orange-50">
                    <div class="flex align-items-center gap-2">
                        <i class="pi pi-exclamation-triangle text-orange-500"></i>
                        <span>{{ subscriptionWarning() }}</span>
                        <p-button routerLink="/homeowner/subscriptions" [label]="'HOMEOWNER_DASHBOARD.RENEW' | translate" size="small" class="mr-auto"></p-button>
                    </div>
                </p-card>
            </div>

            <div class="col-span-12">
                <p-card>
                    <ng-template #header>
                        <div class="flex align-items-center justify-content-between px-4 pt-4">
                            <h5 class="m-0">{{ 'HOMEOWNER_DASHBOARD.RECENT_BOOKINGS' | translate }}</h5>
                            <p-button [label]="'HOMEOWNER_DASHBOARD.SEARCH_BUTTON' | translate" routerLink="/homeowner/workers" size="small"></p-button>
                        </div>
                    </ng-template>
                    <p-table [value]="bookings()" [rows]="5" [paginator]="true" [tableStyle]="{ 'min-width': '40rem' }">
                        <ng-template #header>
                            <tr>
                                 <th>{{ 'ADMIN.TABLE_WORKER' | translate }}</th>
                                 <th>{{ 'HOMEOWNER_DASHBOARD.DATE' | translate }}</th>
                                <th>{{ 'ADMIN.TABLE_AMOUNT' | translate }}</th>
                                <th>{{ 'ADMIN.TABLE_STATUS' | translate }}</th>
                            </tr>
                        </ng-template>
                        <ng-template #body let-booking>
                            <tr>
                                <td>{{ booking.workerName }}</td>
                                <td>{{ booking.startDate | date:'shortDate' }}</td>
                                <td>{{ booking.monthlySalary | currency:booking.currencyCode:'':'1.0-0' }} {{booking.currencyCode}}</td>
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
    private subscriptionService = inject(SubscriptionService);
    private translate = inject(TranslateService);

    subscriptionWarning = signal<string | null>(null);

    bookings = signal<any[]>([]);

    ngOnInit() {
        this.loadBookings();

        this.subscriptionService.getMySubscriptions().subscribe({
    next: (data) => {
        const active = data.find(s => s.isActive && s.daysRemaining > 0);
        if (active && active.daysRemaining <= 7)
            this.subscriptionWarning.set(this.translate.instant('HOMEOWNER_DASHBOARD.DAYS_LEFT', { days: active.daysRemaining }));
    }
});
    }

    loadBookings() {
        this.apiService.getMyBookings().subscribe({
          next: (data: any) => this.bookings.set(Array.isArray(data) ? data : [])
        });
    }

    getStatusLabel(status: number): string {
       const labels: { [k: number]: string } = {
            0: this.translate.instant('BOOKING_DETAIL.STATUS_LABEL_PENDING'), 1: this.translate.instant('ADMIN.WORKER_CONFIRMED'), 2: this.translate.instant('BOOKING_DETAIL.STATUS_LABEL_WAITING_PAYMENT'),
            3: this.translate.instant('BOOKING_DETAIL.STATUS_LABEL_PAID'), 4: this.translate.instant('BOOKING_DETAIL.STATUS_LABEL_ACTIVE'), 5: this.translate.instant('BOOKING_DETAIL.STATUS_LABEL_COMPLETED'), 6: this.translate.instant('BOOKING_DETAIL.STATUS_LABEL_CANCELLED'), 7: this.translate.instant('BOOKING_DETAIL.STATUS_LABEL_REPLACEMENT') , 8:this.translate.instant('BOOKING_DETAIL.STATUS_LABEL_REVIEW')
        };
        return labels[status] || this.translate.instant('COMMON.UNKNOWN');
    }

    getStatusSeverity(status: number): 'success' | 'info' | 'warn' | 'danger' | 'secondary' | 'contrast' | undefined {
         const s: { [k: number]: any } = {
            0: 'warn', 1: 'info', 2: 'warn', 3: 'success', 4: 'info', 5: 'success', 6: 'danger', 7: 'warn',8:'info'
        };
        return s[status] || 'secondary';
    }
}