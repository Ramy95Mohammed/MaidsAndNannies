import { Component, OnDestroy, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { StyleClassModule } from 'primeng/styleclass';
import { AuthService } from '../services/auth.service';
import { NotificationItem, NotificationService } from '../services/notification.service';

@Component({
    selector: 'app-notifications-bell',
    standalone: true,
    imports: [CommonModule, StyleClassModule, TranslatePipe],
    template: `
    <div class="relative" *ngIf="authService.isLoggedIn()">
        <button type="button" class="layout-topbar-action relative" pStyleClass="@next"
                enterFromClass="hidden" enterActiveClass="animate-scalein"
                leaveToClass="hidden" leaveActiveClass="animate-fadeout"
                [hideOnOutsideClick]="true" (click)="refresh()">
            <i class="pi pi-bell"></i>
            <span *ngIf="unreadCount() > 0" class="notification-badge">{{ unreadCount() }}</span>
        </button>

        <div class="notification-panel hidden">
            <div class="notification-panel-header">
                <span class="font-bold">{{ 'NOTIF.TITLE' | translate }}</span>
                <button type="button" class="text-sm text-primary" (click)="markAllRead()">
                    {{ 'NOTIF.MARK_ALL_READ' | translate }}
                </button>
            </div>
            <div class="notification-list">
                <div *ngFor="let n of notifications()" class="notification-item"
                     [class.unread]="!n.isRead" (click)="open(n)">
                    <div>{{ n.title | translate: getParams(n) }}</div>
                    <div class="notification-time">{{ timeAgo(n.createdAt) }}</div>
                </div>
                <p *ngIf="notifications().length === 0" class="notification-empty">
                    {{ 'NOTIF.EMPTY' | translate }}
                </p>
            </div>
        </div>
    </div>`
})
export class NotificationsBell implements OnInit, OnDestroy {
    private notificationService = inject(NotificationService);
    public authService = inject(AuthService);
    private translate = inject(TranslateService);

    notifications = signal<NotificationItem[]>([]);
    unreadCount = signal(0);
    private timer: ReturnType<typeof setInterval> | undefined;

    ngOnInit() {
        this.refresh();
        this.timer = setInterval(() => this.refresh(), 60000);
    }

    ngOnDestroy() {
        if (this.timer) clearInterval(this.timer);
    }

    refresh() {
        if (!this.authService.isLoggedIn()) return;
        this.notificationService.getUnreadCount().subscribe(r => this.unreadCount.set(r.count));
        this.notificationService.getNotifications().subscribe(list => this.notifications.set(list));
    }

    getParams(n: NotificationItem): Record<string, unknown> {
        try {
            const raw = n.message ? JSON.parse(n.message) : {};
            const out: Record<string, unknown> = {};
            for (const k of Object.keys(raw)) {
                out[k.charAt(0).toLowerCase() + k.slice(1)] = raw[k];
            }
            return out;
        } catch {
            return {};
        }
    }

    open(n: NotificationItem) {
        if (!n.isRead) {
            this.notificationService.markRead(n.id).subscribe(() => {
                n.isRead = true;
                this.unreadCount.update(c => Math.max(0, c - 1));
            });
        }
    }

    markAllRead() {
        this.notificationService.markAllRead().subscribe(() => this.refresh());
    }

    timeAgo(iso: string): string {       
        const diff =  Date.now() - new Date(iso).getTime();
        const mins = Math.floor(diff / 60000);
        if (mins < 1) return this.translate.instant('NOTIF.JUST_NOW');
        if (mins < 60) return this.translate.instant('NOTIF.MINUTES_AGO', { count: mins });
        const hours = Math.floor(mins / 60);
        if (hours < 24) return this.translate.instant('NOTIF.HOURS_AGO', { count: hours });
        return this.translate.instant('NOTIF.DAYS_AGO', { count: Math.floor(hours / 24) });
    }

   
}