import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { MessageService, ConfirmationService } from 'primeng/api';
import { ApiService } from '../../../core/services/api.service';
import { LanguageService } from '../../../core/services/language.service';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { ToggleSwitch } from "primeng/toggleswitch";

@Component({
    selector: 'app-admin-workers',
    standalone: true,
    imports: [CommonModule, FormsModule, CardModule, ButtonModule, TableModule, TagModule, ToastModule, ConfirmDialogModule, TranslatePipe, ToggleSwitch],
    providers: [MessageService, ConfirmationService],
    template: `
        <p-toast></p-toast>
        <p-confirmdialog></p-confirmdialog>

        <div class="card">
            <h2>{{ 'ADMIN.WORKERS_ALL' | translate }}</h2>
            <p-table [value]="workers()" [rows]="10" [paginator]="true" [tableStyle]="{ 'min-width': '60rem' }">
                <ng-template #header>
                    <tr>
                        <th>{{ 'ADMIN.NAME' | translate }}</th>
                        <th>{{ 'ADMIN.NATIONALITY' | translate }}</th>
                        <th>{{ 'ADMIN.SPECIALIZATION' | translate }}</th>
                        <th>{{ 'ADMIN.AVAILABILITY' | translate }}</th>
                        <th>{{ 'ADMIN.PASSPORT' | translate }}</th>
                        <th>{{ 'ADMIN.DATE' | translate }}</th>
                        <th>{{ 'ADMIN.TABLE_ACTIONS' | translate }}</th>
                    </tr>
                </ng-template>
                <ng-template #body let-worker>
                    <tr>
                        <td>{{ worker.fullName }}</td>
                        <td>{{ getNationalityLabel(worker) }}</td>
                        <td>{{ getSpecializationsLabel(worker.specializations) }}</td>
                        <td>
                            <p-toggleswitch [ngModel]="worker.isAvailable"  (onChange)="toggleAvailability(worker, $event.checked)"/>                            
                        </td>
                        <td>{{ worker.passportNumber || ('ADMIN.NOT_AVAILABLE' | translate) }}</td>
                        <td>{{ worker.createdAt | date:'short' }}</td>
                        <td>
                            <p-button *ngIf="worker.verificationStatus === 0" icon="pi pi-check" [rounded]="true" [outlined]="true" class="mr-2" severity="success" (click)="verifyWorker(worker.id)"></p-button>
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
    private translate = inject(TranslateService);
    private langService = inject(LanguageService);

    workers = signal<any[]>([]);

    get specializationLabels(): any {
        return {
            0: this.translate.instant('SPECIALIZATIONS.CLEANING'),
            1: this.translate.instant('SPECIALIZATIONS.COOKING'),
            2: this.translate.instant('SPECIALIZATIONS.CHILDCARE'),
            3: this.translate.instant('SPECIALIZATIONS.ELDERLYCARE'),
            4: this.translate.instant('SPECIALIZATIONS.GENERALHOUSEKEEPING')
        };
    }

    ngOnInit() { this.loadData(); }

    loadData() {
        this.apiService.getAllWorkers().subscribe({
            next: (data) => this.workers.set(data)
        });
    }

    getNationalityLabel(w: any): string {
        return this.langService.getCurrentLanguage() === 'ar'
            ? (w.nationalityAr ?? '—')
            : (w.nationalityEn ?? '—');
    }

    getSpecializationsLabel(specs: number[] | null): string {
        if (!specs || specs.length === 0) return this.translate.instant('COMMON.UNSPECIFIED');
        return specs.map(s => this.specializationLabels[s] || s).join(', ');
    }

    toggleAvailability(worker: any, value: boolean) {
        this.apiService.updateWorkerAvailability(worker.id, value).subscribe({
            next: () => {
                worker.isAvailable = value;
                this.messageService.add({ severity: 'success', summary: this.translate.instant('COMMON.SUCCESS'), detail: this.translate.instant('ADMIN.AVAILABILITY_UPDATED') });
            },
            error: () => {
                this.messageService.add({ severity: 'error', summary: this.translate.instant('COMMON.ERROR'), detail: this.translate.instant('ADMIN.AVAILABILITY_FAILED') });
                this.loadData();
            }
        });
    }

    verifyWorker(id: number) {
        this.confirmationService.confirm({
            message: this.translate.instant('ADMIN.CONFIRM_WORKER_TITLE'),
            header: this.translate.instant('ADMIN.CONFIRM'),
            icon: 'pi pi-exclamation-triangle',
            accept: () => {
                this.apiService.verifyWorker(id).subscribe({
                    next: () => {
                        this.messageService.add({ severity: 'success', summary: this.translate.instant('COMMON.SUCCESS'), detail: this.translate.instant('ADMIN.WORKER_CONFIRMED') });
                        this.loadData();
                    }
                });
            }
        });
    }
}