import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { ButtonModule } from 'primeng/button';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { MessageService, ConfirmationService } from 'primeng/api';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { SubscriptionService, SubscriptionDto } from '../../../core/services/subscription.service';

@Component({
    selector: 'app-admin-subscriptions',
    standalone: true,
    imports: [CommonModule, TableModule, TagModule, ButtonModule, ToastModule, ConfirmDialogModule, TranslatePipe],
    providers: [MessageService, ConfirmationService],
    template: `
        <p-toast />
        <p-confirmdialog />
        <div class="card">
            <h2>{{ 'SUBSCRIPTION.ALL_SUBSCRIPTIONS' | translate }}</h2>

              <p-table [value]="subscriptions()" [rows]="15" [paginator]="true" [tableStyle]="{ 'min-width': '60rem' }">
                <ng-template #header>
                    <tr>
                        <th>#</th>
                        <th>{{ 'HOMEOWNER.REGISTER' | translate }}</th>
                        <th>{{ 'SUBSCRIPTION.AMOUNT' | translate }}</th>
                        <th>{{ 'SUBSCRIPTION.START_DATE' | translate }}</th>
                        <th>{{ 'SUBSCRIPTION.END_DATE' | translate }}</th>
                        <th>{{ 'SUBSCRIPTION.STATUS' | translate }}</th>
                        <th>{{ 'COMMON.ACTIONS' | translate }}</th>
                    </tr>
                </ng-template>
                <ng-template #body let-s>
                    <tr>
                        <td>{{ s.id }}</td>
                        <td>{{ s.homeownerName }}</td>
                        <td>{{ s.amount | currency:'EGP':'symbol':'1.0-0' }}</td>
                        <td>{{ s.startDate | date:'shortDate' }}</td>
                        <td>{{ s.endDate | date:'shortDate' }}</td>
                        <td>
                            <p-tag *ngIf="s.isActive" [value]="'SUBSCRIPTION.ACTIVE' | translate" severity="success"></p-tag>
                            <p-tag *ngIf="!s.isActive" [value]="'SUBSCRIPTION.PENDING_TAG' | translate" severity="warn"></p-tag>
                        </td>
                        <td>
                            <p-button *ngIf="!s.isActive" [label]="'ADMIN.CONFIRM_RENEWAL' | translate" size="small" severity="success" (onClick)="confirmRenewal(s.id)"></p-button>
                        </td>
                    </tr>
                </ng-template>
            </p-table>
        </div>
    `
})
export class AdminSubscriptions implements OnInit {
    private subscriptionService = inject(SubscriptionService);
    private messageService = inject(MessageService);
    private confirmationService = inject(ConfirmationService);
    private translate = inject(TranslateService);

    subscriptions = signal<SubscriptionDto[]>([]);

    ngOnInit() { this.load(); }

    load() {
        this.subscriptionService.getAllSubscriptions().subscribe({
            next: (data) => this.subscriptions.set(data)
        });
    }

    confirmRenewal(id: number) {
        this.confirmationService.confirm({
            message: this.translate.instant('ADMIN.CONFIRM_RENEW_MSG'),
            header: this.translate.instant('ADMIN.CONFIRM'),
            icon: 'pi pi-exclamation-triangle',
            accept: () => {
                this.subscriptionService.confirmRenewal(id).subscribe({
                    next: () => {
                        this.messageService.add({ severity: 'success', detail: this.translate.instant('ADMIN.RENEWAL_CONFIRMED') });
                        this.load();
                    }
                });
            }
        });
    }
}