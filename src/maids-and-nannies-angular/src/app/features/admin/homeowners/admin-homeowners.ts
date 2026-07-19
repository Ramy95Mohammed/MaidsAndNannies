import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { MessageService, ConfirmationService } from 'primeng/api';
import { TranslatePipe } from '@ngx-translate/core';
import { ApiService } from '../../../core/services/api.service';

@Component({
    selector: 'app-admin-homeowners',
    standalone: true,
    imports: [CommonModule, CardModule, ButtonModule, TableModule, TagModule, ToastModule, ConfirmDialogModule, TranslatePipe],
    providers: [MessageService, ConfirmationService],
    template: `
        <p-toast></p-toast>
        <p-confirmdialog></p-confirmdialog>

        <div class="card">
            <h2>{{ 'DASHBOARD.PENDING_VERIFICATIONS' | translate }} - Homeowners</h2>

            <p-table [value]="homeowners()" [rows]="10" [paginator]="true" [tableStyle]="{ 'min-width': '60rem' }">
                <ng-template #header>
                    <tr>
                        <th>الاسم</th>
                        <th>البريد</th>
                        <th>الهاتف</th>
                        <th>الرقم القومي</th>
                        <th>المدينة</th>
                        <th>التاريخ</th>
                        <th>الإجراءات</th>
                    </tr>
                </ng-template>
                <ng-template #body let-homeowner>
                    <tr>
                        <td>{{ homeowner.fullName }}</td>
                        <td>{{ homeowner.email }}</td>
                        <td>{{ homeowner.phoneNumber }}</td>
                        <td>{{ homeowner.nationalIdNumber }}</td>
                        <td>{{ homeowner.city }}</td>
                        <td>{{ homeowner.createdAt | date:'short' }}</td>
                        <td>
                            <p-button icon="pi pi-check" [rounded]="true" [outlined]="true" class="mr-2" severity="success" (click)="verifyHomeowner(homeowner.id)"></p-button>
                            <p-button icon="pi pi-times" [rounded]="true" [outlined]="true" severity="danger" (click)="rejectHomeowner(homeowner.id)"></p-button>
                        </td>
                    </tr>
                </ng-template>
            </p-table>
        </div>
    `
})
export class AdminHomeowners implements OnInit {
    private apiService = inject(ApiService);
    private messageService = inject(MessageService);
    private confirmationService = inject(ConfirmationService);

    homeowners = signal<any[]>([]);

    ngOnInit() {
        this.loadData();
    }

    loadData() {
        this.apiService.getPendingHomeowners().subscribe({
            next: (data) => this.homeowners.set(data)
        });
    }

    verifyHomeowner(id: number) {
        this.confirmationService.confirm({
            message: 'هل أنت متأكد من تأكيد صاحبة المنزل؟',
            header: 'تأكيد',
            icon: 'pi pi-exclamation-triangle',
            accept: () => {
                this.apiService.verifyHomeowner(id).subscribe({
                    next: () => {
                        this.messageService.add({ severity: 'success', summary: 'تم', detail: 'تم تأكيد صاحبة المنزل' });
                        this.loadData();
                    }
                });
            }
        });
    }

    rejectHomeowner(id: number) {
        this.confirmationService.confirm({
            message: 'هل أنت متأكد من رفض طلب التسجيل؟',
            header: 'رفض',
            icon: 'pi pi-exclamation-triangle',
            accept: () => {
                this.apiService.rejectHomeowner(id, 'تم الرفض من الإدارة').subscribe({
                    next: () => {
                        this.messageService.add({ severity: 'info', summary: 'تم', detail: 'تم رفض الطلب' });
                        this.loadData();
                    }
                });
            }
        });
    }
}
