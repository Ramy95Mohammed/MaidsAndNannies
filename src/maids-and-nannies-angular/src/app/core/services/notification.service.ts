import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { Observable } from 'rxjs';

export interface NotificationItem {
    id: number;
    type: string;
    title: string;
    message: string;
    isRead: boolean;
    createdAt: string;
}

@Injectable({ providedIn: 'root' })
export class NotificationService {
    private readonly API_URL = environment.apiUrl;
    private http = inject(HttpClient);

    getNotifications(): Observable<NotificationItem[]> {
        return this.http.get<NotificationItem[]>(`${this.API_URL}/notifications`);
    }

    getUnreadCount(): Observable<{ count: number }> {
        return this.http.get<{ count: number }>(`${this.API_URL}/notifications/unread-count`);
    }

    markRead(id: number): Observable<{ message: string }> {
        return this.http.post<{ message: string }>(`${this.API_URL}/notifications/${id}/read`, {});
    }

    markAllRead(): Observable<{ message: string }> {
        return this.http.post<{ message: string }>(`${this.API_URL}/notifications/read-all`, {});
    }
}