import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';
import { AuthService } from '../../../core/services/auth.service';
import { ApiService } from '../../../core/services/api.service';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';

@Component({
    selector: 'app-worker-dashboard',
    standalone: true,
    imports: [CommonModule, RouterModule, CardModule, ButtonModule, TableModule, TagModule, ToastModule , TranslatePipe],
    providers: [MessageService],
    template: `
        <p-toast></p-toast>
        <div class="grid grid-cols-12 gap-8">
            <div class="col-span-12">
                <h2>{{ 'WORKER_DASHBOARD.WELCOME' | translate:{name: authService.currentUser()?.fullName} }}</h2>
            </div>

            <div class="col-span-12 md:col-span-4">
                <p-card styleClass="mb-0">
                    <div class="flex align-items-center gap-4">
                        <div class="flex align-items-center justify-content-center w-12 h-12 border-round bg-green-100">
                            <i class="pi pi-calendar text-green-500 text-xl"></i>
                        </div>
                        <div>
                            <span class="text-muted-color text-sm">{{ 'WORKER_DASHBOARD.COMPLETED_BOOKINGS' | translate }}</span>
                            <div class="text-lg font-bold">{{ completedBookings().length }}</div>
                        </div>
                    </div>
                </p-card>
            </div>

            <div class="col-span-12 md:col-span-4">
                <p-card styleClass="mb-0" routerLink="/worker/bookings">
                    <div class="flex align-items-center gap-4">
                        <div class="flex align-items-center justify-content-center w-12 h-12 border-round bg-orange-100">
                            <i class="pi pi-clock text-orange-500 text-xl"></i>
                        </div>
                        <div>
                            <span class="text-muted-color text-sm">{{ 'WORKER_DASHBOARD.NEW_REQUESTS' | translate }}</span>
                            <div class="text-lg font-bold">{{ confirmations().length }}</div>
                        </div>
                    </div>
                </p-card>
            </div>

            <div class="col-span-12 md:col-span-4">
                <p-card styleClass="mb-0">
                    <div class="flex align-items-center gap-4">
                        <div class="flex align-items-center justify-content-center w-12 h-12 border-round bg-purple-100">
                            <i class="pi pi-star text-purple-500 text-xl"></i>
                        </div>
                        <div>
                            <span class="text-muted-color text-sm">{{ 'WORKER_DASHBOARD.RATING' | translate }}</span>
                            <div class="text-lg font-bold">{{ rating() }}</div>
                        </div>
                    </div>
                </p-card>
            </div>

            <div class="col-span-12">
                <p-card>
                    <ng-template #header>
                        <div class="flex align-items-center justify-content-between px-4 pt-4">
                            <h5 class="m-0">{{ 'WORKER_DASHBOARD.NEW_REQUESTS' | translate }}</h5>
                            <p-button [label]="'WORKER_DASHBOARD.EDIT_PROFILE' | translate" routerLink="/worker/profile" size="small" [text]="true"></p-button>
                        </div>
                    </ng-template>
                    <p-table [value]="confirmations()" [rows]="5" [paginator]="true" [tableStyle]="{ 'min-width': '40rem' }" *ngIf="confirmations().length > 0">
                        <ng-template #header>
                            <tr>
                                <th>{{ 'WORKER_DASHBOARD.HOMEOWNER' | translate }}</th>
                                <th>{{ 'WORKER_DASHBOARD.DATE' | translate }}</th>
                                <!-- <th>{{ 'COMMON.ACTIONS' | translate }}</th> -->
                            </tr>
                        </ng-template>
                        <ng-template #body let-booking>
                            <tr>
                                <td>{{ booking.homeownerName }}</td>
                                <td>{{ booking.startDate | date:'fullDate' }}</td>
                                <!-- <td>
                                    <p-button icon="pi pi-check" [rounded]="true" [outlined]="true" class="mr-2" severity="success" [title]="'WORKER_DASHBOARD.ACCEPT' | translate" (click)="acceptBooking(booking.id)"></p-button>
                                    <p-button icon="pi pi-times" [rounded]="true" [outlined]="true" severity="danger" [title]="'WORKER_DASHBOARD.REJECT' | translate" (click)="rejectBooking(booking.id)"></p-button>
                                </td> -->
                            </tr>
                        </ng-template>
                    </p-table>
                    <p *ngIf="confirmations().length === 0" class="text-muted-color p-3">{{ 'WORKER_DASHBOARD.NO_PENDING' | translate }}</p>
                </p-card>
            </div>
        </div>
    `
})
export class WorkerDashboard implements OnInit {
    authService = inject(AuthService);
    private apiService = inject(ApiService);
    private messageService = inject(MessageService);
    private translate = inject(TranslateService);

    confirmations = signal<any[]>([]);
    activeBookings = signal<any[]>([]);
    completedBookings = signal<any[]>([]);
    rating = signal('-');

    ngOnInit() {
        this.loadBookings();
        this.loadRating();
    }

    loadBookings() {
        this.apiService.getWorkerBookings({ page: 1, pageSize: 50 }).subscribe({
            next: (res: any) => {
                const all: any[] = Array.isArray(res?.data) ? res.data : [];
                this.confirmations.set(all.filter(b => b.status === 0));
                this.activeBookings.set(all.filter(b => b.status === 4));
                this.completedBookings.set(all.filter(b => b.status === 5));
            },
            error: () => {
                this.confirmations.set([]);
                this.completedBookings.set([]);
            }
        });
    }

    loadRating() {
        this.apiService.getWorkerProfile().subscribe({
            next: (p: any) => this.rating.set(p?.averageRating ? Number(p.averageRating).toFixed(1) : '-'),
            error: () => this.rating.set('-')
        });
    }

    acceptBooking(id: number) {
        this.apiService.updateBookingStatus(id, 1).subscribe({
            next: () => {
                this.messageService.add({ severity: 'success', detail: this.translate.instant('WORKER_DASHBOARD.ACCEPT_OK') });
                this.loadBookings();
            },
            error: () => this.messageService.add({ severity: 'error', detail: this.translate.instant('COMMON.ERROR') })
        });
    }

    rejectBooking(id: number) {
        this.apiService.updateBookingStatus(id, 6).subscribe({
            next: () => {
                this.messageService.add({ severity: 'warn', detail: this.translate.instant('WORKER_DASHBOARD.REJECT_OK') });
                this.loadBookings();
            },
            error: () => this.messageService.add({ severity: 'error', detail: this.translate.instant('COMMON.ERROR') })
        });
    }
}