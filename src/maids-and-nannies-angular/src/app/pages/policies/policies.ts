import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { AccordionModule } from 'primeng/accordion';
import { ButtonModule } from 'primeng/button';
import { ApiService } from '../../core/services/api.service';
import { LanguageService } from '../../core/services/language.service';
import { TranslatePipe } from '@ngx-translate/core';
import { ProgressSpinnerModule } from 'primeng/progressspinner';


interface PolicyItem {
    key: string; titleAr: string; titleEn: string;
    contentAr: string; contentEn: string; sortOrder: number;
}

@Component({
    selector: 'app-policies',
    standalone: true,
    imports: [CommonModule, AccordionModule, ButtonModule, TranslatePipe, ProgressSpinnerModule],
    template: `
        <div class="bg-surface-50 dark:bg-surface-950 min-h-screen">
            <div class="max-w-3xl mx-auto p-6">
                <div class="flex justify-content-between align-items-center mb-4">
                    <h1 class="m-0 text-2xl">{{ 'POLICIES.TITLE' | translate }}</h1>
                    <p-button [label]="'POLICIES.BACK' | translate" icon="pi pi-arrow-left" [text]="true" (onClick)="goBack()"></p-button>
                </div>
                <p class="text-muted-color mb-4">{{ 'POLICIES.SUBTITLE' | translate }}</p>

                <div *ngIf="loading()" class="text-center py-6">
                    <p-progress-spinner  styleClass="w-4rem h-4rem"></p-progress-spinner>
                </div>

                <p-accordion *ngIf="!loading() && policies().length > 0">
                    @for (p of policies(); track $index) {
                         <p-accordion-panel [value]="$index">
                        <p-accordion-header>{{title(p)}}</p-accordion-header>   
                         <p-accordion-content>
                              <p style="white-space: pre-line">{{ content(p) }}</p>
                         </p-accordion-content>                     
                         </p-accordion-panel>
                    }                    
                </p-accordion>

                <div *ngIf="!loading() && policies().length === 0" class="text-muted-color">
                    {{ 'COMMON.ERROR' | translate }}
                </div>
            </div>
        </div>
    `
})
export class PoliciesComponent implements OnInit {
    private apiService = inject(ApiService);
    private router = inject(Router);
    private langService = inject(LanguageService);

    policies = signal<PolicyItem[]>([]);
    loading = signal(true);
    isAr = true;

    ngOnInit() {
        this.isAr = this.langService.getCurrentLanguage() === 'ar';
        this.apiService.getPolicies().subscribe({
            next: (data) => this.policies.set(data),
            error: () => this.loading.set(false),
            complete: () => this.loading.set(false)
        });
    }

    title(p: PolicyItem) { return this.isAr ? p.titleAr : p.titleEn; }
    content(p: PolicyItem) { return this.isAr ? p.contentAr : p.contentEn; }
    goBack() { this.router.navigate(['/auth/login']); }
}