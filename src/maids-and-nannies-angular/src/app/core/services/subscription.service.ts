import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface SubscriptionDto {
  id: number;
  homeownerName: string;
  amount: number;
  startDate: string;
  endDate: string;
  isActive: boolean;
  daysRemaining: number;
  createdAt: string;
}

@Injectable({ providedIn: 'root' })
export class SubscriptionService {
  private readonly API = environment.apiUrl;

  constructor(private http: HttpClient) {}

  getMySubscriptions(): Observable<SubscriptionDto[]> {
    return this.http.get<SubscriptionDto[]>(`${this.API}/subscription/my`);
  }

  renewSubscription(id: number, formData: FormData): Observable<any> {
    return this.http.post(`${this.API}/subscription/${id}/renew`, formData);
  }

  getAllSubscriptions(): Observable<SubscriptionDto[]> {
    return this.http.get<SubscriptionDto[]>(`${this.API}/subscription/all`);
  }

  confirmRenewal(id: number): Observable<any> {
    return this.http.post(`${this.API}/subscription/${id}/confirm`, {});
  }
}