import { Component, inject } from '@angular/core';
import { MenuItem } from 'primeng/api';
import { RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { StyleClassModule } from 'primeng/styleclass';
import { ButtonModule } from 'primeng/button';
import { AppConfigurator } from './app.configurator';
import { LayoutService } from '../service/layout.service';
import { AuthService } from '../../core/services/auth.service';
import { LanguageService } from '../../core/services/language.service';
import { TranslatePipe } from '@ngx-translate/core';

@Component({
    selector: 'app-topbar',
    standalone: true,
    imports: [RouterModule, CommonModule, StyleClassModule, AppConfigurator, ButtonModule, TranslatePipe],
    template: ` <div   class="layout-topbar" dir="ltr">
        <div class="layout-topbar-logo-container">
            <button class="layout-menu-button layout-topbar-action" (click)="layoutService.onMenuToggle()">
                <i class="pi pi-bars"></i>
            </button>
            <a class="layout-topbar-logo" routerLink="/">
                <span class="text-xl font-bold text-primary">{{ 'APP.NAME' | translate }}</span>
            </a>
        </div>

        <div  class="layout-topbar-actions">
            <div class="layout-config-menu flex items-center gap-2">
                <p-button [label]="langService.getCurrentLanguage() === 'ar' ? 'EN' : 'عربي'"
                          severity="secondary"
                          [text]="true"
                          (onClick)="langService.setLanguage(langService.getCurrentLanguage() === 'ar' ? 'en' : 'ar')">
                </p-button>

                <button type="button" class="layout-topbar-action" (click)="toggleDarkMode()">
                    <i [ngClass]="{ 'pi ': true, 'pi-moon': layoutService.isDarkTheme(), 'pi-sun': !layoutService.isDarkTheme() }"></i>
                </button>

                <div class="relative">
                    <button
                        class="layout-topbar-action layout-topbar-action-highlight"
                        pStyleClass="@next"
                        enterFromClass="hidden"
                        enterActiveClass="animate-scalein"
                        leaveToClass="hidden"
                        leaveActiveClass="animate-fadeout"
                        [hideOnOutsideClick]="true"
                    >
                        <i class="pi pi-palette"></i>
                    </button>
                    <app-configurator />
                </div>
            </div>

            <button class="layout-topbar-menu-button layout-topbar-action" pStyleClass="@next" enterFromClass="hidden" enterActiveClass="animate-scalein" leaveToClass="hidden" leaveActiveClass="animate-fadeout" [hideOnOutsideClick]="true">
                <i class="pi pi-ellipsis-v"></i>
            </button>

            <div class="layout-topbar-menu hidden lg:block">
                <div class="layout-topbar-menu-content">
                    <div *ngIf="authService.currentUser() as user" class="flex items-center gap-2 px-3">
                        <i class="pi pi-user"></i>
                        <span class="text-sm">{{ user.fullName }}</span>
                        <span class="text-xs text-muted-color">({{ user.role }})</span>
                    </div>
                    <button *ngIf="authService.isLoggedIn()" type="button" class="layout-topbar-action" (click)="authService.logout()">
                        <i class="pi pi-sign-out"></i>
                        <span>{{ 'AUTH.LOGOUT' | translate }}</span>
                    </button>
                    <a *ngIf="!authService.isLoggedIn()" type="button" class="layout-topbar-action" routerLink="/auth/login">
                        <i class="pi pi-sign-in"></i>
                        <span>{{ 'AUTH.LOGIN' | translate }}</span>
                    </a>
                </div>
            </div>
        </div>
    </div>`
})
export class AppTopbar {
    items!: MenuItem[];

    layoutService = inject(LayoutService);
    authService = inject(AuthService);
    langService = inject(LanguageService);

    toggleDarkMode() {
        this.layoutService.layoutConfig.update((state) => ({ ...state, darkTheme: !state.darkTheme }));
    }
}
