import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';
import { TranslatePipe } from '@ngx-translate/core';
import { ApiService } from '../../../core/services/api.service';

@Component({
    selector: 'app-worker-bookings',
    standalone: true,
    imports: [CommonModule, RouterModule, CardModule, ButtonModule, TableModule, TagModule, ToastModule, TranslatePipe],
    providers: [MessageService],
    template: `
        <p-toast></p-toast>
        <div class="card">
            <h2>حجوزاتي</h2>

            <p-table [value]="bookings()" [rows]="10" [paginator]="true">
                <ng-template pTemplate="header">
                    <tr>
                        <th>الصاحبة</th>
                        <th>التاريخ</th>
                        <th>المبلغ</th>
                        <th>الحالة</th>
                        <th>إجراءات</th>
                    </tr>
                </ng-template>
                <ng-template pTemplate="body" let-booking>
                    <tr>
                        <td>{{ booking.homeownerName }}</td>
                        <td>{{ booking.startDate | date:'short' }}</td>
                        <td>{{ booking.monthlySalary | currency:'EGP':'symbol':'1.0-0' }}</td>
                        <td>
                            <p-tag [value]="getStatusLabel(booking.status)" [severity]="getStatusSeverity(booking.status)"></p-tag>
                        </td>
                        <td>
                            <p-button *ngIf="booking.status === 0" label="قبول" icon="pi pi-check" [rounded]="true" [text]="true" (onClick)="updateStatus(booking.id, 1)"></p-button>
                            <p-button *ngIf="booking.status === 1" label="بدء العمل" icon="pi pi-play" [rounded]="true" [text]="true" (onClick)="updateStatus(booking.id, 2)"></p-button>
                            <p-button *ngIf="booking.status === 2" label="إنهاء" icon="pi pi-stop" [rounded]="true" [text]="true" (onClick)="updateStatus(booking.id, 3)"></p-button>
                            <p-button *ngIf="booking.status === 0" label="رفض" icon="pi pi-times" [rounded]="true" [text]="true" severity="danger" (onClick)="updateStatus(booking.id, 6)"></p-button>
                        </td>
                    </tr>
                </ng-template>
            </p-table>

            <div *ngIf="bookings().length === 0" class="text-center py-8">
                <p class="text-muted-color">لا توجد حجوزات بعد</p>
            </div>
        </div>
    `
})
export class WorkerBookings implements OnInit {
    private apiService = inject(ApiService);
    private messageService = inject(MessageService);

    bookings = signal<any[]>([]);

    ngOnInit() {
        this.loadBookings();
    }

    loadBookings() {
        this.apiService.getWorkerBookings().subscribe({
            next: (data) => this.bookings.set(data.Data || []),
            error: () => this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل تحميل الحجوزات' })
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

    updateStatus(id: number, status: number) {
        this.apiService.updateBookingStatus(id, status).subscribe({
            next: () => {
                this.messageService.add({ severity: 'success', summary: 'تم', detail: 'تم تحديث الحالة' });
                this.loadBookings();
            },
            error: () => this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل تحديث الحالة' })
        });
    }
}
