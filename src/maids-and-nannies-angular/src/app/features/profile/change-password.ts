import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { PasswordModule } from 'primeng/password';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { AuthService } from '../../core/services/auth.service';

@Component({
    selector: 'app-change-password',
    standalone: true,
    imports: [CommonModule, FormsModule, ButtonModule, PasswordModule, ToastModule, TranslatePipe],
    providers: [MessageService],
    template: `
        <p-toast></p-toast>
        <div class="card">
            <h3>{{ 'CHANGE_PASSWORD.TITLE' | translate }}</h3>
            <div class="flex flex-column gap-3">
                <div>
                    <label class="block font-bold mb-1">{{ 'CHANGE_PASSWORD.CURRENT' | translate }}</label>
                    <p-password [(ngModel)]="currentPassword" [toggleMask]="true" [feedback]="false" [fluid]="true"></p-password>
                </div>
                <div>
                    <label class="block font-bold mb-1">{{ 'CHANGE_PASSWORD.NEW' | translate }}</label>
                    <p-password [(ngModel)]="newPassword" [toggleMask]="true" [feedback]="false" [fluid]="true"></p-password>
                </div>
                <div>
                    <label class="block font-bold mb-1">{{ 'CHANGE_PASSWORD.CONFIRM' | translate }}</label>
                    <p-password [(ngModel)]="confirmPassword" [toggleMask]="true" [feedback]="false" [fluid]="true"></p-password>
                </div>
                <p-button [label]="'CHANGE_PASSWORD.SAVE' | translate" icon="pi pi-key" (onClick)="save()" [loading]="saving()" styleClass="w-fit"></p-button>
            </div>
        </div>
    `
})
export class ChangePasswordComponent {
    currentPassword = '';
    newPassword = '';
    confirmPassword = '';
    saving = signal(false);

    private authService = inject(AuthService);
    private messageService = inject(MessageService);
    private translate = inject(TranslateService);

    save() {
        if (!this.currentPassword || !this.newPassword || !this.confirmPassword) {
            this.messageService.add({ severity: 'warn', summary: this.translate.instant('COMMON.ERROR'), detail: this.translate.instant('CHANGE_PASSWORD.FILL_ALL') });
            return;
        }
        if (this.newPassword !== this.confirmPassword) {
            this.messageService.add({ severity: 'warn', summary: this.translate.instant('COMMON.ERROR'), detail: this.translate.instant('CHANGE_PASSWORD.MISMATCH') });
            return;
        }
        if (this.newPassword.length < 8) {
            this.messageService.add({ severity: 'warn', summary: this.translate.instant('COMMON.ERROR'), detail: this.translate.instant('CHANGE_PASSWORD.TOO_SHORT') });
            return;
        }

        this.saving.set(true);
        this.authService.changePassword(this.currentPassword, this.newPassword).subscribe({
            next: () => {
                this.saving.set(false);
                this.currentPassword = ''; this.newPassword = ''; this.confirmPassword = '';
                this.messageService.add({ severity: 'success', summary: this.translate.instant('COMMON.SUCCESS'), detail: this.translate.instant('CHANGE_PASSWORD.SUCCESS') });
            },
            error: (err) => {
                this.saving.set(false);
                this.messageService.add({ severity: 'error', summary: this.translate.instant('COMMON.ERROR'), detail: err.error?.message || this.translate.instant('COMMON.ERROR') });
            }
        });
    }
}