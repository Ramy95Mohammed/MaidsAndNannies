import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { DialogModule } from 'primeng/dialog';
import { InputNumberModule } from 'primeng/inputnumber';
import { MessageService, ConfirmationService } from 'primeng/api';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { ApiService } from '../../../core/services/api.service';

@Component({
    selector: 'app-admin-homeowners',
    standalone: true,
    imports: [CommonModule, ReactiveFormsModule, CardModule, ButtonModule, TableModule, TagModule, ToastModule, ConfirmDialogModule, DialogModule, InputNumberModule, TranslatePipe],
    providers: [MessageService, ConfirmationService],
    template: `
        <p-toast></p-toast>
        <p-confirmdialog></p-confirmdialog>

        <div class="card">
            <h2>{{ 'DASHBOARD.PENDING_VERIFICATIONS' | translate }} - Homeowners</h2>

            <p-table [value]="homeowners()" [rows]="10" [paginator]="true" [tableStyle]="{ 'min-width': '60rem' }">
                <ng-template #header>
                    <tr>
                        <th>{{ 'ADMIN.NAME' | translate }}</th>
                        <th>{{ 'ADMIN.EMAIL' | translate }}</th>
                        <th>{{ 'ADMIN.PHONE' | translate }}</th>
                        <th>{{ 'ADMIN.NATIONAL_ID' | translate }}</th>
                        <th>{{ 'ADMIN.CITY' | translate }}</th>
                        <th>{{ 'ADMIN.DATE' | translate }}</th>
                        <th>{{ 'ADMIN.TABLE_ACTIONS' | translate }}</th>
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

        <div class="card">
            <h2>كل صاحبات المنازل</h2>

            <p-table [value]="allHomeowners()" [rows]="10" [paginator]="true" [tableStyle]="{ 'min-width': '70rem' }">
                <ng-template #header>
                    <tr>
                        <th>{{ 'ADMIN.NAME' | translate }}</th>
                        <th>{{ 'ADMIN.EMAIL' | translate }}</th>
                        <th>{{ 'ADMIN.PHONE' | translate }}</th>
                        <th>استبدال (تقصير العاملة)</th>
                        <th>استبدال (رغبة شخصية)</th>
                        <th>{{ 'ADMIN.TABLE_ACTIONS' | translate }}</th>
                    </tr>
                </ng-template>
                <ng-template #body let-ho>
                    <tr>
                        <td>{{ ho.fullName }}</td>
                        <td>{{ ho.email }}</td>
                        <td>{{ ho.phoneNumber }}</td>
                        <td>{{ ho.maxFaultReplacementCount ?? 'حسب الإعدادات' }}</td>
                        <td>{{ ho.maxPreferenceReplacementCount ?? 'حسب الإعدادات' }}</td>
                        <td>
                            <p-button icon="pi pi-pencil" [rounded]="true" [outlined]="true" severity="warn" (click)="openLimitsDialog(ho)"></p-button>
                        </td>
                    </tr>
                </ng-template>
            </p-table>
        </div>

        <p-dialog header="تخصيص حدود الاستبدال" [modal]="true" [(visible)]="showLimitsDialog" [style]="{ width: '420px' }">
            <form [formGroup]="limitsForm" class="flex flex-column gap-3">
                <div>
                    <label class="block font-bold mb-1">الحد الأقصى للاستبدال (تقصير العاملة)</label>
                    <p-inputnumber formControlName="maxFaultReplacementCount" [min]="0" [showClear]="true" class="w-full"></p-inputnumber>
                    <small class="text-muted-color">اتركه فارغاً للرجوع للإعدادات العامة</small>
                </div>
                <div>
                    <label class="block font-bold mb-1">الحد الأقصى للاستبدال (رغبة شخصية)</label>
                    <p-inputnumber formControlName="maxPreferenceReplacementCount" [min]="0" [showClear]="true" class="w-full"></p-inputnumber>
                    <small class="text-muted-color">اتركه فارغاً للرجوع للإعدادات العامة</small>
                </div>
            </form>
            <ng-template #footer>
                <p-button label="حفظ" icon="pi pi-check" (onClick)="saveLimits()"></p-button>
            </ng-template>
        </p-dialog>
    `
})
export class AdminHomeowners implements OnInit {
    private apiService = inject(ApiService);
    private messageService = inject(MessageService);
    private confirmationService = inject(ConfirmationService);
    private translate = inject(TranslateService);
    private fb = inject(FormBuilder);

    homeowners = signal<any[]>([]);
    allHomeowners = signal<any[]>([]);

    showLimitsDialog = false;
    selectedHomeowner: any = null;

    limitsForm: FormGroup = this.fb.group({
        maxFaultReplacementCount: [null],
        maxPreferenceReplacementCount: [null]
    });

    ngOnInit() {
        this.loadData();
        this.apiService.getAllHomeowners().subscribe({
            next: (data) => this.allHomeowners.set(data)
        });
    }

    loadData() {
        this.apiService.getPendingHomeowners().subscribe({
            next: (data) => this.homeowners.set(data)
        });
    }

    openLimitsDialog(ho: any) {
        this.selectedHomeowner = ho;
        this.limitsForm.reset({
            maxFaultReplacementCount: ho.maxFaultReplacementCount,
            maxPreferenceReplacementCount: ho.maxPreferenceReplacementCount
        });
        this.showLimitsDialog = true;
    }

    saveLimits() {
        if (!this.selectedHomeowner) return;
        this.apiService.updateHomeownerReplacementLimits(this.selectedHomeowner.id, {
            maxFaultReplacementCount: this.limitsForm.get('maxFaultReplacementCount')?.value ?? null,
            maxPreferenceReplacementCount: this.limitsForm.get('maxPreferenceReplacementCount')?.value ?? null
        }).subscribe({
            next: () => {
                this.messageService.add({ severity: 'success', detail: 'تم تحديث حدود الاستبدال' });
                this.showLimitsDialog = false;
                this.apiService.getAllHomeowners().subscribe({
                    next: (data) => this.allHomeowners.set(data)
                });
            }
        });
    }

    verifyHomeowner(id: number) {
        this.confirmationService.confirm({
            message: this.translate.instant('ADMIN.CONFIRM_HOMEOWNER'),
            header: this.translate.instant('ADMIN.CONFIRM'),
            icon: 'pi pi-exclamation-triangle',
            accept: () => {
                this.apiService.verifyHomeowner(id).subscribe({
                    next: () => {
                        this.messageService.add({ severity: 'success', summary: this.translate.instant('COMMON.SUCCESS'), detail: this.translate.instant('ADMIN.HOMEOWNER_CONFIRMED') });
                        this.loadData();
                    }
                });
            }
        });
    }

    rejectHomeowner(id: number) {
        this.confirmationService.confirm({
            message: this.translate.instant('ADMIN.REJECT_HOMEOWNER'),
            header: this.translate.instant('ADMIN.REJECT'),
            icon: 'pi pi-exclamation-triangle',
            accept: () => {
                this.apiService.rejectHomeowner(id, 'تم الرفض من الإدارة').subscribe({
                    next: () => {
                        this.messageService.add({ severity: 'info', summary: this.translate.instant('COMMON.SUCCESS'), detail: this.translate.instant('ADMIN.REQUEST_REJECTED') });
                        this.loadData();
                    }
                });
            }
        });
    }
}