import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { MessageModule } from 'primeng/message';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { FileUploadModule } from 'primeng/fileupload';
import { ToastModule } from 'primeng/toast';
import { MessageService, ConfirmationService } from 'primeng/api';
import { ApiService } from '@/core/services/api.service';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';

@Component({
    selector: 'app-admin-backup',
    standalone: true,
    imports: [CommonModule, CardModule, ButtonModule, MessageModule, ConfirmDialogModule, FileUploadModule, ToastModule, TranslatePipe],
    providers: [MessageService, ConfirmationService],
    template: `
        <p-toast />
        <p-confirmdialog />
        <div class="card">
            <h2>{{ 'ADMIN_BACKUP.TITLE' | translate }}</h2>
            <div class="grid grid-cols-12 gap-4">
                <div class="col-span-12 md:col-span-6">
                    <p-card header="{{ 'ADMIN_BACKUP.CREATE_TITLE' | translate }}">
                        <p>{{ 'ADMIN_BACKUP.CREATE_DESC' | translate }}</p>
                        <p-button [label]="'ADMIN_BACKUP.CREATE' | translate" icon="pi pi-download" (onClick)="createBackup()" [loading]="creating()"></p-button>
                    </p-card>
                </div>
                <div class="col-span-12 md:col-span-6">
                    <p-card header="{{ 'ADMIN_BACKUP.RESTORE_TITLE' | translate }}">
                        <p-message severity="warn" [text]="'ADMIN_BACKUP.RESTORE_WARNING' | translate"></p-message>
                        <p class="mt-3" *ngIf="selectedFile">{{ 'ADMIN_BACKUP.SELECTED_FILE' | translate }}: <strong>{{ selectedFile.name }}</strong></p>
                        <div class="flex flex-column gap-3 mt-3">
                            <p-fileupload mode="basic" accept=".bak" [auto]="false" [chooseLabel]="'ADMIN_BACKUP.CHOOSE' | translate" (onSelect)="onFileSelected($event)"></p-fileupload>
                            <p-button [label]="'ADMIN_BACKUP.RESTORE' | translate" icon="pi pi-upload" severity="danger" [loading]="restoring()" [disabled]="!selectedFile" (onClick)="confirmRestore()"></p-button>
                        </div>
                    </p-card>
                </div>
            </div>
        </div>
    `
})
export class AdminBackup {
    private api = inject(ApiService);
    private messageService = inject(MessageService);
    private confirmationService = inject(ConfirmationService);
    private translate = inject(TranslateService);

    creating = signal(false);
    restoring = signal(false);
    selectedFile: File | null = null;

    onFileSelected(event: any) {
        this.selectedFile = event.currentFiles?.[0] ?? null;
    }

    createBackup() {
        this.creating.set(true);
        this.api.createBackup().subscribe({
            next: (blob: Blob) => {
                this.creating.set(false);
                const url = URL.createObjectURL(blob);
                const a = document.createElement('a');
                a.href = url;
                a.download = `MaidsAndNanniesDb_${new Date().toISOString().replace(/[:.]/g, '-')}.bak`;
                a.click();
                URL.revokeObjectURL(url);
                this.messageService.add({ severity: 'success', detail: this.translate.instant('ADMIN_BACKUP.CREATE_OK') });
            },
            error: () => {
                this.creating.set(false);
                this.messageService.add({ severity: 'error', detail: this.translate.instant('ADMIN_BACKUP.CREATE_ERROR') });
            }
        });
    }

    confirmRestore() {
        this.confirmationService.confirm({
            message: this.translate.instant('ADMIN_BACKUP.RESTORE_CONFIRM'),
            header: this.translate.instant('ADMIN_BACKUP.RESTORE_TITLE'),
            icon: 'pi pi-exclamation-triangle',
            accept: () => this.restore()
        });
    }

    restore() {
        if (!this.selectedFile) return;
        this.restoring.set(true);
        this.api.restoreBackup(this.selectedFile).subscribe({
            next: () => {
                this.restoring.set(false);
                this.selectedFile = null;
                this.messageService.add({ severity: 'success', detail: this.translate.instant('ADMIN_BACKUP.RESTORE_OK') });
            },
            error: (err) => {
                this.restoring.set(false);
                this.messageService.add({ severity: 'error', detail: err?.error?.message || this.translate.instant('ADMIN_BACKUP.RESTORE_ERROR') });
            }
        });
    }
}