import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { ButtonModule } from 'primeng/button';
import { TranslatePipe } from '@ngx-translate/core';
import { BookingService, BookingListDto } from '../../../core/services/booking.service';

@Component({
    selector: 'app-homeowner-bookings',
    standalone: true,
    imports: [CommonModule, RouterModule, TableModule, TagModule, ButtonModule, TranslatePipe],
    template: `
        <div class="card">
            <div class="flex justify-content-between mb-4">
                <h2>{{ 'BOOKING.MY_BOOKINGS' | translate }}</h2>
            </div>
            <p-table [value]="bookings()" [rows]="10" [tableStyle]="{ 'min-width': '50rem' }">
                <ng-template #header>
                    <tr>
                        <th>{{ 'COMMON.ID' | translate }}</th>
                        <th>{{ 'WORKER.REGISTER' | translate }}</th>
                        <th>{{ 'BOOKING.START_DATE' | translate }}</th>
                        <th>{{ 'BOOKING.MONTHLY_SALARY' | translate }}</th>
                        <th>{{ 'BOOKING.STATUS' | translate }}</th>
                        <th>{{ 'COMMON.ACTIONS' | translate }}</th>
                    </tr>
                </ng-template>
                <ng-template #body let-booking>
                    <tr>
                        <td>{{ booking.id }}</td>
                        <td>{{ booking.workerName }}</td>
                        <td>{{ booking.startDate | date:'shortDate' }}</td>
                        <td>{{ booking.monthlySalary | currency:'EGP':'symbol':'1.0-0' }}</td>
                        <td><p-tag [value]="getStatusLabel(booking.status)" [severity]="getStatusSeverity(booking.status)"></p-tag></td>
                        <td>
                            <p-button [routerLink]="['/homeowner/bookings', booking.id]" [label]="'COMMON.VIEW' | translate" size="small"></p-button>
                        </td>
                    </tr>
                </ng-template>
            </p-table>
        </div>
    `
})
export class HomeownerBookings implements OnInit {
    private bookingService = inject(BookingService);
    bookings = signal<BookingListDto[]>([]);

    ngOnInit() {
        this.bookingService.getMyBookings().subscribe({
            next: (data) => this.bookings.set(data)
        });
    }

    getStatusLabel(status: number): string {
        const labels: { [k: number]: string } = {
            0: 'في الانتظار', 1: 'تم تأكيد العاملة', 2: 'بانتظار الدفع',
            3: 'مدفوع', 4: 'نشط', 5: 'مكتمل', 6: 'ملغي', 7: 'طلب استبدال' , 8:'قيد المراجعة'
        };
        return labels[status] || 'غير معروف';
    }

    getStatusSeverity(status: number): any {
        const s: { [k: number]: any } = {
            0: 'warn', 1: 'info', 2: 'warn', 3: 'success', 4: 'info', 5: 'success', 6: 'danger', 7: 'warn',8:'info'
        };
        return s[status] || 'secondary';
    }
}