    import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
import { CheckboxModule } from 'primeng/checkbox';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';
import { TranslatePipe } from '@ngx-translate/core';
import { CurrencyService, CurrencyDto } from '../../../core/services/currency.service';

@Component({
    selector: 'app-admin-currencies',
    standalone: true,
    imports: [
        CommonModule, ReactiveFormsModule, TableModule, ButtonModule, DialogModule,
        InputTextModule, InputNumberModule, CheckboxModule, ToastModule, TranslatePipe
    ],
    providers: [MessageService],
    template: `
        <p-toast />
        <div class="card">
            <div class="flex justify-content-between mb-4">
                <h2>{{ 'CURRENCY.MANAGE' | translate }}</h2>
                <p-button [label]="'CURRENCY.ADD' | translate" icon="pi pi-plus" (onClick)="showAddDialog()"></p-button>
            </div>

            <p-table [value]="currencies()" [rows]="15" [tableStyle]="{'min-width':'50rem'}">
                <ng-template #header>
                    <tr>
                        <th>{{ 'COMMON.ID' | translate }}</th>
                        <th>{{ 'CURRENCY.CODE' | translate }}</th>
                        <th>{{ 'CURRENCY.SYMBOL' | translate }}</th>
                        <th>{{ 'CURRENCY.NAME_AR' | translate }}</th>
                        <th>{{ 'CURRENCY.NAME_EN' | translate }}</th>
                        <th>{{ 'CURRENCY.RATE' | translate }}</th>
                        <th>{{ 'CURRENCY.ACTIVE' | translate }}</th>
                        <th>{{ 'COMMON.ACTIONS' | translate }}</th>
                    </tr>
                </ng-template>
                <ng-template #body let-c>
                    <tr>
                        <td>{{ c.id }}</td>
                        <td>{{ c.code }}</td>
                        <td>{{ c.symbol }}</td>
                        <td>{{ c.nameAr }}</td>
                        <td>{{ c.nameEn }}</td>
                        <td>{{ c.rateToEgp }}</td>
                        <td>
                            <i [class]="c.isActive ? 'pi pi-check text-green-500' : 'pi pi-times text-red-500'"></i>
                        </td>
                        <td>
                            <div class="flex gap-1">
                                <p-button icon="pi pi-pencil" size="small" [text]="true" (onClick)="showEditDialog(c)"></p-button>
                                <p-button icon="pi pi-trash" size="small" severity="danger" [text]="true" (onClick)="deleteCurrency(c.id)"></p-button>
                            </div>
                        </td>
                    </tr>
                </ng-template>
            </p-table>
        </div>

        <p-dialog [(visible)]="dialogVisible" [header]="isEdit ? ('CURRENCY.EDIT' | translate) : ('CURRENCY.ADD' | translate)" [modal]="true" styleClass="w-30rem">
            <form [formGroup]="form" class="flex flex-col gap-3">
                <div>
                    <label class="block font-bold mb-1">{{ 'CURRENCY.CODE' | translate }}</label>
                    <input pInputText formControlName="code" class="w-full" />
                </div>
                <div>
                    <label class="block font-bold mb-1">{{ 'CURRENCY.SYMBOL' | translate }}</label>
                    <input pInputText formControlName="symbol" class="w-full" />
                </div>
                <div>
                    <label class="block font-bold mb-1">{{ 'CURRENCY.NAME_AR' | translate }}</label>
                    <input pInputText formControlName="nameAr" class="w-full" />
                </div>
                <div>
                    <label class="block font-bold mb-1">{{ 'CURRENCY.NAME_EN' | translate }}</label>
                    <input pInputText formControlName="nameEn" class="w-full" />
                </div>
                <div>
                    <label class="block font-bold mb-1">{{ 'CURRENCY.RATE' | translate }}</label>
                    <p-inputnumber formControlName="rateToEgp" mode="decimal" [minFractionDigits]="2" [min]="0" class="w-full"></p-inputnumber>
                </div>
                <div class="flex align-items-center gap-2">
                    <p-checkbox formControlName="isActive" [binary]="true"></p-checkbox>
                    <label>{{ 'CURRENCY.ACTIVE' | translate }}</label>
                </div>
            </form>
            <div class="flex justify-content-end gap-2 mt-3">
                <p-button [label]="'COMMON.CANCEL' | translate" [outlined]="true" (onClick)="dialogVisible = false"></p-button>
                <p-button [label]="'COMMON.SAVE' | translate" (onClick)="save()" [disabled]="form.invalid"></p-button>
            </div>
        </p-dialog>
    `
})
export class AdminCurrencies implements OnInit {
    private currencyService = inject(CurrencyService);
    private fb = inject(FormBuilder);
    private messageService = inject(MessageService);

    currencies = signal<CurrencyDto[]>([]);
    dialogVisible = false;
    isEdit = false;
    editId: number | null = null;

    form: FormGroup = this.fb.group({
        code: ['', Validators.required],
        symbol: ['', Validators.required],
        nameAr: ['', Validators.required],
        nameEn: ['', Validators.required],
        rateToEgp: [1, [Validators.required, Validators.min(0.01)]],
        isActive: [true]
    });

    ngOnInit() { this.load(); }

    load() {
        this.currencyService.getCurrencies(true).subscribe({
            next: (data) => this.currencies.set(data)
        });
    }

    showAddDialog() {
        this.isEdit = false;
        this.editId = null;
        this.form.reset({ code: '', symbol: '', nameAr: '', nameEn: '', rateToEgp: 1, isActive: true });
        this.dialogVisible = true;
    }

    showEditDialog(c: CurrencyDto) {
        this.isEdit = true;
        this.editId = c.id;
        this.form.patchValue(c);
        this.dialogVisible = true;
    }

    save() {
        if (this.form.invalid) return;
        const data = this.form.value;

        if (this.isEdit && this.editId) {
            this.currencyService.updateCurrency(this.editId, data).subscribe({
                next: () => { this.messageService.add({ severity:'success', detail:'تم التحديث' }); this.dialogVisible = false; this.load(); }
            });
        } else {
            this.currencyService.createCurrency(data).subscribe({
                next: () => { this.messageService.add({ severity:'success', detail:'تمت الإضافة' }); this.dialogVisible = false; this.load(); }
            });
        }
    }

    deleteCurrency(id: number) {
        this.currencyService.deleteCurrency(id).subscribe({
            next: () => { this.messageService.add({ severity:'success', detail:'تم الحذف' }); this.load(); }
        });
    }
}