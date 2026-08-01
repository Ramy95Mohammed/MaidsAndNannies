import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { TagModule } from 'primeng/tag';
import { ChipModule } from 'primeng/chip';
import { ToastModule } from 'primeng/toast';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { FormsModule } from '@angular/forms';
import { MessageService } from 'primeng/api';
import { TranslatePipe } from '@ngx-translate/core';
import { ApiService } from '../../../core/services/api.service';

@Component({
  selector: 'app-job-browse',
  standalone: true,
  imports: [CommonModule, RouterModule, CardModule, ButtonModule, TagModule, ChipModule, ToastModule, DialogModule, InputTextModule, FormsModule, TranslatePipe],
  providers: [MessageService],
  template: `
    <p-toast />
    <div class="card">
      <h2>{{ 'JOB_POST.BROWSE' | translate }}</h2>
      <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        <div *ngFor="let post of posts()" class="card cursor-pointer hover:shadow-lg transition-duration-300">
          <div class="mb-3">
            <p class="text-sm whitespace-pre-wrap line-height-3">{{ post.description | slice:0:150 }}...</p>
          </div>
          <div class="flex flex-wrap gap-2 mb-3">
            <!-- <p-chip [label]="getSpecLabel(post.specialization)"></p-chip> -->
            <p-chip [label]="getSpecLabel(post.specialization)"></p-chip>
            <p-chip *ngFor="let s of post.specializations || []" [label]="getSpecLabel(s)"></p-chip>
            
            <p-chip [label]="getTypeLabel(post.bookingType)"></p-chip>
          </div>
          <div class="flex align-items-center justify-content-between">
            <div>
              <!-- <span class="text-xl font-bold text-primary">{{ post.monthlySalary | currency:'EGP':'symbol':'1.0-0' }}</span> -->
              <span class="text-xl font-bold text-primary">{{ post.monthlySalary }} {{ post.currencyCode }}</span>
              <span class="text-muted-color text-sm">/ {{ post.bookingType === 0 ? 'يوم' : post.bookingType === 2 ? 'ساعة' : 'شهر' }}</span>
            </div>
            <p-button [label]="'WORKERS.BOOK' | translate" icon="pi pi-send" [rounded]="true" (onClick)="openApplyDialog(post)"></p-button>
          </div>
          <div class="text-sm text-muted-color mt-2">
            <i class="pi pi-calendar mr-1"></i>{{ post.startDate | date:'shortDate' }}
          </div>
        </div>
      </div>
      <div *ngIf="posts().length === 0" class="text-center py-8">
        <i class="pi pi-briefcase text-4xl text-muted-color mb-4"></i>
        <p class="text-muted-color">{{ 'COMMON.NO_DATA' | translate }}</p>
      </div>
    </div>

    <p-dialog [(visible)]="showDialog" [header]="'JOB_POST.APPLY' | translate" [modal]="true">
      <div class="mb-3">
        <label class="block font-bold mb-2">{{ 'JOB_POST.MESSAGE' | translate }}</label>
        <textarea pInputTextarea [(ngModel)]="applyMessage" rows="4" class="w-full" [placeholder]="'JOB_POST.MESSAGE_PLACEHOLDER' | translate"></textarea>
      </div>
      <p-button [label]="'JOB_POST.SUBMIT_APPLICATION' | translate" icon="pi pi-send" styleClass="w-full" (onClick)="submitApplication()"></p-button>
    </p-dialog>
  `
})
export class JobBrowse implements OnInit {
  private api = inject(ApiService);
  private msg = inject(MessageService);
  posts = signal<any[]>([]);
  showDialog = false;
  selectedPostId = 0;
  applyMessage = '';

  ngOnInit() { this.api.getApprovedJobPosts().subscribe({ next: (d) => this.posts.set(d) }); }

  openApplyDialog(post: any) { this.selectedPostId = post.id; this.applyMessage = ''; this.showDialog = true; }

  submitApplication() {
    this.api.applyForJob(this.selectedPostId, this.applyMessage).subscribe({
      next: () => { this.msg.add({ severity: 'success', detail: 'تم تقديم الطلب' }); this.showDialog = false; },
      error: () => this.msg.add({ severity: 'error', detail: 'فشل تقديم الطلب' })
    });
  }

  getSpecLabel(v: number) { return ['تنظيف', 'طبخ', 'رعاية أطفال', 'رعاية مسنين', 'عمل منزلي'][v] || ''; }
  getTypeLabel(t: number) { return ['يومي', 'شهري', 'ساعي'][t] || ''; }
}