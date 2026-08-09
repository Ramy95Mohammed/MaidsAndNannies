import { Component, inject, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import {ButtonModule} from 'primeng/button';
import {CheckboxModule} from 'primeng/checkbox';
import {InputTextModule} from 'primeng/inputtext';
import {PasswordModule} from 'primeng/password';
import {RippleModule} from 'primeng/ripple';
import {MessageModule} from 'primeng/message';
import {TranslatePipe, TranslateService} from '@ngx-translate/core';
import {AppFloatingConfigurator} from '../../layout/component/app.floatingconfigurator';
import {AuthService} from '../../core/services/auth.service';
import {LanguageService} from '../../core/services/language.service';
import { Toast } from "primeng/toast";
import { MessageService } from 'primeng/api';
import { Dialog } from "primeng/dialog";

@Component({
    selector: 'app-login',
    standalone: true,
    imports: [CommonModule, ButtonModule, CheckboxModule, InputTextModule, PasswordModule, FormsModule, RouterModule, RippleModule, MessageModule, TranslatePipe, AppFloatingConfigurator, Toast, Dialog],
    template: `
        <app-floating-configurator />
        <p-toast></p-toast>
        <div class="bg-surface-50 dark:bg-surface-950 flex items-center justify-center min-h-screen min-w-screen overflow-hidden">
            <div class="flex flex-col items-center justify-center">
                <div style="border-radius: 56px; padding: 0.3rem; background: linear-gradient(180deg, var(--primary-color) 10%, rgba(33, 150, 243, 0) 30%)">
                    <div class="w-full bg-surface-0 dark:bg-surface-900 py-20 px-8 sm:px-20" style="border-radius: 53px">
                        <div class="text-center mb-8">
                            <div class="text-surface-900 dark:text-surface-0 text-3xl font-medium mb-4">{{ 'APP.NAME' | translate }}</div>
                            <span class="text-muted-color font-medium">{{ 'APP.TAGLINE' | translate }}</span>
                        </div>

                        <div *ngIf="errorMessage" class="mb-4">
                            <p-message severity="error" [text]="errorMessage"></p-message>
                        </div>

                        <div>
                            <label for="email1" class="block text-surface-900 dark:text-surface-0 text-xl font-medium mb-2">{{ 'AUTH.EMAIL' | translate }}</label>
                            <input pInputText id="email1" type="text" [placeholder]="'AUTH.EMAIL' | translate" class="w-full md:w-120 mb-8" [(ngModel)]="email" />

                            <label for="password1" class="block text-surface-900 dark:text-surface-0 font-medium text-xl mb-2">{{ 'AUTH.PASSWORD' | translate }}</label>
                            <p-password id="password1" [(ngModel)]="password" [placeholder]="'AUTH.PASSWORD' | translate" [toggleMask]="true" styleClass="mb-4" [fluid]="true" [feedback]="false"></p-password>

                            <div class="flex items-center justify-between mt-2 mb-8 gap-8">
                                <div class="flex items-center">
                                    <p-checkbox [(ngModel)]="checked" id="rememberme1" binary class="mr-2"></p-checkbox>
                                    <label for="rememberme1">{{ 'AUTH.REMEMBER_ME' | translate }}</label>
                                </div>
                                <!-- <span class="font-medium no-underline ml-2 text-right cursor-pointer text-primary">{{ 'AUTH.FORGOT_PASSWORD' | translate }}</span> -->
                                                                 <span (click)="forgotDialogVisible = true" class="font-medium no-underline ml-2 text-right cursor-pointer text-primary">{{ 'AUTH.FORGOT_PASSWORD' | translate }}</span>
                            </div>
                            <p-button [label]="'AUTH.SIGN_IN' | translate" styleClass="w-full" (onClick)="login()" [loading]="isLoading"></p-button>

                            <div class="text-center mt-6">
                                <span class="text-muted-color">{{ 'AUTH.NO_ACCOUNT' | translate }}</span>
                                <a routerLink="/auth/register" class="font-medium text-primary ml-2 cursor-pointer">{{ 'AUTH.REGISTER' | translate }}</a>
                            </div>

                            <div class="flex justify-center mt-4 gap-2">
                                <p-button [label]="'LANGUAGE.AR' | translate" [outlined]="langService.getCurrentLanguage() !== 'ar'" severity="secondary" size="small" (onClick)="langService.setLanguage('ar')"></p-button>
                                <p-button [label]="'LANGUAGE.EN' | translate" [outlined]="langService.getCurrentLanguage() !== 'en'" severity="secondary" size="small" (onClick)="langService.setLanguage('en')"></p-button>
                            </div>
                           
                            <div class="text-center mt-4">
                                <a (click)="goToPolicies()" class="text-primary cursor-pointer font-medium">{{ 'POLICIES.LINK' | translate }}</a>
                            </div>

                        </div>
                    </div>
                </div>
            </div>

                        <p-dialog [(visible)]="forgotDialogVisible" [header]="'FORGOT.TITLE' | translate" [modal]="true" styleClass="w-full md:w-30rem">
                <div *ngIf="forgotStep === 1">
                    <p class="text-muted-color mb-3">{{ 'FORGOT.EMAIL_HINT' | translate }}</p>
                    <label class="block font-bold mb-1">{{ 'AUTH.EMAIL' | translate }}</label>
                    <input pInputText type="email" class="w-full mb-3" [(ngModel)]="forgotEmail" />
                    <p-button [label]="'FORGOT.SEND_CODE' | translate" styleClass="w-full" (onClick)="sendCode()" [loading]="forgotSending"></p-button>
                </div>
                <div *ngIf="forgotStep === 2">
                    <p class="text-muted-color mb-3">{{ 'FORGOT.CODE_HINT' | translate }}</p>
                    <label class="block font-bold mb-1">{{ 'FORGOT.CODE' | translate }}</label>
                    <input pInputText type="text" maxlength="6" class="w-full mb-3" [(ngModel)]="resetCode" />
                    <label class="block font-bold mb-1">{{ 'FORGOT.NEW_PASSWORD' | translate }}</label>
                    <p-password [(ngModel)]="resetNewPassword" [toggleMask]="true" [feedback]="false" [fluid]="true"></p-password>
                    <label class="block font-bold mb-1 mt-3">{{ 'FORGOT.CONFIRM' | translate }}</label>
                    <p-password [(ngModel)]="resetConfirmPassword" [toggleMask]="true" [feedback]="false" [fluid]="true"></p-password>
                    <p-button [label]="'FORGOT.RESET' | translate" styleClass="w-full mt-4" (onClick)="resetPassword()" [loading]="forgotSending"></p-button>
                </div>
            </p-dialog>

        </div>
    `
})
export class Login{
    email: string = '';
    password: string = '';
    checked: boolean = false;
    isLoading: boolean = false;
    errorMessage: string = '';

        forgotDialogVisible = false;
    forgotStep = 1;
    forgotEmail = '';
    resetCode = '';
    resetNewPassword = '';
    resetConfirmPassword = '';
    forgotSending = false;

    private authService = inject(AuthService);
    langService = inject(LanguageService);
    private router = inject(Router);
    private translate = inject(TranslateService);     
   private messageService = inject(MessageService);

     login() {
        if (!this.email || !this.password) {
            this.errorMessage = this.translate.instant('AUTH.FILL_ALL_FIELDS');  
            return;
        }

        this.isLoading = true;
        this.errorMessage = '';

        this.authService.login(this.email, this.password).subscribe({
            next:   (response) => {                
                this.isLoading = false;
                const role = response.role;
                if (role === 'Admin') {
                    this.router.navigate(['/admin/dashboard']);
                } else if (role === 'Homeowner') {
                    this.router.navigate(['/homeowner/dashboard']);
                } else if (role === 'Worker') {
                    this.router.navigate(['/worker/dashboard']);
                } else {
                    this.router.navigate(['/']);
                }
            },
            error: (error) => {
                this.isLoading = false;
                this.errorMessage = error.error?.message || this.translate.instant('AUTH.LOGIN_FAILED');
            }
        });
    }

        sendCode() {
        if (!this.forgotEmail) {
            this.messageService.add({ severity: 'warn', summary: this.translate.instant('COMMON.ERROR'), detail: this.translate.instant('FORGOT.EMAIL_REQUIRED') });
            return;
        }
        this.forgotSending = true;
        this.authService.forgotPassword(this.forgotEmail).subscribe({
            next: () => {
                this.forgotSending = false;
                this.forgotStep = 2;
                this.messageService.add({ severity: 'success', summary: this.translate.instant('COMMON.SUCCESS'), detail: this.translate.instant('FORGOT.SENT_SUCCESS') });
            },
            error: () => {
                this.forgotSending = false;
                this.messageService.add({ severity: 'error', summary: this.translate.instant('COMMON.ERROR'), detail: this.translate.instant('FORGOT.SENT_SUCCESS') });
            }
        });
    }

    resetPassword() {
        if (!this.resetCode) {
            this.messageService.add({ severity: 'warn', summary: this.translate.instant('COMMON.ERROR'), detail: this.translate.instant('FORGOT.CODE_REQUIRED') });
            return;
        }
        if (this.resetNewPassword !== this.resetConfirmPassword) {
            this.messageService.add({ severity: 'warn', summary: this.translate.instant('COMMON.ERROR'), detail: this.translate.instant('FORGOT.MISMATCH') });
            return;
        }
        if (this.resetNewPassword.length < 8) {
            this.messageService.add({ severity: 'warn', summary: this.translate.instant('COMMON.ERROR'), detail: this.translate.instant('FORGOT.TOO_SHORT') });
            return;
        }
        this.forgotSending = true;
        this.authService.resetPassword(this.forgotEmail, this.resetCode, this.resetNewPassword).subscribe({
            next: () => {
                this.forgotSending = false;
                this.forgotDialogVisible = false;
                this.forgotStep = 1;
                this.forgotEmail = ''; this.resetCode = ''; this.resetNewPassword = ''; this.resetConfirmPassword = '';
                this.messageService.add({ severity: 'success', summary: this.translate.instant('COMMON.SUCCESS'), detail: this.translate.instant('FORGOT.RESET_SUCCESS') });
            },
            error: (err) => {
                this.forgotSending = false;
                this.messageService.add({ severity: 'error', summary: this.translate.instant('COMMON.ERROR'), detail: err.error?.message || this.translate.instant('COMMON.ERROR') });
            }
        });
    }

        goToPolicies() { this.router.navigate(['/policies']); }
}
