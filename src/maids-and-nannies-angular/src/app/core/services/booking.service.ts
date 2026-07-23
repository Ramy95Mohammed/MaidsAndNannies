import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface BookingListDto {
  id: number;
  workerName: string;
  workerId: number;
  serviceType: number;
  startDate: string;
  monthlySalary: number;
  commissionAmount: number;
  status: number;
  isPaid: boolean;
  replacementCount: number;
  createdAt: string;
}

export interface BookingDetailDto {
  id: number;
  homeownerId: string;
  homeownerName: string;
  homeownerPhone: string | null;
  workerId: string;
  workerFullName: string | null;
  workerPhone: string | null;
  workerWhatsApp: string | null;
  workerProfileImage: string | null;
  workerNationalityId: number | null;
  serviceType: number;
  startDate: string;
  endDate: string | null;
  monthlySalary: number;
  commissionAmount: number;
  commissionType: number;
  status: number;
  isPaid: boolean;
  replacementCount: number;
  adminNotes: string | null;
  createdAt: string;
}

@Injectable({ providedIn: 'root' })
export class BookingService {
  private readonly API_URL = environment.apiUrl;

  constructor(private http: HttpClient) {}

  // Homeowner
  createBooking(data: any): Observable<any> {
    return this.http.post(`${this.API_URL}/booking`, data);
  }

  getMyBookings(): Observable<BookingListDto[]> {
    return this.http.get<BookingListDto[]>(`${this.API_URL}/booking`);
  }

  getBookingById(id: number): Observable<BookingDetailDto> {
    return this.http.get<BookingDetailDto>(`${this.API_URL}/booking/${id}`);
  }

  cancelBooking(id: number): Observable<any> {
    return this.http.post(`${this.API_URL}/booking/${id}/cancel`, {});
  }

  uploadPaymentProof(id: number, formData: FormData): Observable<any> {
    return this.http.post(`${this.API_URL}/booking/${id}/upload-proof`, formData);
  }

  requestReplacement(id: number, newWorkerId: number): Observable<any> {
    return this.http.post(`${this.API_URL}/booking/${id}/replace`, { newWorkerId });
  }

  // Worker
  getWorkerBookings(): Observable<BookingListDto[]> {
    return this.http.get<BookingListDto[]>(`${this.API_URL}/booking/worker`);
  }

  // Admin
  confirmWorker(id: number): Observable<any> {
    return this.http.post(`${this.API_URL}/booking/${id}/confirm-worker`, {});
  }

  requestPayment(id: number): Observable<any> {
    return this.http.post(`${this.API_URL}/booking/${id}/request-payment`, {});
  }

  confirmPayment(id: number): Observable<any> {
    return this.http.post(`${this.API_URL}/booking/${id}/confirm-payment`, {});
  }

  startWork(id: number): Observable<any> {
    return this.http.post(`${this.API_URL}/booking/${id}/start`, {});
  }

  completeWork(id: number): Observable<any> {
    return this.http.post(`${this.API_URL}/booking/${id}/complete`, {});
  }
}