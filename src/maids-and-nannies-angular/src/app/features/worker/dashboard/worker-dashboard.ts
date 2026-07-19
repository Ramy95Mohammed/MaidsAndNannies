import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';
import { AuthService } from '../../../core/services/auth.service';
import { ApiService } from '../../../core/services/api.service';

@Component({
    selector: 'app-worker-dashboard',
    standalone: true,
    imports: [CommonModule, RouterModule, CardModule, ButtonModule, TableModule, TagModule, ToastModule],
    providers: [MessageService],
    template: `
        <p-toast></p-toast>
        <div class="grid grid-cols-12 gap-8">
            <div class="col-span-12">
                <h2>مرحباً {{ authService.currentUser()?.fullName }}</h2>
            </div>

            <div class="col-span-12 md:col-span-4">
                <p-card styleClass="mb-0">
                    <div class="flex align-items-center gap-4">
                        <div class="flex align-items-center justify-content-center w-12 h-12 border-round bg-green-100">
                            <i class="pi pi-calendar text-green-500 text-xl"></i>
                        </div>
                        <div>
                            <span class="text-muted-color text-sm">الحجوزات النشطة</span>
                            <div class="text-lg font-bold">{{ activeBookings().length }}</div>
                        </div>
                    </div>
                </p-card>
            </div>

            <div class="col-span-12 md:col-span-4">
                <p-card styleClass="mb-0">
                    <div class="flex align-items-center justify-content-center w-12 h-12 border-round bg-orange-100">
                        <i class="pi pi-clock text-orange-500 text-xl"></i>
                    </div>
                    <div>
                        <span class="text-muted-color text-sm">طلبات جديدة</span>
                        <div class="text-lg font-bold">{{ pendingBookings().length }}</div>
                    </div>
                </p-card>
            </div>

            <div class="col-span-12 md:col-span-4">
                <p-card styleClass="mb-0">
                    <div class="flex align-items-center justify-content-center w-12 h-12 border-round bg-purple-100">
                        <i class="pi pi-star text-purple-500 text-xl"></i>
                    </div>
                    <div>
                        <span class="text-muted-color text-sm">التقييم</span>
                        <div class="text-lg font-bold">-</div>
                    </div>
                </p-card>
            </div>

            <div class="col-span-12">
                <p-card>
                    <ng-template #header>
                        <div class="flex align-items-center justify-content-between px-4 pt-4">
                            <h5 class="m-0">طلبات الحجز الواردة</h5>
                            <p-button label="تعديل الملف" routerLink="/worker/profile" size="small" [text]="true"></p-button>
                        </div>
                    </ng-template>
                    <p-table [value]="pendingBookings()" [rows]="5" [tableStyle]="{ 'min-width': '40rem' }">
                        <ng-template #header>
                            <tr>
                                <th>صاحبة المنزل</th>
                                <th>التاريخ</th>
                                <th>المبلغ</th>
                                <th>الإجراءات</th>
                            </tr>
                        </ng-template>
                        <ng-template #body let-booking>
                            <tr>
                                <td>{{ booking.homeownerName }}</td>
                                <td>{{ booking.startDate | date:'shortDate' }}</td>
                                <td>{{ booking.monthlySalary | currency:'EGP':'symbol':'1.0-0' }}</td>
                                <td>
                                    <p-button icon="pi pi-check" [rounded]="true" [outlined]="true" class="mr-2" severity="success" (click)="acceptBooking(booking.id)"></p-button>
                                    <p-button icon="pi pi-times" [rounded]="true" [outlined]="true" severity="danger" (click)="rejectBooking(booking.id)"></p-button>
                                </td>
                            </tr>
                        </ng-template>
                    </p-table>
                </p-card>
            </div>
        </div>
    `
})
export class WorkerDashboard implements OnInit {
    authService = inject(AuthService);
    private apiService = inject(ApiService);
    private messageService = inject(MessageService);

    activeBookings = signal<any[]>([]);
    pendingBookings = signal<any[]>([]);

    ngOnInit() {
        this.loadBookings();
    }

    loadBookings() {
        this.apiService.getWorkerBookings().subscribe({
            next: (data) => {
                const all = data.Data || [];
                this.activeBookings.set(all.filter((b: any) => b.status === 2));
                this.pendingBookings.set(all.filter((b: any) => b.status === 0));
            }
        });
    }

    acceptBooking(id: number) {
        this.apiService.updateBookingStatus(id, 1).subscribe({
            next: () => {
                this.messageService.add({ severity: 'success', summary: 'تم', detail: 'تم قبول الحجز' });
                this.loadBookings();
            },
            error: () => this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل قبول الحجز' })
        });
    }

    rejectBooking(id: number) {
        this.apiService.updateBookingStatus(id, 6).subscribe({
            next: () => {
                this.messageService.add({ severity: 'warn', summary: 'تم', detail: 'تم رفض الحجز' });
                this.loadBookings();
            },
            error: () => this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل رفض الحجز' })
        });
    }
}
