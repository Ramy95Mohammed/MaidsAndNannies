import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { ButtonModule } from 'primeng/button';
import { TranslatePipe } from '@ngx-translate/core';
import { BookingService, BookingListDto } from '../../../core/services/booking.service';

@Component({
    selector: 'app-worker-bookings',
    standalone: true,
    imports: [CommonModule, RouterModule, TableModule, TagModule, ButtonModule, TranslatePipe],
    template: `
        <div class="card">
            <h2>{{ 'BOOKING.MY_BOOKINGS' | translate }}</h2>
            <p-table [value]="bookings()" [rows]="10" [tableStyle]="{ 'min-width': '50rem' }">
                <ng-template #header>
                    <tr>
                        <th>{{ 'COMMON.ID' | translate }}</th>
                        <th>{{ 'BOOKING.START_DATE' | translate }}</th>
                        <th>{{ 'BOOKING.MONTHLY_SALARY' | translate }}</th>
                        <th>{{ 'BOOKING.TYPE' | translate }}</th>
                        <th>{{ 'BOOKING.QUANTITY' | translate }}</th>
                        <th>{{ 'BOOKING.TOTAL_AMOUNT' | translate }}</th>
                        <th>{{ 'BOOKING.STATUS' | translate }}</th>
                    </tr>
                </ng-template>
                <ng-template #body let-booking>
                    <tr>
                        <td>{{ booking.id }}</td>
                        <td>{{ booking.startDate | date:'shortDate' }}</td>
                      <td>{{ (booking.bookingType == 0)? booking.dailySalary:(booking.bookingType == 1)?booking.monthlySalary:booking.hourlySalary | currency:booking.currencyCode:'':'1.0-0' }} {{ booking.currencyCode }}</td>
                        <td>{{ getBookingTypeLabel(booking.bookingType) }}</td>
                        <td>{{ (booking.bookingType == 1)?"__": booking.quantity }}</td>
                        <td>{{ booking.totalAmount | currency:booking.currencyCode:'':'1.0-0' }} {{ booking.currencyCode }}</td>
                        <td><p-tag [value]="getStatusLabel(booking.status)" [severity]="getStatusSeverity(booking.status)"></p-tag></td>
                    </tr>
                </ng-template>
            </p-table>
        </div>
    `
})
export class WorkerBookings implements OnInit {
    private bookingService = inject(BookingService);
    bookings = signal<BookingListDto[]>([]);

    ngOnInit() {
        this.bookingService.getWorkerBookings().subscribe({
            next: (data) => this.bookings.set(data)
        });
    }

        getBookingTypeLabel(type: number): string {
        return ['يومي', 'شهري', 'ساعي'][type] || '—';
    }
    getStatusLabel(status: number): string {
               return ['في الانتظار', 'تم تأكيد العاملة', 'تم تأكيد العاملة', 'تم تأكيد العاملة', 'نشط', 'مكتمل', 'ملغي', 'طلب استبدال', 'قيد المراجعة'][status] || '—';
    }

    getStatusSeverity(status: number): any {
        const s: { [k: number]: any } = {
            0: 'warn', 1: 'info', 2: 'warn', 3: 'success', 4: 'info', 5: 'success', 6: 'danger', 7: 'warn',8:'info'
        };
        return s[status] || 'secondary';
    }
}