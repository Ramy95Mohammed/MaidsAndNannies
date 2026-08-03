import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { ToastModule } from 'primeng/toast';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { SelectModule } from 'primeng/select';
import { FormsModule } from '@angular/forms';
import { MessageService } from 'primeng/api';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { ApiService } from '../../../core/services/api.service';
import { Textarea } from 'primeng/textarea';

@Component({
  selector: 'app-admin-job-posts',
  standalone: true,
  imports: [CommonModule, CardModule, ButtonModule, TableModule, TagModule, ToastModule,
    DialogModule, InputTextModule, Textarea, SelectModule, FormsModule, TranslatePipe],
  providers: [MessageService],
  template: `
    <p-toast />
    <div class="card">
      <h2>{{ 'JOB_POST.REVIEW_TITLE' | translate }}</h2>
          <p-table [value]="posts()" [rows]="10" [paginator]="true">
        <ng-template #header>
          <tr>
            <th>{{ 'COMMON.ID' | translate }}</th>
            <th>{{ 'ADMIN.TABLE_HOMEOWNER' | translate }}</th>
            <th>{{ 'JOB_POST.DESCRIPTION' | translate }}</th>
            <th>{{ 'BOOKING.TYPE' | translate }}</th>
            <th>{{ 'ADMIN.TABLE_SALARY' | translate }}</th>
            <th>{{ 'JOB_POST.DATE' | translate }}</th>
            <th>{{ 'ADMIN.TABLE_ACTIONS' | translate }}</th>
          </tr>
        </ng-template>
        <ng-template #body let-p>
          <tr>
            <td>{{ p.id }}</td>
            <td>{{ p.homeownerName }}</td>
            <td>{{ p.description | slice:0:80 }}...</td>
            <td>{{ getTypeLabel(p.bookingType) }}</td>
            <td>{{ p.monthlySalary }} {{ p.currencyCode }}</td>            
            <td>{{ p.createdAt | date:'short' }}</td>
            <td><p-button [label]="'JOB_POST.REVIEW' | translate" icon="pi pi-eye" size="small" (onClick)="openReview(p)"></p-button></td>
          </tr>
        </ng-template>
      </p-table>
    </div>

    <p-dialog [(visible)]="showDialog" [header]="'JOB_POST.REVIEW' | translate" [modal]="true" [style]="{ width: '600px' }">
      <div class="mb-3">
        <label class="block font-bold mb-2">{{ 'JOB_POST.ORIGINAL_DESC' | translate }}</label>
        <p class="p-3 border-1 border-round whitespace-pre-wrap">{{ selectedPost?.description }}</p>
      </div>
      <div class="mb-3">
        <label class="block font-bold mb-2">{{ 'JOB_POST.SANITIZED_DESC' | translate }}</label>
        <textarea pInputTextarea [(ngModel)]="sanitizedDescription" rows="5" class="w-full"></textarea>
        <small class="text-muted-color">{{ 'JOB_POST.SANITIZED_HINT' | translate }}</small>
      </div>
      <div class="flex gap-2">
        <p-button [label]="'JOB_POST.APPROVE' | translate" icon="pi pi-check" severity="success" styleClass="flex-1" (onClick)="review(true)"></p-button>
        <p-button [label]="'ADMIN.REJECT' | translate" icon="pi pi-times" severity="danger" styleClass="flex-1" (onClick)="review(false)"></p-button>
      </div>
    </p-dialog>
  `
})
export class AdminJobPosts implements OnInit {
  private api = inject(ApiService);
  private msg = inject(MessageService);
  private translate = inject(TranslateService);
  posts = signal<any[]>([]);
  showDialog = false;
  selectedPost: any = null;
  sanitizedDescription = '';

  ngOnInit() { this.load(); }

  load() { this.api.getPendingJobPosts().subscribe({ next: (d) => this.posts.set(d) }); }

  openReview(post: any) {
    this.selectedPost = post;
    this.sanitizedDescription = post.description;
    this.showDialog = true;
  }

  review(approved: boolean) {
    this.api.reviewJobPost(this.selectedPost.id, {
      sanitizedDescription: this.sanitizedDescription,
      isApproved: approved,
      rejectionReason: approved ? null : this.translate.instant('JOB_POST.REJECTED_BY_ADMIN')
    }).subscribe({
      next: () => {
        this.msg.add({ severity: 'success', detail: approved ? this.translate.instant('JOB_POST.APPROVED') : this.translate.instant('JOB_POST.REJECTED') });
        this.showDialog = false;
        this.load();
      },
      error: () => this.msg.add({ severity: 'error', detail: this.translate.instant('JOB_POST.REVIEW_FAILED') })
    });
  }

  getTypeLabel(t: number) { return [this.translate.instant('WORKER_DETAIL.DAILY'), this.translate.instant('WORKER_DETAIL.MONTHLY'), this.translate.instant('WORKER_DETAIL.HOURLY')][t] || '—'; }
}