import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { MessageService, ConfirmationService } from 'primeng/api';
import { ApiService } from '../../../core/services/api.service';

@Component({
    selector: 'app-admin-workers',
    standalone: true,
    imports: [CommonModule, CardModule, ButtonModule, TableModule, TagModule, ToastModule, ConfirmDialogModule],
    providers: [MessageService, ConfirmationService],
    template: `
        <p-toast></p-toast>
        <p-confirmdialog></p-confirmdialog>

        <div class="card">
            <h2>العاملات بانتظار التأكيد</h2>

            <p-table [value]="workers()" [rows]="10" [paginator]="true" [tableStyle]="{ 'min-width': '60rem' }">
                <ng-template #header>
                    <tr>
                        <th>الاسم</th>
                        <th>الجنسية</th>
                        <th>التخصص</th>
                        <th>المدينة</th>
                        <th>الباسبورت</th>
                        <th>التاريخ</th>
                        <th>الإجراءات</th>
                    </tr>
                </ng-template>
                <ng-template #body let-worker>
                    <tr>
                        <td>{{ worker.fullName }}</td>
                        <td>{{ worker.nationality }}</td>
                        <td>{{ getSpecializationLabel(worker.specialization) }}</td>
                        <td>{{ worker.city }}</td>
                        <td>{{ worker.passportNumber || 'غير متوفر' }}</td>
                        <td>{{ worker.createdAt | date:'short' }}</td>
                        <td>
                            <p-button icon="pi pi-check" [rounded]="true" [outlined]="true" class="mr-2" severity="success" (click)="verifyWorker(worker.id)"></p-button>
                        </td>
                    </tr>
                </ng-template>
            </p-table>
        </div>
    `
})
export class AdminWorkers implements OnInit {
    private apiService = inject(ApiService);
    private messageService = inject(MessageService);
    private confirmationService = inject(ConfirmationService);

    workers = signal<any[]>([]);

    specializations: { [key: number]: string } = {
        0: 'تنظيف',
        1: 'طبخ',
        2: 'رعاية أطفال',
        3: 'رعاية مسنين',
        4: 'عمل منزلي عام'
    };

    ngOnInit() {
        this.loadData();
    }

    loadData() {
        this.apiService.getPendingWorkers().subscribe({
            next: (data) => this.workers.set(data)
        });
    }

    getSpecializationLabel(value: number): string {
        return this.specializations[value] || 'غير محدد';
    }

    verifyWorker(id: number) {
        this.confirmationService.confirm({
            message: 'هل أنت متأكد من تأكيد العاملة؟',
            header: 'تأكيد',
            icon: 'pi pi-exclamation-triangle',
            accept: () => {
                this.apiService.verifyWorker(id).subscribe({
                    next: () => {
                        this.messageService.add({ severity: 'success', summary: 'تم', detail: 'تم تأكيد العاملة' });
                        this.loadData();
                    }
                });
            }
        });
    }
}
