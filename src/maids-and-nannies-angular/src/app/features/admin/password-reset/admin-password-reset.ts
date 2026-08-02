import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { ApiService } from '../../../core/services/api.service';

interface ResetRequest {
    id: number; userId: string; email: string; fullName: string;
    phoneNumber: string; code: string; status: number;
    createdAt: string; expiresAt: string;
}

@Component({
    selector: 'app-admin-password-reset',
    standalone: true,
    imports: [CommonModule, TableModule, ButtonModule, InputTextModule, ToastModule, TranslatePipe],
    providers: [MessageService],
    template: `
        <p-toast></p-toast>
        <div class="card">
            <div class="flex justify-content-between align-items-center mb-3">
                <h2 class="m-0">{{ 'ADMIN_RESET.TITLE' | translate }}</h2>
                <p-button [label]="'COMMON.REFRESH' | translate" icon="pi pi-refresh" severity="secondary" (onClick)="load()"></p-button>
            </div>

            <p-table [value]="requests()" [loading]="loading()" [rows]="10" [paginator]="true">
                <ng-template #header>
                    <tr>
                        <th>{{ 'COMMON.NAME' | translate }}</th>
                        <th>{{ 'AUTH.EMAIL' | translate }}</th>
                        <th>{{ 'ADMIN_RESET.CODE' | translate }}</th>
                        <th>{{ 'ADMIN_RESET.REQUESTED_AT' | translate }}</th>
                        <th>{{ 'ADMIN_RESET.EXPIRES_AT' | translate }}</th>
                        <th style="width: 14rem">{{ 'COMMON.ACTIONS' | translate }}</th>
                    </tr>
                </ng-template>
                <ng-template #body let-r>
                    <tr>
                        <td>{{ r.fullName }}</td>
                        <td>{{ r.email }}</td>
                        <td>
                            <div class="flex align-items-center gap-2">
                                <input pInputText [value]="r.code" readonly style="width: 6rem; text-align: center; font-weight: bold;" />
                                <p-button icon="pi pi-copy" severity="secondary" [text]="true" (onClick)="copyCode(r)"></p-button>
                            </div>
                        </td>
                        <td>{{ r.createdAt | date: 'short' }}</td>
                        <td>{{ r.expiresAt | date: 'short' }}</td>
                        <td>
                            <div class="flex align-items-center gap-2">
                                <a *ngIf="r.phoneNumber" [href]="waLink(r)" target="_blank" rel="noopener" class="inline-flex align-items-center gap-1 text-green-500 font-medium">
                                    <i class="pi pi-whatsapp"></i>{{ 'ADMIN_RESET.WA_LINK' | translate }}
                                </a>
                                <p-button [label]="'ADMIN_RESET.MARK_SENT' | translate" size="small" (onClick)="markSent(r)"></p-button>
                            </div>
                        </td>
                    </tr>
                </ng-template>
                <ng-template #emptymessage>
                    <tr><td colspan="6" class="text-center text-color-secondary">{{ 'ADMIN_RESET.EMPTY' | translate }}</td></tr>
                </ng-template>
            </p-table>
        </div>
    `
})
export class AdminPasswordReset implements OnInit {
    private apiService = inject(ApiService);
    private messageService = inject(MessageService);
    private translate = inject(TranslateService);

    requests = signal<ResetRequest[]>([]);
    loading = signal(false);

    ngOnInit() { this.load(); }

    load() {
        this.loading.set(true);
        this.apiService.getPasswordResetRequests().subscribe({
            next: (data) => this.requests.set(data),
            error: () => this.messageService.add({ severity: 'error', summary: this.translate.instant('COMMON.ERROR'), detail: this.translate.instant('COMMON.ERROR') }),
            complete: () => this.loading.set(false)
        });
    }

    copyCode(r: ResetRequest) {
        navigator.clipboard?.writeText(r.code).then(() =>
            this.messageService.add({ severity: 'success', summary: this.translate.instant('COMMON.SUCCESS'), detail: this.translate.instant('ADMIN_RESET.COPIED') })
        );
    }

    markSent(r: ResetRequest) {
        this.apiService.markResetRequestSent(r.id).subscribe({
            next: () => {
                this.messageService.add({ severity: 'success', summary: this.translate.instant('COMMON.SUCCESS'), detail: this.translate.instant('ADMIN_RESET.SENT_OK') });
                this.load();
            },
            error: () => this.messageService.add({ severity: 'error', summary: this.translate.instant('COMMON.ERROR'), detail: this.translate.instant('COMMON.ERROR') })
        });
    }

    waLink(r: ResetRequest): string {
        const digits = r.phoneNumber.replace(/[^0-9]/g, '');
        const intl = digits.startsWith('0') ? '2' + digits : digits;
        const text = encodeURIComponent(`${this.translate.instant('ADMIN_RESET.WA_TEXT')}: ${r.code}`);
        return `https://wa.me/${intl}?text=${text}`;
    }
}