import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MenuItem } from 'primeng/api';
import { AppMenuitem } from './app.menuitem';
import { AuthService } from '../../core/services/auth.service';
import { LanguageService } from '../../core/services/language.service';

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
    langService = inject(LanguageService);

    ngOnInit() {
        this.buildMenu();
    }

    buildMenu() {
        const user = this.authService.currentUser();
        const isAr = this.langService.getCurrentLanguage() === 'ar';
    
        
        if (user?.role === 'Admin') {
            this.model.push({
                label: isAr ? 'الإدارة' : 'Admin',
                items: [
                    { label: isAr ? 'لوحة التحكم' : 'Dashboard', icon: 'pi pi-fw pi-chart-bar', routerLink: ['/admin/dashboard'] },
                    { label: isAr ? 'الصاحبات' : 'Homeowners', icon: 'pi pi-fw pi-home', routerLink: ['/admin/homeowners'] },
                    { label: isAr ? 'العاملات' : 'Workers', icon: 'pi pi-fw pi-users', routerLink: ['/admin/workers'] },
                    { label: isAr ? 'الحجوزات' : 'Bookings', icon: 'pi pi-fw pi-calendar', routerLink: ['/admin/bookings'] },
                    { label: isAr ? 'المدفوعات' : 'Payments', icon: 'pi pi-fw pi-dollar', routerLink: ['/admin/payments'] },
                    { label: isAr ? 'الاشتراكات' : 'Subscriptions', icon: 'pi pi-fw pi-credit-card', routerLink: ['/admin/subscriptions'] },
                ]
            });
        }

        if (user?.role === 'Homeowner') {
            this.model.push({
                label: isAr ? 'صاحبة المنزل' : 'Homeowner',
                items: [
                    { label: isAr ? 'لوحة التحكم' : 'Dashboard', icon: 'pi pi-fw pi-chart-bar', routerLink: ['/homeowner/dashboard'] },
                    { label: isAr ? 'ملفي الشخصي' : 'My Profile', icon: 'pi pi-fw pi-user', routerLink: ['/homeowner/profile'] },
                    { label: isAr ? 'بحث عن عاملة' : 'Search Workers', icon: 'pi pi-fw pi-search', routerLink: ['/homeowner/workers'] },
                    { label: isAr ? 'حجوزاتي' : 'My Bookings', icon: 'pi pi-fw pi-calendar', routerLink: ['/homeowner/bookings'] },                    
                    { label: isAr ? 'اشتراكاتي' : 'My Subscriptions', icon: 'pi pi-fw pi-credit-card', routerLink: ['/homeowner/subscriptions'] },
                ]
            });
        }

        if (user?.role === 'Worker') {
            this.model.push({
                label: isAr ? 'العاملة' : 'Worker',
                items: [
                    { label: isAr ? 'لوحة التحكم' : 'Dashboard', icon: 'pi pi-fw pi-chart-bar', routerLink: ['/worker/dashboard'] },
                    { label: isAr ? 'ملفي الشخصي' : 'My Profile', icon: 'pi pi-fw pi-user', routerLink: ['/worker/profile'] },
                    { label: isAr ? 'حجوزاتي' : 'My Bookings', icon: 'pi pi-fw pi-calendar', routerLink: ['/worker/bookings'] }
                ]
            });
        }
        
    }
}
