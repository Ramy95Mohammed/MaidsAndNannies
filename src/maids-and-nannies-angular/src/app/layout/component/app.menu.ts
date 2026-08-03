import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MenuItem } from 'primeng/api';
import { AppMenuitem } from './app.menuitem';
import { AuthService } from '../../core/services/auth.service';
import { TranslateService } from '@ngx-translate/core';

@Component({
    selector: 'app-menu',
    standalone: true,
    imports: [CommonModule, AppMenuitem, RouterModule],
    template: `<ul class="layout-menu">
        <ng-container *ngFor="let item of model; let i = index">
            <li app-menuitem *ngIf="!item.separator" [item]="item" [index]="i" [root]="true"></li>
            <li *ngIf="item.separator" class="menu-separator"></li>
        </ng-container>
    </ul> `
})
export class AppMenu {
    model: MenuItem[] = [];
    authService = inject(AuthService);
    private translate = inject(TranslateService);

    ngOnInit() {
        setTimeout(() => {
            this.buildMenu();
            
        }, 500);
    }

    buildMenu() {
        const user = this.authService.currentUser();
        const t = (k: string) => this.translate.instant(k);

        if (user?.role === 'Admin') {
            this.model.push({
                label: t('MENU.ADMIN'),
                items: [
                    { label: t('MENU.DASHBOARD'), icon: 'pi pi-fw pi-chart-bar', routerLink: ['/admin/dashboard'] },
                    { label: t('MENU.HOMEOWNERS'), icon: 'pi pi-fw pi-home', routerLink: ['/admin/homeowners'] },
                    { label: t('MENU.WORKERS'), icon: 'pi pi-fw pi-users', routerLink: ['/admin/workers'] },
                    { label: t('MENU.BOOKINGS'), icon: 'pi pi-fw pi-calendar', routerLink: ['/admin/bookings'] },
                    { label: t('MENU.PAYMENTS'), icon: 'pi pi-fw pi-dollar', routerLink: ['/admin/payments'] },
                    { label: t('MENU.SUBSCRIPTIONS'), icon: 'pi pi-fw pi-credit-card', routerLink: ['/admin/subscriptions'] },
                    { label: t('MENU.CURRENCIES'), icon: 'pi pi-fw pi-money-bill', routerLink: ['/admin/currencies'] },
                    { label: t('MENU.SETTINGS'), icon: 'pi pi-fw pi-cog', routerLink: ['/admin/settings'] },
                    { label: t('MENU.PASSWORD_RESET'), icon: 'pi pi-fw pi-key', routerLink: ['/admin/password-reset'] },
                    { label: t('MENU.JOB_POSTS'), icon: 'pi pi-fw pi-briefcase', routerLink: ['/admin/job-posts'] },
                    { label: t('MENU.REGISTER_HOMEOWNER'), icon: 'pi pi-fw pi-user-plus', routerLink: ['/admin/register-homeowner'] },
                ]
            });
        }

        if (user?.role === 'Homeowner') {
            this.model.push({
                label: t('MENU.HOMEOWNER'),
                items: [
                    { label: t('MENU.DASHBOARD'), icon: 'pi pi-fw pi-chart-bar', routerLink: ['/homeowner/dashboard'] },
                    { label: t('MENU.MY_PROFILE'), icon: 'pi pi-fw pi-user', routerLink: ['/homeowner/profile'] },
                    { label: t('MENU.SEARCH_WORKERS'), icon: 'pi pi-fw pi-search', routerLink: ['/homeowner/workers'] },
                    { label: t('JOB_POST.MY_POSTS'), icon: 'pi pi-fw pi-briefcase', routerLink: ['/homeowner/jobs'] },
                    { label: t('MENU.MY_BOOKINGS'), icon: 'pi pi-fw pi-calendar', routerLink: ['/homeowner/bookings'] },
                    { label: t('MENU.MY_SUBSCRIPTIONS'), icon: 'pi pi-fw pi-credit-card', routerLink: ['/homeowner/subscriptions'] },
                ]
            });
        }

        if (user?.role === 'Worker') {
            this.model.push({
                label: t('MENU.WORKER'),
                items: [
                    { label: t('MENU.DASHBOARD'), icon: 'pi pi-fw pi-chart-bar', routerLink: ['/worker/dashboard'] },
                    { label: t('MENU.MY_PROFILE'), icon: 'pi pi-fw pi-user', routerLink: ['/worker/profile'] },
                    { label: t('MENU.MY_BOOKINGS'), icon: 'pi pi-fw pi-calendar', routerLink: ['/worker/bookings'] },
                    { label: t('MENU.BROWSE_JOBS'), icon: 'pi pi-fw pi-briefcase', routerLink: ['/worker/jobs'] },
                    { label: t('JOB_POST.MY_APPLICATIONS'), icon: 'pi pi-fw pi-file', routerLink: ['/worker/applications'] },
                ]
            });
        }
    }
}