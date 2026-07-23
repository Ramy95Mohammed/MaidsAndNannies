import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { ButtonModule } from 'primeng/button';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { MessageService, ConfirmationService } from 'primeng/api';
import { ApiService } from '../../../core/services/api.service';
import { BookingService } from '@/core/services/booking.service';

interface BookingRow {
    id: number;
    homeownerName: string;
    workerName: string;
    monthlySalary: number;
    status: number;
    replacementCount: number;
}

@Component({
    selector: 'app-admin-bookings',
    standalone: true,
    imports: [CommonModule, ReactiveFormsModule, TableModule, TagModule, ButtonModule, ToastModule, ConfirmDialogModule],
    providers: [MessageService, ConfirmationService],
    template: `
        <p-toast />
        <p-confirmdialog />
        <div class="card">
            <h2>إدارة الحجوزات</h2>
            <p-table [value]="bookings()" [rows]="15" [tableStyle]="{ 'min-width': '65rem' }">
                <ng-template #header>
                    <tr>
                        <th>#</th>
                        <th>صاحبة المنزل</th>
                        <th>العاملة</th>
                        <th>الراتب</th>
                        <th>العمولة</th>
                        <th>الحالة</th>
                        <th>استبدال</th>
                        <th>إجراءات</th>
                    </tr>
                </ng-template>
                <ng-template #body let-b>
                    <tr>
                        <td>{{ b.id }}</td>
                        <td>{{ b.homeownerName }}</td>
                        <td>{{ b.workerName }}</td>
                        <td>{{ b.monthlySalary | currency:'EGP':'symbol':'1.0-0' }}</td>
                        <td>{{ b.commissionAmount | currency:'EGP':'symbol':'1.0-0' }}</td>
                        <td><p-tag [value]="statusLabel(b.status)" [severity]="statusSeverity(b.status)"></p-tag></td>
                        <td>{{ b.replacementCount }}/2</td>
                        <td>
                            <div class="flex gap-1">
                                <p-button *ngIf="b.status === 0" label="تأكيد العاملة" size="small" (onClick)="confirmWorker(b.id)"></p-button>
                                <p-button *ngIf="b.status === 1" label="طلب دفع" size="small" (onClick)="requestPayment(b.id)"></p-button>
                                <p-button *ngIf="b.status === 3" label="بدء العمل" size="small" (onClick)="startWork(b.id)"></p-button>
                                <p-button *ngIf="b.status === 4" label="إنهاء" size="small" severity="success" (onClick)="completeWork(b.id)"></p-button>                                
                                <p-button *ngIf="b.status === 7" label="تأكيد البديلة" size="small" severity="warn" (onClick)="confirmWorker(b.id)"></p-button>
                            </div>
                        </td>
                    </tr>
                </ng-template>
            </p-table>
        </div>
    `
})
export class AdminBookings implements OnInit {
    private apiService = inject(ApiService);
    private bookingService = inject(BookingService);
    private messageService = inject(MessageService);
    private confirmationService = inject(ConfirmationService);

    bookings = signal<BookingRow[]>([]);

    ngOnInit() { this.load(); }

    load() {
        this.apiService.getAllBookings().subscribe({
            next: (data) => this.bookings.set(data)
        });
    }

    confirmWorker(id: number) {
        this.confirmationService.confirm({
            message: 'تأكيد العاملة لهذا الحجز؟',
            header: 'تأكيد',
            icon: 'pi pi-exclamation-triangle',
            accept: () => {
                this.bookingService.confirmWorker(id).subscribe({
                    next: () => { this.messageService.add({ severity:'success', detail:'تم تأكيد العاملة' }); this.load(); }
                });
            }
        });
    }

    requestPayment(id: number) {
        this.bookingService.requestPayment(id).subscribe({
            next: () => { this.messageService.add({ severity:'success', detail:'تم طلب الدفع' }); this.load(); }
        });
    }

    startWork(id: number) {
        this.bookingService.startWork(id).subscribe({
            next: () => { this.messageService.add({ severity:'success', detail:'تم بدء العمل' }); this.load(); }
        });
    }

    completeWork(id: number) {
        this.bookingService.completeWork(id).subscribe({
            next: () => { this.messageService.add({ severity:'success', detail:'تم إنهاء الحجز' }); this.load(); }
        });
    }

    statusLabel(s: number): string {
        return ['في الانتظار','تم تأكيد العاملة','بانتظار الدفع','مدفوع','نشط','مكتمل','ملغي','طلب استبدال','قيد المراجعة'][s]||'—';
    }
  statusSeverity(s: number): string {
    return ['warn','info','warn','success','info','success','danger','warn','info'][s]||'secondary';
}
}