import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { TranslatePipe } from '@ngx-translate/core';
import { ApiService } from '../../../core/services/api.service';

@Component({
    selector: 'app-admin-dashboard',
    standalone: true,
    imports: [CommonModule, RouterModule, CardModule, ButtonModule, TableModule, TagModule, TranslatePipe],
    template: `
        <div class="grid grid-cols-12 gap-8">
            <div class="col-span-12">
                <h2>{{ 'DASHBOARD.TITLE' | translate }} - Admin</h2>
            </div>

            <!-- Stats Cards -->
            <div class="col-span-12 md:col-span-6 xl:col-span-3">
                <p-card styleClass="mb-0">
                    <div class="flex align-items-center gap-4">
                        <div class="flex align-items-center justify-content-center w-12 h-12 border-round bg-blue-100">
                            <i class="pi pi-users text-blue-500 text-xl"></i>
                        </div>
                        <div>
                            <span class="text-muted-color text-sm">{{ 'DASHBOARD.TOTAL_USERS' | translate }}</span>
                            <div class="text-2xl font-bold">{{ stats().totalUsers }}</div>
                        </div>
                    </div>
                </p-card>
            </div>

            <div class="col-span-12 md:col-span-6 xl:col-span-3">
                <p-card styleClass="mb-0">
                    <div class="flex align-items-center gap-4">
                        <div class="flex align-items-center justify-content-center w-12 h-12 border-round bg-green-100">
                            <i class="pi pi-home text-green-500 text-xl"></i>
                        </div>
                        <div>
                            <span class="text-muted-color text-sm">{{ 'DASHBOARD.TOTAL_HOMEOWNERS' | translate }}</span>
                            <div class="text-2xl font-bold">{{ stats().totalHomeowners }}</div>
                        </div>
                    </div>
                </p-card>
            </div>

            <div class="col-span-12 md:col-span-6 xl:col-span-3">
                <p-card styleClass="mb-0">
                    <div class="flex align-items-center gap-4">
                        <div class="flex align-items-center justify-content-center w-12 h-12 border-round bg-orange-100">
                            <i class="pi pi-user text-orange-500 text-xl"></i>
                        </div>
                        <div>
                            <span class="text-muted-color text-sm">{{ 'DASHBOARD.TOTAL_WORKERS' | translate }}</span>
                            <div class="text-2xl font-bold">{{ stats().totalWorkers }}</div>
                        </div>
                    </div>
                </p-card>
            </div>

            <div class="col-span-12 md:col-span-6 xl:col-span-3">
                <p-card styleClass="mb-0">
                    <div class="flex align-items-center gap-4">
                        <div class="flex align-items-center justify-content-center w-12 h-12 border-round bg-purple-100">
                            <i class="pi pi-calendar text-purple-500 text-xl"></i>
                        </div>
                        <div>
                            <span class="text-muted-color text-sm">{{ 'DASHBOARD.TOTAL_BOOKINGS' | translate }}</span>
                            <div class="text-2xl font-bold">{{ stats().totalBookings }}</div>
                        </div>
                    </div>
                </p-card>
            </div>

            <!-- Pending Items -->
            <div class="col-span-12 md:col-span-6">
                <p-card>
                    <ng-template #header>
                        <div class="flex align-items-center justify-content-between px-4 pt-4">
                            <h5 class="m-0">{{ 'DASHBOARD.PENDING_VERIFICATIONS' | translate }}</h5>
                            <p-button label="عرض الكل" routerLink="/admin/homeowners" [text]="true" size="small"></p-button>
                        </div>
                    </ng-template>
                    <div class="text-center py-4">
                        <div class="text-4xl font-bold text-orange-500">{{ stats().pendingVerifications }}</div>
                        <span class="text-muted-color">طلب تأكيد معلق</span>
                    </div>
                </p-card>
            </div>

            <div class="col-span-12 md:col-span-6">
                <p-card>
                    <ng-template #header>
                        <div class="flex align-items-center justify-content-between px-4 pt-4">
                            <h5 class="m-0">{{ 'DASHBOARD.PENDING_PAYMENTS' | translate }}</h5>
                            <p-button label="عرض الكل" routerLink="/admin/payments" [text]="true" size="small"></p-button>
                        </div>
                    </ng-template>
                    <div class="text-center py-4">
                        <div class="text-4xl font-bold text-red-500">{{ stats().pendingPayments }}</div>
                        <span class="text-muted-color">دفعة معلقة</span>
                    </div>
                </p-card>
            </div>

            <div class="col-span-12">
                <p-card>
                    <ng-template #header>
                        <div class="flex align-items-center justify-content-between px-4 pt-4">
                            <h5 class="m-0">{{ 'DASHBOARD.ACTIVE_BOOKINGS' | translate }}</h5>
                            <p-button label="عرض الكل" routerLink="/admin/bookings" [text]="true" size="small"></p-button>
                        </div>
                    </ng-template>
                    <div class="text-center py-4">
                        <div class="text-4xl font-bold text-green-500">{{ stats().activeBookings }}</div>
                        <span class="text-muted-color">حجز نشط حالياً</span>
                    </div>
                </p-card>
            </div>
        </div>
    `
})
export class AdminDashboard implements OnInit {
    private apiService = inject(ApiService);

    stats = signal<any>({
        totalUsers: 0,
        totalHomeowners: 0,
        totalWorkers: 0,
        totalBookings: 0,
        activeBookings: 0,
        pendingVerifications: 0,
        pendingPayments: 0
    });

    ngOnInit() {
        this.loadStats();
    }

    loadStats() {
        this.apiService.getAdminDashboard().subscribe({
            next: (data) => this.stats.set(data)
        });
    }
}
