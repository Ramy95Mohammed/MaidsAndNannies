import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import {ButtonModule} from 'primeng/button';
import {CheckboxModule} from 'primeng/checkbox';
import {InputTextModule} from 'primeng/inputtext';
import {PasswordModule} from 'primeng/password';
import {RippleModule} from 'primeng/ripple';
import {MessageModule} from 'primeng/message';
import {TranslatePipe} from '@ngx-translate/core';
import {AppFloatingConfigurator} from '../../layout/component/app.floatingconfigurator';
import {AuthService} from '../../core/services/auth.service';
import {LanguageService} from '../../core/services/language.service';

@Component({
    selector: 'app-login',
    standalone: true,
    imports: [CommonModule, ButtonModule, CheckboxModule, InputTextModule, PasswordModule, FormsModule, RouterModule, RippleModule, MessageModule, TranslatePipe, AppFloatingConfigurator],
    template: `
        <app-floating-configurator />
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
                                <span class="font-medium no-underline ml-2 text-right cursor-pointer text-primary">{{ 'AUTH.FORGOT_PASSWORD' | translate }}</span>
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
                        </div>
                    </div>
                </div>
            </div>
        </div>
    `
})
export class Login {
    email: string = '';
    password: string = '';
    checked: boolean = false;
    isLoading: boolean = false;
    errorMessage: string = '';

    private authService = inject(AuthService);
    langService = inject(LanguageService);
    private router = inject(Router);

    login() {
        if (!this.email || !this.password) {
            this.errorMessage = 'Please fill in all fields';
            return;
        }

        this.isLoading = true;
        this.errorMessage = '';

        this.authService.login(this.email, this.password).subscribe({
            next: (response) => {
                console.log('user Response' , response);
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
                this.errorMessage = error.error?.message || 'Login failed';
            }
        });
    }
}
