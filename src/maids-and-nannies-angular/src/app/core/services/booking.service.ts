import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface BookingListDto {
  id: number;
  workerName: string;
  workerId: number;
  serviceType: number;
  bookingType: number;
  quantity: number;
  currencyCode:string,
  startDate: string;
  monthlySalary: number;
  totalAmount: number;
  totalAmountAfterConversion: number;
  commissionAmount: number;
  status: number;
  isPaid: boolean;
  replacementCount: number;
  createdAt: string;
  hasReviewed: boolean;
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
  bookingType: number;
  quantity: number;
  currencyCode:string,
  startDate: string;
  endDate: string | null;
  monthlySalary: number;
  dailySalary:number,
  hourlySalary:number,
  totalAmount: number;
  totalAmountAfterConversion:number;
  commissionAmount: number;
  commissionType: number;
  status: any;
  isPaid: boolean;
  replacementCount: number;
  maxReplacement:number;
  maxFaultReplacement: number;
  maxPreferenceReplacement: number;
    paymentAmount: number;
  requirePaymentProof: boolean;
  outstandingAmount: number;
  adminNotes: string | null;
  createdAt: string;
  jobPostId:number;  
  hasReviewed: boolean;
}

export interface PagedResult<T> {
  data: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

@Injectable({ providedIn: 'root' })
export class BookingService {
  private readonly API_URL = environment.apiUrl;

  constructor(private http: HttpClient) {}

  // Homeowner
  createBooking(data: any): Observable<any> {
    return this.http.post(`${this.API_URL}/booking`, data);
  }

    getMyBookings(params?: any): Observable<PagedResult<BookingListDto>> {
    return this.http.get<PagedResult<BookingListDto>>(`${this.API_URL}/booking`, { params });
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

// reason: 0 = تقصير من العاملة (WorkerFault) — بدون عمولة إضافية
//         1 = رغبة شخصية من صاحبة المنزل (HomeownerPreference) — عمولة جديدة عن الفترة المتبقية
requestReplacement(
  id: number,
  reason: 0 | 1,
  newWorkerId?: number | null,
  applicationId?: number | null
): Observable<any> {
  return this.http.post(`${this.API_URL}/booking/${id}/replace`, { newWorkerId, applicationId, reason });
}

  // Worker
   // Worker
  getWorkerBookings(params?: any): Observable<PagedResult<BookingListDto>> {
    return this.http.get<PagedResult<BookingListDto>>(`${this.API_URL}/booking/worker`, { params });
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

    reviewBooking(id: number, rating: number, comment: string | null): Observable<any> {
    return this.http.post(`${this.API_URL}/booking/${id}/review`, { rating, comment });
  }
}